using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.Barcode;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.Barcode;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class DispatchService(
    VSSMDbContext context,
    DispatchScanSessionService dispatchScan,
    IArrivalService arrivalService,
    IGs1BarcodeParser barcodeParser) : IDispatchService
{

    private async Task<List<int>> GetUserStoreIdsAsync(int recipientId)
    {
        var storeIds = await context.TblStores
            .Where(s => s.RecipientId == recipientId)
            .Select(s => s.StoreId)
            .ToListAsync();

        return storeIds;
    }
    public async Task<List<DispatchEditLineDto>> GetDispatchLinesForEditAsync(int dispatchId, CancellationToken ct = default)
    {
        var rawLines = await (
            from line in context.TblDispatchLines.Where(l => l.DispatchId == dispatchId).AsNoTracking()

            join item in context.TblItems on line.ItemId equals item.ItemId

            join ids in context.TblItemDetailStores
                on line.ItemDetailStoreId equals ids.PKItemDetailStoreId
                into idsJoin
            from ids in idsJoin.DefaultIfEmpty()

            join store in context.TblStores on line.StoreId equals store.StoreId
                into storeJoin
            from store in storeJoin.DefaultIfEmpty()

            join manufacturer in context.TblBaseInfo on line.ManufacturerId equals manufacturer.BaseInfoId
                into mfrJoin
            from manufacturer in mfrJoin.DefaultIfEmpty()

            join vvmStage in context.TblBaseInfo on line.FkVvmStageBaseInfo equals vvmStage.BaseInfoId
                into vvmJoin
            from vvmStage in vvmJoin.DefaultIfEmpty()

            select new
            {
                line.DispatchLineId,
                ItemDetailStoreId = line.ItemDetailStoreId ?? 0,
                ItemId = line.ItemId ?? 0,
                ItemName = item.FullName,
                line.BatchNumber,
                line.ExpiryDate,
                StoreId = line.StoreId ?? 0,
                StoreName = store != null ? store.Name : "-",
                ManufacturerName = manufacturer != null ? manufacturer.Title : "-",
                DoseCount = line.Quantity ?? 0,
                NumVial = line.NumVial ?? 0,
                VvmStageId = line.FkVvmStageBaseInfo ?? 0,
                VvmStageName = vvmStage != null ? vvmStage.Title : "-",
                line.Note,
                CurrentStock = ids != null ? (ids.CurrentStock ?? 0) : 0,
            }
        ).ToListAsync(ct);

        if (rawLines.Count == 0) return new List<DispatchEditLineDto>();

        var scannedUids = await context.TblDispatchUids
            .AsNoTracking()
            .Where(du => du.FKDispatchId == dispatchId)
            .Select(du => new
            {
                ItemId = du.UidInfo.OrderInfo.FkItemId,
                du.UidInfo.OrderInfo.BatchNumber,
                Uid = du.UidInfo.FirstLevelUid
            })
            .ToListAsync(ct);

        var uidLookup = scannedUids
            .GroupBy(u => (u.ItemId, u.BatchNumber))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Uid).ToList());

        return rawLines.Select(r => new DispatchEditLineDto
        {
            DispatchLineId = r.DispatchLineId,
            ItemDetailStoreId = r.ItemDetailStoreId,
            ItemId = r.ItemId,
            ItemName = r.ItemName,
            BatchNumber = r.BatchNumber,
            ExpiryDateShamsi = r.ExpiryDate.HasValue ? r.ExpiryDate.Value.ToShamsi() : null,
            ExpiryDate = r.ExpiryDate,
            StoreId = r.StoreId,
            StoreName = r.StoreName,
            ManufacturerName = r.ManufacturerName,
            DoseCount = r.DoseCount,
            NumVial = r.NumVial,
            VvmStageId = r.VvmStageId,
            VvmStageName = r.VvmStageName,
            Note = r.Note,
            CurrentStock = r.CurrentStock,
            Uids = uidLookup.TryGetValue((r.ItemId, r.BatchNumber), out var uids) ? uids : new List<string>()
        }).ToList();
    }
    public async Task<List<ItemLookupDto>> GetItemsByClassAsync(int classId, CancellationToken ct = default)
    {
        return await context.TblItems
            .Where(i => i.FkClassId == classId && (i.IsActive == null || i.IsActive == true))
            .Select(i => new ItemLookupDto
            {
                ItemId = i.ItemId,
                FullName = i.FullName,
            })
            .OrderBy(i => i.FullName)
            .ToListAsync(ct);
    }

    public async Task<List<BatchRowDto>> GetBatchesByItemAsync(
        int itemId,
        int recipientId,
        CancellationToken ct = default)
    {
        List<int> allowedStoreIds = new List<int>();
        bool isAdmin = (recipientId == 0);

        if (!isAdmin)
        {
            allowedStoreIds = await context.TblStores
                .Where(s => s.RecipientId == recipientId)
                .Select(s => s.StoreId)
                .ToListAsync(ct);

            if (allowedStoreIds.Count == 0)
                return new List<BatchRowDto>();
        }

        var item = await context.TblItems
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);

        if (item == null)
            return new List<BatchRowDto>();

        TblItem? diluentItem = null;
        TblItemDetailStore? diluentStore = null;

        if (item.RequiresDiluent.HasValue && item.RequiresDiluent.Value && item.DiluentId.HasValue)
        {
            diluentItem = await context.TblItems
                .FirstOrDefaultAsync(i => i.ItemId == item.DiluentId.Value, ct);

            if (diluentItem != null)
            {
                diluentStore = await context.TblItemDetailStores
                    .Include(s => s.ItemDetail)
                    .Where(s => s.ItemDetail.ItemId == diluentItem.ItemId && s.CurrentStock > 0)
                    .OrderByDescending(s => s.ItemDetailId)
                    .FirstOrDefaultAsync(ct);
            }
        }

        var query = from ids in context.TblItemDetailStores
                        .Where(s => s.ItemDetail.ItemId == itemId && s.CurrentStock > 0)
                    join id in context.TblItemDetails on ids.ItemDetailId equals id.ItemDetailId
                    join mfr in context.TblBaseInfo on id.ManufacturerId equals mfr.BaseInfoId into mfrJ
                    from mfr in mfrJ.DefaultIfEmpty()
                    join dpv in context.TblBaseInfo on id.DosePerVialId equals dpv.BaseInfoId into dpvJ
                    from dpv in dpvJ.DefaultIfEmpty()
                    join store in context.TblStores on ids.StoreId equals store.StoreId into storeJ
                    from store in storeJ.DefaultIfEmpty()
                    select new
                    {
                        ids.PKItemDetailStoreId,
                        StoreId = ids.StoreId ?? 0,      // ← تبدیل int? به int
                        id.BatchNumber,
                        id.ExpiryDate,
                        CurrentStock = ids.CurrentStock ?? 0,
                        DosePerVialStr = dpv != null ? dpv.Title : "-",
                        DosePerVialNum = dpv != null ? int.Parse(dpv.Title) : 0,
                        ManufacturerName = mfr != null ? mfr.Title : "-",
                        StoreName = store != null ? store.Name : "-",
                    };

        if (!isAdmin)
        {
            query = query.Where(x => allowedStoreIds.Contains(x.StoreId));
        }

        var rawRows = await query.ToListAsync(ct);

        var result = rawRows.Select(r => new BatchRowDto
        {
            ItemDetailStoreId = r.PKItemDetailStoreId,
            ItemId = item.ItemId,
            ItemName = item.FullName,
            BatchNumber = r.BatchNumber,
            ExpiryDateShamsi = r.ExpiryDate.HasValue ? r.ExpiryDate.Value.ToShamsi() : null,
            ExpiryDate = r.ExpiryDate,
            CurrentStock = (int)r.CurrentStock,
            DosePerVial = r.DosePerVialStr,
            DosePerVialNum = r.DosePerVialNum,
            ManufacturerName = r.ManufacturerName,
            StoreId = r.StoreId,
            StoreName = r.StoreName,
            RequiresDiluent = item.RequiresDiluent.HasValue && item.RequiresDiluent.Value,
            DiluentItemDetailStoreId = diluentStore?.PKItemDetailStoreId,
            DiluentItemId = diluentItem?.ItemId,
            DiluentItemName = diluentItem?.FullName,
            DiluentBatchNumber = diluentStore?.ItemDetail?.BatchNumber,
            DiluentExpiryDateShamsi = diluentStore?.ItemDetail?.ExpiryDate.HasValue == true
                                        ? diluentStore.ItemDetail.ExpiryDate.Value.ToShamsi()
                                        : null,
            DiluentExpiryDate = diluentStore?.ItemDetail?.ExpiryDate,
            DiluentManufacturerName = null,
        }).ToList();

        return result;
    }
    // ──────────────────────────────────────────────────────────────────────────
    // بند ۱۰a + بند ۹: کسر CurrentStock از tblItemDetailStores هنگام ثبت نهایی
    // این متد پس از CreateDispatchAsync یا UpdateDispatchAsync صدا زده می‌شود
    // ──────────────────────────────────────────────────────────────────────────
    public async Task DecrementStockOnFinalAsync(List<DispatchLineDto> lines, CancellationToken ct = default)
    {
        foreach (var line in lines)
        {
            var store = await context.TblItemDetailStores.FindAsync(line.ItemDetailStoreId);
            if (store == null) continue;

            store.CurrentStock = (store.CurrentStock ?? 0) - line.DoseCount;
            if (store.CurrentStock < 0) store.CurrentStock = 0;
        }

        await context.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // بند ۱۰c: برگشت موجودی هنگام حذف dispatch یا dispatch lines
    // در DeleteDispatchAsync این متد نیز فراخوانی شود
    // ──────────────────────────────────────────────────────────────────────────
    public async Task RestoreStockAsync(int dispatchId, CancellationToken ct = default)
    {
        var lines = await context.TblDispatchLines
            .Where(l => l.DispatchId == dispatchId)
            .ToListAsync(ct);

        foreach (var line in lines)
        {
            if (line.ItemDetailStoreId == null) continue;
            var store = await context.TblItemDetailStores.FindAsync(line.ItemDetailStoreId);
            if (store == null) continue;

            store.CurrentStock = (store.CurrentStock ?? 0) + (line.Quantity ?? 0);
        }

        await context.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // بند ۹: بروزرسانی FK_DISPATCH_UID در tblUidInfo هنگام ثبت نهایی
    // برای هر UID انتخاب‌شده، فیلد FKDispatchUid با PK جدول tblDispatchUid
    // بروز می‌شود
    // ──────────────────────────────────────────────────────────────────────────
    public async Task LinkUidsToDispatchUidAsync(int dispatchId, CancellationToken ct = default)
    {
        var dispatchUids = await context.TblDispatchUids
            .Where(du => du.FKDispatchId == dispatchId)
            .ToListAsync(ct);

        foreach (var du in dispatchUids)
        {
            var uidInfo = await context.TblUidInfo.FindAsync(du.FKUidInfo);
            if (uidInfo == null) continue;

            uidInfo.FKDispatchUid = du.PKDispatchUid;
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task ConfirmDispatchAsync(int id, CancellationToken cancellationToken)
    {
        var dispatch = await context.TblDispatches.FindAsync(id);

        if (dispatch.IsConfirmed.HasValue && dispatch.IsConfirmed.Value)
            return;

        dispatch.IsConfirmed = true;
        dispatch.IsDrafted = false;
        dispatch.ConfirmationTime = DateTime.Now;
        dispatch.DispatchVoucherNumber = (int.Parse(dispatch.DispatchVoucherNumber) + 1).ToString();

        await context.SaveChangesAsync(cancellationToken);

    }

    public async Task UpdateDispatchAsync(UpdateDispatchDto updateDispatcheDto, string submitType, CancellationToken cancellationToken)
    {
        var dispatch = await context.TblDispatches.FindAsync(updateDispatcheDto.Id);
        if (dispatch == null)
            throw new ApplicationException("ارسال یافت نشد.");

        dispatch.SenderId = updateDispatcheDto.SenderId;
        dispatch.ReceiverId = updateDispatcheDto.ReceiverId;
        dispatch.TransportMode = updateDispatcheDto.TransportMode;
        dispatch.EnteredById = updateDispatcheDto.EnteredById;
        dispatch.RequestNum = updateDispatcheDto.RequestNum;
        dispatch.Note = updateDispatcheDto.Note;
        dispatch.Date = string.IsNullOrWhiteSpace(updateDispatcheDto.Date) ? null : DateTimeHelper.ToGregorian(updateDispatcheDto.Date);
        dispatch.ReceivingMode = updateDispatcheDto.ReceivingMode;
        dispatch.ReportNumber = updateDispatcheDto.ReportNumber;

        if (submitType == "draft")
        {
            dispatch.IsDrafted = true;
            dispatch.IsConfirmed = false;
            dispatch.ConfirmationTime = null;
        }
        else if (submitType == "final")
        {
            dispatch.IsDrafted = false;
            dispatch.IsConfirmed = true;
            dispatch.ConfirmationTime = DateTime.Now;

            if (long.TryParse(dispatch.DispatchVoucherNumber, out long currentNumber))
            {
                dispatch.DispatchVoucherNumber = (currentNumber + 1).ToString();
            }
            else
            {
                dispatch.DispatchVoucherNumber = "1";
            }
            dispatch.SeqPreview = 0;
        }

        var oldLines = await context.TblDispatchLines
            .Where(l => l.DispatchId == updateDispatcheDto.Id)
            .ToListAsync(cancellationToken);

        if (oldLines.Any())
        {
            context.TblDispatchLines.RemoveRange(oldLines);
        }

        var oldItemIds = oldLines.Select(l => l.ItemId ?? 0).ToHashSet();
        var newItemIds = updateDispatcheDto.Items.Select(i => i.ItemId).ToHashSet();
        var removedItemIds = oldItemIds.Except(newItemIds).ToList();

        if (removedItemIds.Any())
        {
            var dispatchUids = await context.TblDispatchUids
                .Where(du => du.FKDispatchId == updateDispatcheDto.Id)
                .Include(du => du.UidInfo)
                    .ThenInclude(ui => ui.OrderInfo)
                .ToListAsync(cancellationToken);

            var uidsToRemove = dispatchUids
                .Where(du => du.UidInfo?.OrderInfo?.FkItemId != null &&
                              removedItemIds.Contains(du.UidInfo.OrderInfo.FkItemId))
                .ToList();

            if (uidsToRemove.Any())
            {
                context.TblDispatchUids.RemoveRange(uidsToRemove);
            }
        }

        foreach (var item in updateDispatcheDto.Items)
        {
            var itemDetailStore = await context.TblItemDetailStores
                .FindAsync(item.ItemDetailStoreId);

            if (itemDetailStore == null)
                continue; 

            var itemDetail = await context.TblItemDetails
                .Include(t => t.DosePerVial)
                .FirstOrDefaultAsync(t => t.ItemDetailId == itemDetailStore.ItemDetailId);

            if (itemDetail == null)
                continue;

            int numvial = 0;
            if (itemDetail.DosePerVial != null &&
                int.TryParse(itemDetail.DosePerVial.Title, out int dosePerVial))
            {
                numvial = item.DoseCount / dosePerVial;
            }

            var line = new TblDispatchLine
            {
                NumVial = numvial,
                DispatchId = updateDispatcheDto.Id,
                ItemId = item.ItemId,
                Quantity = item.DoseCount,
                DosePerVialId = itemDetail.DosePerVialId,
                StoreId = item.StoreId,
                FkVvmTypeBaseInfo = itemDetail.VvmTypeId,
                FkVvmStageBaseInfo = item.VvmstageId,
                ManufacturerId = itemDetail.ManufacturerId,
                ItemDetailStoreId = item.ItemDetailStoreId,
                Note = item.Note,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate,
                EntryDate = DateTime.Now
            };

            context.TblDispatchLines.Add(line);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new ApplicationException($"خطا در ذخیره تغییرات: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<(int dispatchId, long seqPreview)> CreateDispatchAsync(
        CreateDispatchDto createDispatcheDto, 
        string submitType, 
        CancellationToken cancellationToken)
    {
        var fiscalPeriod = await context.TblFiscalPeriods.FirstOrDefaultAsync(f => f.IsActive.Value);

        var reportNumber = await GenerateReportNumberAsync();

        var dispatch = new TblDispatch
        {
            SenderId = createDispatcheDto.SenderId,
            ReceiverId = createDispatcheDto.ReceiverId,
            FiscalPeriodId = fiscalPeriod.FiscalPeriodId,
            TransportMode = createDispatcheDto.TransportMode,
            EnteredById = createDispatcheDto.EnteredById,
            RequestNum = createDispatcheDto.RequestNum,
            Note = createDispatcheDto.Note,
            Date = string.IsNullOrWhiteSpace(createDispatcheDto.Date) ? null : DateTimeHelper.ToGregorian(createDispatcheDto.Date),
            EntryDate = DateTime.Now,
            IsDrafted = submitType == "draft" ? true : false,
            ReceivingMode = createDispatcheDto.ReceivingMode,
            IsCancelled = false,
            IsConfirmed = submitType == "final" ? true : false,
            DispatchVoucherPrinted = false,
            ConfirmationTime = submitType == "final" ? DateTime.Now : null,
            ReceiverStatus = 0,
            DispatchVoucherNumber = createDispatcheDto.DispatchVoucherNumber.ToString(),
            ReportNumber = reportNumber
        };

        float totalPrice = 0;
        float totalVolume = 0;

        foreach (var item in createDispatcheDto.Items)
        {
            var itemInfo = await context.TblItems.FindAsync(item.ItemId);
            var itemDetailStore = await context.TblItemDetailStores.FindAsync(item.ItemDetailStoreId);
            var itemDetail = await context.TblItemDetails.FindAsync(itemDetailStore.ItemDetailId);


            var line = new TblDispatchLine
            {
                ItemId = item.ItemId,
                Quantity = item.DoseCount,
                DosePerVialId = itemDetail.DosePerVialId,
                StoreId = item.StoreId,
                FkVvmTypeBaseInfo = itemDetail.VvmTypeId,
                FkVvmStageBaseInfo = item.VvmstageId,
                ManufacturerId = itemDetail.ManufacturerId,
                ItemDetailStoreId = item.ItemDetailStoreId,
                Note = item.Note,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate,
                EntryDate = DateTime.Now
            };

            totalVolume += item.Volume ?? 0;

            dispatch.TblDispatchLines.Add(line);
        }

        var result = await context.TblDispatches.AddAsync(dispatch, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return (dispatch.DispatchId, dispatch.SeqPreview);
    }

    public async Task DeleteDispatchAsync(int id, CancellationToken cancellationToken)
    {
        var dispatchUids = await context.TblDispatchUids
            .Where(du => du.FKDispatchId == id)
            .ToListAsync(cancellationToken);

        if (dispatchUids.Any())
        {
            var uidInfoIds = dispatchUids.Select(du => du.FKUidInfo).ToList();
            var uidInfos = await context.TblUidInfo
                .Where(u => uidInfoIds.Contains(u.PkUidInfo))
                .ToListAsync(cancellationToken);

            foreach (var uid in uidInfos)
            {
                uid.FKDispatchUid = null; 
            }

            context.TblDispatchUids.RemoveRange(dispatchUids);
        }

        var dispatch = await context.TblDispatches.FindAsync(id);
        if (dispatch != null)
        {
            context.TblDispatches.Remove(dispatch);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DispatchArrivedAsync(
        DispatchArrivedDto arrivedDto,
        int userId,
        int recipientId,
        CancellationToken cancellationToken)
    {
        if (arrivedDto?.Lines == null || !arrivedDto.Lines.Any())
            throw new ArgumentException("هیچ خطی برای ورود وجود ندارد.");

        var lineIds = arrivedDto.Lines.Select(t => t.DispatchLineId).ToList();
        var storeMap = arrivedDto.Lines.ToDictionary(l => l.DispatchLineId, l => l.StoreId);

        var fiscalPeriodId = await context.TblFiscalPeriods
            .Where(f => f.IsActive.Value)
            .Select(f => f.FiscalPeriodId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fiscalPeriodId == 0)
            throw new InvalidOperationException("دوره مالی فعالی یافت نشد. لطفاً ابتدا یک دوره مالی فعال تعریف کنید.");

        var data = await (
            from line in context.TblDispatchLines.Where(l => lineIds.Contains(l.DispatchLineId))
            join dispatch in context.TblDispatches
                .Where(d => d.ReceiverStatus == 0 && d.ReceiverId == recipientId && d.IsConfirmed == true)
                on line.DispatchId equals dispatch.DispatchId
            join ids in context.TblItemDetailStores
                on line.ItemDetailStoreId equals ids.PKItemDetailStoreId
            join id in context.TblItemDetails
                on ids.ItemDetailId equals id.ItemDetailId
            select new
            {
                line.DispatchLineId,
                dispatch.DispatchId,
                dispatch.SenderId,
                line.BatchNumber,
                id.CommercialNameId,
                line.DosePerVialId,
                line.ExpiryDate,
                id.ItemDetailId,
                line.ItemId,
                line.ManufacturerId,
                id.ProductionDate,
                line.Quantity,
                line.FkUseBaseInfo,
                line.FkVvmStageBaseInfo,
                line.FkVvmTypeBaseInfo,
            }).ToListAsync(cancellationToken);

        if (data.Count == 0)
            throw new InvalidOperationException("هیچ داده‌ای برای ورود یافت نشد.");

        var missingStores = lineIds.Except(storeMap.Keys).ToList();
        if (missingStores.Any())
            throw new ArgumentException($"برای خطوط {string.Join(", ", missingStores)} انبار مقصد مشخص نشده است.");

        var first = data.First();
        var now = DateTime.Now;

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var receiveDispatch = new TblReceiveDispatches
            {
                ReceiveDate = DateTime.Today,
                CreationTime = now,
                CreatedBy = userId
            };
            context.TblReceiveDispatches.Add(receiveDispatch);
            await context.SaveChangesAsync(cancellationToken);

            var arrival = new TblArrival
            {
                Date = now,
                FiscalPeriodId = fiscalPeriodId,
                EntryDate = now,
                EnteredById = userId,
                IsConfirmed = true,
                IsDrafted = false,
                ConfirmationTime = now,
                SenderId = first.SenderId,
                ReceiverId = recipientId,
                DispatchId = first.DispatchId,
                ReportNumber = await arrivalService.Generate()
            };

            var itemDetailStores = new List<TblItemDetailStore>(data.Count);

            foreach (var item in data)
            {
                var newStoreId = storeMap[item.DispatchLineId];

                arrival.ArrivalLines.Add(new TblArrivalLine
                {
                    BatchNum = item.BatchNumber,
                    CommertialNameId = item.CommercialNameId,
                    DosePerVialId = item.DosePerVialId,
                    EntryDate = now,
                    ExpiryDate = item.ExpiryDate,
                    ItemDetailId = item.ItemDetailId,
                    ItemId = item.ItemId,
                    ManufacturerId = item.ManufacturerId,
                    ProductionDate = item.ProductionDate,
                    Quantity = item.Quantity,
                    StoreId = newStoreId,
                    FkUseId = item.FkUseBaseInfo,
                    FkVvmStageId = item.FkVvmStageBaseInfo,
                    FkVvmTypeId = item.FkVvmTypeBaseInfo,
                });

                itemDetailStores.Add(new TblItemDetailStore
                {
                    ItemDetailId = item.ItemDetailId,
                    StoreId = newStoreId,
                    CurrentStock = item.Quantity,
                    EntryDate = now
                });
            }

            context.TblItemDetailStores.AddRange(itemDetailStores);
            context.TblArrivals.Add(arrival);
            await context.SaveChangesAsync(cancellationToken);


            var batchToStoreMap = data.ToDictionary(
                d => d.BatchNumber,
                d => storeMap[d.DispatchLineId]
            );

            var dispatchIds = data.Select(d => d.DispatchId).Distinct().ToHashSet();

            var dispatchUids = await context.TblDispatchUids
                .Where(du => dispatchIds.Contains(du.FKDispatchId))
                .ToListAsync(cancellationToken);

            var uidIds = dispatchUids.Select(du => du.FKUidInfo).ToList();

            var uids = await context.TblUidInfo
                .Where(u => uidIds.Contains(u.PkUidInfo) /*&& u.FkReceiveDispatchId == null*/)
                .Include(u => u.OrderInfo)
                .ToListAsync(cancellationToken);

            if (uids.Any())
            {
                foreach (var uid in uids)
                {
                    if (uid.OrderInfo != null && batchToStoreMap.TryGetValue(uid.OrderInfo.BatchNumber, out var newStoreId))
                    {
                        uid.FkReceiveDispatchId = receiveDispatch.PkReceiveDispatchesId;
                        uid.CurrentStore = newStoreId;
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }

                var currentDispatch = await context.TblDispatches
                    .FirstOrDefaultAsync(d => d.DispatchId == first.DispatchId, cancellationToken);

                if (currentDispatch != null)
                {
                    currentDispatch.ArrivalId = arrival.Id;
                    currentDispatch.ReceiverStatus = 2;
                    currentDispatch.FkReceiveDispatchId = receiveDispatch.PkReceiveDispatchesId;
                    await context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    public async Task<UpdateDispatchDto?> GetByIdAsync(int id)
    {
        var dispatch = await context.TblDispatches.FindAsync(id);
        if (dispatch is null) return null;

        var items = from line in context.TblDispatchLines.Where(t => t.DispatchId == id)
                    join item in context.TblItems on line.ItemId equals item.ItemId
                    join vvmstage in context.TblBaseInfo on line.FkVvmStageBaseInfo equals vvmstage.BaseInfoId into vvmstages
                    from vvmstage in vvmstages.DefaultIfEmpty()
                    select new { line, item, vvmstage };

        var model = new UpdateDispatchDto
        {
            IsConfirmed = dispatch.IsConfirmed ?? false,
            IsDrafted = dispatch.IsDrafted ?? false,
            Id = dispatch.DispatchId,
            Date = dispatch.Date?.Date.ToShamsi(),
            RequestNum = dispatch.RequestNum,
            DispatchVoucherNumber = ulong.TryParse(dispatch.DispatchVoucherNumber, out ulong res) ? res : 0,
            ReportNumber = dispatch.ReportNumber,
            ReceiverId = dispatch.ReceiverId ?? 0,
            TransportMode = dispatch.TransportMode,
            ReceivingMode = dispatch.ReceivingMode,
            Note = dispatch.Note,
            Items = items.Select(i => new DispatchLineDto
            {
                DispatchLineId = i.line.DispatchLineId,
                ItemDetailStoreId = i.line.ItemDetailStoreId,
                FkClassId = i.item.FkClassId ?? 0,
                ItemName = i.item.FullName,
                ItemId = i.line.ItemId ?? 0,
                VvmstageName = i.vvmstage.Title,
                ExpiryDate = i.line.ExpiryDate,
                DoseCount = i.line.Quantity ?? 0,
                VvmstageId = i.line.FkVvmStageBaseInfo ?? 0,
                Note = i.line.Note,
                BatchNumber = i.line.BatchNumber,
                StoreId = i.line.StoreId

            }).ToList()
        };

        return model;
    }

    public Task<List<ToBeArrivedDispatchLineDto>> GetDispatchLinesAsync(long dispatchId, CancellationToken cancellationToken)
    {
        var query = from dispatchLine in context.TblDispatchLines
                        .Where(dl => dl.DispatchId == dispatchId)
                        .AsNoTracking()

                    join itemDetailStore in context.TblItemDetailStores
                        on dispatchLine.ItemDetailStoreId equals itemDetailStore.PKItemDetailStoreId
                    join itemDetail in context.TblItemDetails
                        on itemDetailStore.ItemDetailId equals itemDetail.ItemDetailId
                    join item in context.TblItems
                        on dispatchLine.ItemId equals item.ItemId

                    join manufacturer in context.TblBaseInfo
                        on dispatchLine.ManufacturerId equals manufacturer.BaseInfoId
                        into manufacturerJoin
                    from manufacturer in manufacturerJoin.DefaultIfEmpty()

                    join doesPerVial in context.TblBaseInfo
                        on dispatchLine.DosePerVialId equals doesPerVial.BaseInfoId
                        into doesPerVialJoin
                    from doesPerVial in doesPerVialJoin.DefaultIfEmpty()

                    join vvmStage in context.TblBaseInfo
                        on dispatchLine.FkVvmStageBaseInfo equals vvmStage.BaseInfoId
                        into vvmStageJoin
                    from vvmStage in vvmStageJoin.DefaultIfEmpty()

                    select new ToBeArrivedDispatchLineDto
                    {
                        DispatchLineId = dispatchLine.DispatchLineId,
                        ItemId = dispatchLine.ItemId,
                        UseOrdinaryStore = item.UseOrdinaryStore,
                        UserAboveZeroStore = item.UserAboveZeroStore,
                        UserBelowZeroStore = item.UserBelowZeroStore,
                        ItemName = item.FullName,
                        BatchNumber = itemDetail.BatchNumber,
                        DoseCount = dispatchLine.Quantity,
                        DoesPerVial = int.Parse(doesPerVial.Title),
                        ExpiryDate = dispatchLine.ExpiryDate,
                        Manufacure = manufacturer.Title,
                        Vvmstage = vvmStage.Title
                    };

        return query.ToListAsync(cancellationToken);
    }

    public async Task<(List<DispatchListDto> data, int count)> GetPagedDispatchesAsync(string? search, int page, int pageSize,
    string sortField, string sortOrder, string? status, int senderId, CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        // base query using joins
        var query = from d in context.TblDispatches.Where(dispatch => dispatch.SenderId == senderId)
                    join r in context.TblRecipients on d.ReceiverId equals r.RecipientId into rj
                    from r in rj.DefaultIfEmpty()
                    select new DispatchListDto
                    {
                        Id = d.DispatchId,
                        Date = d.Date,
                        IsConfirmed = d.IsConfirmed,
                        DispatchVoucherNumber = d.DispatchVoucherNumber,
                        RequestNum = d.RequestNum,
                        IsDrafted = d.IsDrafted,
                        ReceiverStatus = d.ReceiverStatus ?? 0,
                        SeqPreview = d.SeqPreview,
                        ReceiverName = r != null ? r.FullName : "-",
                    };

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "draft")
                query = query.Where(x => x.IsDrafted == true);

            else if (status == "final")
                query = query.Where(x => x.IsConfirmed == true);
        }

        query = query.OrderByDescending(t => t.Id).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.RequestNum.Contains(search) ||
                d.ReceiverName.Contains(search)
            );
        }

        var total = await query.CountAsync();

        query = query.ApplySorting(sortField, sortOrder == "desc");

        return (await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(), total);
    }

    public async Task<(List<ToBeArrivedDispatchListDto> data, int count)> GetPagedToBeArrivedDispatchesAsync(string? search, int page, int pageSize,
        string sortField, string sortOrder, int receiverId, CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        // base query using joins
        var query = from d in context.TblDispatches.Where(dispatch => dispatch.ReceiverStatus == 0 &&
                            dispatch.ReceiverId == receiverId && dispatch.IsConfirmed.HasValue && dispatch.IsConfirmed.Value)
                    join r in context.TblRecipients on d.SenderId equals r.RecipientId into rj
                    from r in rj.DefaultIfEmpty()
                    select new ToBeArrivedDispatchListDto
                    {
                        Id = d.DispatchId,
                        Date = d.Date,
                        DispatchVoucherNumber = d.DispatchVoucherNumber,
                        RequestNum = d.RequestNum,
                        EntryDate = d.EntryDate,
                        SenderName = r != null ? r.FullName : "-",
                    };

        query = query.OrderByDescending(t => t.Id).AsNoTracking();


        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.RequestNum.Contains(search) ||
                d.SenderName.Contains(search)
            );
        }

        var total = await query.CountAsync();

        query = query.ApplySorting(sortField, sortOrder == "desc");

        return (await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(), total);
    }


    public async Task<(bool isSuccess, string message, ScannedPackage data)> ValidateUidAsync(
        int? dispatchLineId,
        int? itemDetailStoreId,
        string barcode,
        int recipientId)
    {
        Gs1BarcodeInfo barcodeInfo;

        if (barcode.Length == 20 && barcode.All(char.IsDigit))
        {
            barcodeInfo = new Gs1BarcodeInfo
            {
                Uid = barcode,
                IsGs1 = false
            };
        }
        else
        {
            barcodeInfo = barcodeParser.Parse(barcode);

            if (barcodeInfo == null || string.IsNullOrEmpty(barcodeInfo.Uid))
            {
                return (false, "شناسه واکسن از بارکد استخراج نشد.", null);
            }

            barcodeInfo.IsGs1 = true;
        }

        int? itemId = null;
        string batchNumber = null;

        if (dispatchLineId.HasValue && dispatchLineId.Value > 0)
        {
            var dispatchLine = await context.TblDispatchLines
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DispatchLineId == dispatchLineId.Value);

            if (dispatchLine == null)
                return (false, "آیتم حواله یافت نشد.", null);

            itemId = dispatchLine.ItemId;
            batchNumber = dispatchLine.BatchNumber;
        }
        else if (itemDetailStoreId.HasValue && itemDetailStoreId.Value > 0)
        {
            var store = await context.TblItemDetailStores
                .AsNoTracking()
                .Include(x => x.ItemDetail)
                .FirstOrDefaultAsync(x => x.PKItemDetailStoreId == itemDetailStoreId.Value);

            if (store == null)
                return (false, "سری ساخت یافت نشد.", null);

            itemId = store.ItemDetail.ItemId;
            batchNumber = store.ItemDetail.BatchNumber;
        }
        else
        {
            return (false, "اطلاعات اعتبارسنجی ناقص است.", null);
        }

        if (barcodeInfo.IsGs1 && barcodeInfo.ExpiryDate.HasValue && barcodeInfo.ExpiryDate.Value < DateTime.Today)
        {
            return (false, "تاریخ مصرف واکسن گذشته است.", null);
        }


        var uidInfoRaw = await context.TblUidInfo
            .AsNoTracking()
            .Include(x => x.OrderInfo)
            .Where(x =>
                (x.FirstLevelUid == barcodeInfo.Uid ||
                 x.SecLevelUid == barcodeInfo.Uid ||
                 x.ParentUid == barcodeInfo.Uid)
                &&
                x.IsActive
                &&
                x.OrderInfo.FkArrivalId != null
                &&
                x.OrderInfo.Status == OrderInfoStatus.APPLIED.ToString()
            )
            .Select(x => new
            {
                x.PkUidInfo,
                x.FirstLevelUid,
                x.SecLevelUid,
                x.ParentUid,
                x.CurrentStore,
                x.FKDispatchUid,
                x.FkReceiveDispatchId,
                x.Used,
                ItemId = x.OrderInfo.FkItemId,
                x.OrderInfo.BatchNumber
            })
            .FirstOrDefaultAsync();

        if (uidInfoRaw == null)
        {
            return (false, "برای بارکد اسکن شده واکسنی یافت نشد.", null);
        }

        if (uidInfoRaw.Used != false)
        {
            return (false, "UID مورد نظر مصرف شده است.", null);
        }

        //if (uidInfoRaw.FKDispatchUid.HasValue)
        //{
        //    return (false, "این UID قبلاً در یک ارسال دیگر استفاده شده است.", null);
        //}

        //if (uidInfoRaw.FkReceiveDispatchId.HasValue)
        //{
        //    return (false, "این UID قبلاً دریافت شده است.", null);
        //}

        if (uidInfoRaw.ItemId != itemId)
        {
            return (false, "بارکد متعلق به این کالا نمی‌باشد.", null);
        }

        if (barcodeInfo.IsGs1 &&
            !string.IsNullOrEmpty(barcodeInfo.BatchNumber) &&
            barcodeInfo.BatchNumber != uidInfoRaw.BatchNumber)
        {
            return (false, "سری ساخت بارکد صحیح نیست.", null);
        }

        if (uidInfoRaw.CurrentStore.HasValue && recipientId > 0)
        {
            var userStoreIds = await context.TblStores
                .Where(s => s.RecipientId == recipientId)
                .Select(s => s.StoreId)
                .ToListAsync();

            if (!userStoreIds.Contains(uidInfoRaw.CurrentStore.Value))
            {
                return (false, "UID یافت نشد.", null);
            }
        }

        UidLevel level;

        if (uidInfoRaw.SecLevelUid == barcodeInfo.Uid)
            level = UidLevel.Second;
        else if (uidInfoRaw.FirstLevelUid == barcodeInfo.Uid)
            level = UidLevel.First;
        else
            level = UidLevel.Parent;

        var result = new ScannedPackage
        {
            Uid = barcodeInfo.Uid,
            Level = level,
            DisplayName = level switch
            {
                UidLevel.Second => "ویال",
                UidLevel.First => "بسته",
                UidLevel.Parent => "کارتن",
                _ => ""
            },
            ItemId = uidInfoRaw.ItemId,
            BatchNumber = uidInfoRaw.BatchNumber,
            Count = 0,
            ChildIds = new(),
            IsExpanded = false
        };

        switch (level)
        {
            case UidLevel.Second:
                result.ChildIds.Add(uidInfoRaw.PkUidInfo);
                result.Count = 1;
                break;

            case UidLevel.First:
                result.ChildIds = await context.TblUidInfo
                    .AsNoTracking()
                    .Where(x =>
                        x.FirstLevelUid == barcodeInfo.Uid
                        &&
                        x.IsActive
                        &&
                        x.Used == false
                        &&
                        x.FKDispatchUid == null
                        &&
                        x.FkReceiveDispatchId == null
                        &&
                        x.OrderInfo.FkArrivalId != null
                        &&
                        x.OrderInfo.Status == OrderInfoStatus.APPLIED.ToString()
                    )
                    .Select(x => x.PkUidInfo)
                    .ToListAsync();

                result.Count = result.ChildIds.Count;
                result.IsExpanded = true;
                break;

            case UidLevel.Parent:
                result.Count = await context.TblUidInfo
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.ParentUid == barcodeInfo.Uid
                        &&
                        x.IsActive
                        &&
                        x.Used == false
                        &&
                        x.FKDispatchUid == null
                        &&
                        x.FkReceiveDispatchId == null
                        &&
                        x.OrderInfo.FkArrivalId != null
                        &&
                        x.OrderInfo.Status == OrderInfoStatus.APPLIED.ToString()
                    );

                result.IsExpanded = false;
                break;
        }

        return (true, string.Empty, result);
    }


    public async Task<(bool isSuccess, string message, int count, int itemId)> SetUidAsync(
        string sessionId,
        int? dispatchLineId,
        int? itemDetailStoreId,
        string barcode,
        int recipientId)
    {

        var validation =
            await ValidateUidAsync(
                dispatchLineId,
                itemDetailStoreId,
                barcode,
                recipientId);

        if (!validation.isSuccess)
            return (
                false,
                validation.message,
                0,
                0
            );

        var package = validation.data;

        var result =
            dispatchScan.AddPackage(
                sessionId,
                package);

        if (!result)
        {
            return (
                false,
                "این شناسه یا بخشی از آن قبلاً انتخاب شده است.",
                0,
                package.ItemId
            );
        }

        return (
            true,
            string.Empty,
            package.Count,
            package.ItemId
        );
    }

    public async Task<(bool success, string message, int count)> ConfirmDispatchUidAsync(
        string sessionId,
        int dispatchId)
    {
        var session = dispatchScan.GetSession(sessionId);
        if (session == null || !session.Items.Any())
            return (false, "هیچ UID معتبری اسکن نشده است.", 0);

        var uidInfoList = new List<TblUidInfo>();
        foreach (var item in session.Items)
        {
            var infos = await context.TblUidInfo
                .Where(x =>
                    x.FirstLevelUid == item.Uid ||
                    x.SecLevelUid == item.Uid ||
                    x.ParentUid == item.Uid)
                .ToListAsync();
            uidInfoList.AddRange(infos);
        }

        if (!uidInfoList.Any())
            return (false, "هیچ UID معتبری یافت نشد.", 0);

        var dispatchUids = uidInfoList
            .Select(uidInfo => new TblDispatchUid
            {
                FKDispatchId = dispatchId,
                FKUidInfo = uidInfo.PkUidInfo
            })
            .Distinct()
            .ToList();

        await context.TblDispatchUids.AddRangeAsync(dispatchUids);
        await context.SaveChangesAsync();

        var reloadedDispatchUids = await context.TblDispatchUids
            .Where(d => d.FKDispatchId == dispatchId)
            .ToListAsync();

        foreach (var uidInfo in uidInfoList)
        {
            var matched = reloadedDispatchUids
                .FirstOrDefault(d => d.FKUidInfo == uidInfo.PkUidInfo);
            if (matched != null)
            {
                uidInfo.FKDispatchUid = matched.PKDispatchUid;
            }
        }

        context.TblUidInfo.UpdateRange(uidInfoList);
        await context.SaveChangesAsync();

        dispatchScan.Clear(sessionId);

        return (true, $"ثبت {dispatchUids.Count} UID با موفقیت انجام شد.", dispatchUids.Count);
    }

    //public async Task<(bool success, string message, int count)> ConfirmDispatchUidAsync(
    //string sessionId,
    //int dispatchId)
    //{

    //    var session =
    //        dispatchScan.GetSession(sessionId);



    //    if (session == null || !session.Items.Any())
    //    {
    //        return (
    //            false,
    //            "موردی برای ثبت وجود ندارد.",
    //            0
    //        );
    //    }



    //    var finalUidIds =
    //        new HashSet<int>();



    //    foreach (var item in session.Items)
    //    {

    //        IQueryable<int> query =
    //            context.TblUidInfo
    //            .Where(x =>
    //                x.FirstLevelUid == item.Uid ||
    //                x.SecLevelUid == item.Uid ||
    //                x.ParentUid == item.Uid
    //            )
    //            .Select(x => x.PkUidInfo);



    //        var ids =
    //            await query.ToListAsync();



    //        foreach (var id in ids)
    //        {
    //            finalUidIds.Add(id);
    //        }

    //    }



    //    var entities =
    //        finalUidIds.Select(x =>
    //            new TblDispatchUid
    //            {
    //                FKDispatchId = dispatchId,
    //                FKUidInfo = x
    //            })
    //        .ToList();



    //    await context.TblDispatchUids
    //        .AddRangeAsync(entities);



    //    await context.SaveChangesAsync();



    //    dispatchScan.Clear(sessionId);



    //    return (
    //        true,
    //        "ثبت با موفقیت انجام شد.",
    //        entities.Count
    //    );
    //}

    public Task<bool> RemoveUidFromSessionAsync(string sessionId, string uid)
    {
        var result = dispatchScan.RemovePackage(sessionId, uid);
        return Task.FromResult(result);
    }

    public async Task<DispatchPrintDto?> GetDispatchPrintDataAsync(int id)
    {
        var dispatch = await context.TblDispatches
            .Include(t => t.TblDispatchLines).ThenInclude(t => t.DosePerVial)
            .Include(t => t.TblDispatchLines).ThenInclude(t => t.Item)
            .Include(t => t.TblDispatchLines).ThenInclude(t => t.Store)
            .Include(t => t.TblDispatchLines).ThenInclude(t => t.Manufacturer)
            .Include(t => t.TblDispatchLines).ThenInclude(t => t.ItemDetailStore).ThenInclude(t => t.ItemDetail)
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .Select(s => new DispatchPrintDto
            {
                Id = s.DispatchId,
                Note = s.Note,
                DispatchVoucherNumber = s.IsConfirmed.HasValue && s.IsConfirmed.Value ? s.DispatchVoucherNumber : s.SeqPreview.ToString(),
                RequestNumber = s.RequestNum,
                TransportMode = s.TransportMode,
                Date = s.Date,
                InputPerson = s.EnteredBy.FirstName + " " + s.EnteredBy.LastName,
                DeliveryPlace = s.Receiver.FullName,
                Recipient = s.Sender.FullName,
                IsCopied = s.DispatchVoucherPrinted ?? false,
                IsConfirmed = s.IsConfirmed,
                Items = s.TblDispatchLines.Select(t => new DispatchLinePrintDto()
                {
                    BatchNumber = t.BatchNumber,
                    ColdStorageNumber = t.Store.Name,
                    DoseCount = t.Quantity,
                    ExpiryDate = t.ExpiryDate,
                    Item = t.Item.FullName,
                    VialSize = t.DosePerVial.Title,
                    VialNumber = t.NumVial
                })

            })
            .FirstOrDefaultAsync(t => t.Id == id);

        var transportMode = await context.TblBaseInfo.FindAsync(int.Parse(dispatch?.TransportMode ?? "0"));

        dispatch.TransportMode = transportMode?.Title ?? string.Empty;

        var dispatchEntity = await context.TblDispatches.FindAsync(id);
        dispatchEntity.DispatchVoucherPrinted = true;
        await context.SaveChangesAsync();

        return dispatch;
    }

    public async Task<string> GenerateReportNumberAsync()
    {
        var fiscalPeriod = await context.TblFiscalPeriods
            .FirstOrDefaultAsync(x => x.IsActive == true);

        var year = ExtractYear(fiscalPeriod?.FullName);
        var prefix = $"{year}-";

        var numbers = await context.TblDispatches
            .Where(x => x.ReportNumber.StartsWith(prefix))
            .Select(x => x.ReportNumber)
            .ToListAsync();

        int maxNumber = numbers
            .Select(rn => rn.Split('-'))
            .Where(parts => parts.Length == 2)
            .Select(parts => int.TryParse(parts[1], out int num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        int nextNumber = maxNumber + 1;
        return $"{year}-{nextNumber:D3}";
    }


    private static string ExtractYear(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "UNKNOWN";


        var match = Regex.Match(fullName, @"\d{4}");


        return match.Success
            ? match.Value
            : "UNKNOWN";
    }
}
