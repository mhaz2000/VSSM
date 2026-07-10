using Microsoft.EntityFrameworkCore;
using System.Text;
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
    IGs1BarcodeParser barcodeParser) : IDispatchService
{
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

    public async Task<List<BatchRowDto>> GetBatchesByItemAsync(int itemId, CancellationToken ct = default)
    {
        var item = await context.TblItems.FindAsync(itemId);

        // بند ۶: پیدا کردن حلال مرتبط در صورت RequiresDiluent = true
        // از فیلد DiluentId در TblItem استفاده می‌شود (IsDiluent وجود ندارد)
        TblItem? diluentItem = null;
        TblItemDetailStore? diluentStore = null;
        if (item?.RequiresDiluent == true && item.DiluentId.HasValue)
        {
            diluentItem = await context.TblItems.FindAsync(item.DiluentId.Value);

            if (diluentItem != null)
            {
                diluentStore = await context.TblItemDetailStores
                    .Include(s => s.ItemDetail)
                    .Where(s => s.ItemDetail.ItemId == diluentItem.ItemId && s.CurrentStock > 0)
                    .OrderByDescending(s => s.ItemDetailId)
                    .FirstOrDefaultAsync();
            }
        }

        // بند ۷a + بند ۶: query بدون ToShamsi (چون EF نمی‌تواند ترجمه کند)
        // تاریخ‌ها به صورت خام برگشت داده و سپس در حافظه تبدیل می‌شوند
        var rawRows = await (
            from ids in context.TblItemDetailStores
                .Where(s => s.ItemDetail.ItemId == itemId && s.CurrentStock > 0)
            join id in context.TblItemDetails on ids.ItemDetailId equals id.ItemDetailId
            join mfr in context.TblBaseInfo on id.ManufacturerId equals mfr.BaseInfoId
                into mfrJ
            from mfr in mfrJ.DefaultIfEmpty()
            join dpv in context.TblBaseInfo on id.DosePerVialId equals dpv.BaseInfoId
                into dpvJ
            from dpv in dpvJ.DefaultIfEmpty()
            join store in context.TblStores on ids.StoreId equals store.StoreId
                into storeJ
            from store in storeJ.DefaultIfEmpty()
            select new
            {
                ids.PKItemDetailStoreId,
                id.BatchNumber,
                id.ExpiryDate,
                CurrentStock = ids.CurrentStock ?? 0,
                DosePerVialStr = dpv != null ? dpv.Title : "-",
                DosePerVialNum = dpv != null ? int.Parse(dpv.Title) : 0,
                ManufacturerName = mfr != null ? mfr.Title : "-",
                StoreId = store != null ? store.StoreId : 0,
                StoreName = store != null ? store.Name : "-",
            }
        ).ToListAsync(ct);

        // بند ۷a: تبدیل تاریخ انقضا به شمسی در حافظه (نه داخل EF query)
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

            // بند ۶: اطلاعات حلال — در حافظه محاسبه می‌شود
            RequiresDiluent = item.RequiresDiluent == true,
            DiluentItemDetailStoreId = diluentStore?.PKItemDetailStoreId,
            DiluentItemId = diluentItem?.ItemId,
            DiluentItemName = diluentItem?.FullName,
            DiluentBatchNumber = diluentStore?.ItemDetail?.BatchNumber,
            // بند ۷a: تبدیل تاریخ انقضا حلال نیز در حافظه
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
        // تمام رکوردهای tblDispatchUid مرتبط با این dispatch
        var dispatchUids = await context.TblDispatchUids
            .Where(du => du.FKDispatchId == dispatchId)
            .ToListAsync(ct);

        foreach (var du in dispatchUids)
        {
            // بند ۹: بروزرسانی FK_DISPATCH_UID در tblUidInfo
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

        dispatch.SenderId = updateDispatcheDto.SenderId;
        dispatch.ReceiverId = updateDispatcheDto.ReceiverId;
        dispatch.TransportMode = updateDispatcheDto.TransportMode;
        dispatch.EnteredById = updateDispatcheDto.EnteredById;
        dispatch.RequestNum = updateDispatcheDto.RequestNum;
        dispatch.Note = updateDispatcheDto.Note;
        dispatch.Date = string.IsNullOrWhiteSpace(updateDispatcheDto.Date) ? null : DateTimeHelper.ToGregorian(updateDispatcheDto.Date);
        dispatch.IsDrafted = submitType == "draft" ? true : false;
        dispatch.ReceivingMode = updateDispatcheDto.ReceivingMode;
        dispatch.IsConfirmed = submitType == "final" ? true : false;
        dispatch.ConfirmationTime = submitType == "final" ? DateTime.Now : null;
        dispatch.DispatchVoucherNumber = submitType == "final" ? (int.Parse(dispatch.DispatchVoucherNumber) + 1).ToString() : dispatch.DispatchVoucherNumber;
        dispatch.ReportNumber = updateDispatcheDto.ReportNumber;

        if (submitType == "final")
            dispatch.SeqPreview = 0;

        var dispatchLines = await context.TblDispatchLines.Where(t => t.DispatchId == updateDispatcheDto.Id).ToListAsync();

        context.TblDispatchLines.RemoveRange(dispatchLines);

        var removedItems = dispatchLines.Select(s => s.ItemId!.Value).ToList().Except(updateDispatcheDto.Items.Select(s => s.ItemId));

        var dispatchUids = context.TblDispatchUids.Where(t => removedItems.Contains(t.UidInfo.OrderInfo.FkItemId) && t.FKDispatchId == updateDispatcheDto.Id)
            .Include(t => t.UidInfo).ThenInclude(t => t.OrderInfo);

        context.TblDispatchUids.RemoveRange(dispatchUids);

        float totalPrice = 0;
        float totalVolume = 0;

        StringBuilder stringBuilder = new StringBuilder();
        foreach (var item in updateDispatcheDto.Items)
        {
            var itemInfo = await context.TblItems.FindAsync(item.ItemId);
            var itemDetailStore = await context.TblItemDetailStores.FindAsync(item.ItemDetailStoreId);
            var itemDetail = await context.TblItemDetails.Include(t => t.DosePerVial)
                .FirstOrDefaultAsync(t => t.ItemDetailId == itemDetailStore.ItemDetailId);

            int numvial = 0;
            if (itemDetail is not null && itemDetail.DosePerVial is not null)
                numvial = item.DoseCount / int.Parse(itemDetail.DosePerVial.Title);

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

            totalVolume += item.Volume ?? 0;

            context.TblDispatchLines.Add(line);
        }
        if (!string.IsNullOrEmpty(stringBuilder.ToString()))
            throw new ApplicationException(stringBuilder.ToString());

        context.TblDispatches.Update(dispatch);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(int dispatchId, long seqPreview)> CreateDispatchAsync(CreateDispatchDto createDispatcheDto, string submitType, CancellationToken cancellationToken)
    {
        var fiscalPeriod = await context.TblFiscalPeriods.FirstOrDefaultAsync(f => f.IsActive.Value);

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
            ReportNumber = createDispatcheDto.ReportNumber
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
        var dispatch = await context.TblDispatches.FindAsync(id);

        context.TblDispatches.Remove(dispatch);

        await context.SaveChangesAsync(cancellationToken);

    }

    public async Task DispatchArrivedAsync(
        DispatchArrivedDto arrivedDto,
        int userId,
        int recipientId,
        CancellationToken cancellationToken)
    {
        var lineIds = arrivedDto.Lines.Select(t => t.DispatchLineId).ToList();
        var storeMap = arrivedDto.Lines.ToDictionary(l => l.DispatchLineId, l => l.StoreId);
        var batchNumbers = new HashSet<string>();

        // ── ۱. واکشی همه داده با یک کوئری ──
        var fiscalPeriodId = await context.TblFiscalPeriods
            .Where(f => f.IsActive.Value)
            .Select(f => f.FiscalPeriodId)
            .FirstOrDefaultAsync(cancellationToken);

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

        if (data.Count == 0) return;

        var first = data[0];
        var now = DateTime.Now;

        // ── ۲. ساخت ReceiveDispatch ──
        var receiveDispatch = new TblReceiveDispatches
        {
            ReceiveDate = DateTime.Today,
            CreationTime = now,
            CreatedBy = userId
        };
        context.TblReceiveDispatches.Add(receiveDispatch);
        await context.SaveChangesAsync(cancellationToken);

        // ── ۳. ساخت Arrival و خطوطش ──
        var arrival = new TblArrival
        {
            Date = now,
            FiscalPeriodId = fiscalPeriodId,
            EntryDate = now,
            EnteredById = userId.ToString(),
            IsConfirmed = true,
            IsDrafted = false,
            ConfirmationTime = now,
            SenderId = first.SenderId,
            ReceiverId = recipientId,
            DispatchId = first.DispatchId,
        };

        var itemDetailStores = new List<TblItemDetailStore>(data.Count);

        foreach (var item in data)
        {
            var storeId = storeMap[item.DispatchLineId];

            arrival.TblArrivalLines.Add(new TblArrivalLine
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
                StoreId = storeId,
                FkUseId = item.FkUseBaseInfo,
                FkVvmStageId = item.FkVvmStageBaseInfo,
                FkVvmTypeId = item.FkVvmTypeBaseInfo,
            });

            itemDetailStores.Add(new TblItemDetailStore
            {
                ItemDetailId = item.ItemDetailId,
                StoreId = storeId,
                CurrentStock = item.Quantity,
                EntryDate = now
            });

            batchNumbers.Add(item.BatchNumber);
        }

        context.TblItemDetailStores.AddRange(itemDetailStores);
        context.TblArrivals.Add(arrival);
        await context.SaveChangesAsync(cancellationToken);

        // ── ۴. آپدیت UIDها با یک کوئری ──
        var uids = await context.TblUidInfo
            .Where(u => batchNumbers.Contains(u.OrderInfo.BatchNumber) && u.FkReceiveDispatchId == null)
            .ToListAsync(cancellationToken);

        foreach (var uid in uids)
        {
            uid.FkReceiveDispatchId = receiveDispatch.PkReceiveDispatchesId;
        }

        var currentDispatch = await context.TblDispatches.FindAsync(first.DispatchId);
        currentDispatch.ArrivalId = arrival.ArrivalId;
        currentDispatch.ReceiverStatus = 2;
        currentDispatch.FkReceiveDispatchId = receiveDispatch.PkReceiveDispatchesId;

        await context.SaveChangesAsync(cancellationToken);
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
                    join r in context.TblUsers on d.ReceiverId equals r.UserId into rj
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
                        ReceiverName = r != null ? r.FirstName + " " + r.LastName : "-",
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

        // 1) Search
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
     string barcode)
    {
        Gs1BarcodeInfo barcodeInfo;
        if (barcode.Length == 20 && barcode.All(char.IsDigit))
        {
            barcodeInfo = new Gs1BarcodeInfo { Uid = barcode, IsGs1 = false };
        }
        else
        {
            barcodeInfo = barcodeParser.Parse(barcode);
            if (barcodeInfo == null || string.IsNullOrEmpty(barcodeInfo.Uid))
                return (false, "شناسه واکسن از بارکد استخراج نشد.", null);
            barcodeInfo.IsGs1 = true;
        }

        int? itemId = null;
        string? batchNumber = null;

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
                .Include(s => s.ItemDetail)
                .FirstOrDefaultAsync(s => s.PKItemDetailStoreId == itemDetailStoreId.Value);
            if (store == null)
                return (false, "سری ساخت یافت نشد.", null);
            itemId = store.ItemDetail.ItemId;
            batchNumber = store.ItemDetail.BatchNumber;
        }
        else
        {
            return (false, "اطلاعات کافی برای اعتبارسنجی وجود ندارد.", null);
        }

        if (barcodeInfo.IsGs1 && barcodeInfo.ExpiryDate.HasValue && barcodeInfo.ExpiryDate.Value < DateTime.Today)
            return (false, "تاریخ مصرف واکسن گذشته است.", null);

        var uidInfo = await context.TblUidInfo
            .AsNoTracking()
            .Include(x => x.OrderInfo)
            .Where(x =>
                (x.FirstLevelUid == barcodeInfo.Uid ||
                 x.SecLevelUid == barcodeInfo.Uid ||
                 x.ParentUid == barcodeInfo.Uid)
                && x.IsActive
                && x.OrderInfo.FkArrivalId != null
                && x.OrderInfo.Status == OrderInfoStatus.APPLIED.ToString()
            )
            .Select(x => new
            {
                x.PkUidInfo,
                x.FirstLevelUid,
                x.SecLevelUid,
                x.ParentUid,
                ItemId = x.OrderInfo.FkItemId,
                x.OrderInfo.BatchNumber
            })
            .FirstOrDefaultAsync();

        if (uidInfo == null)
            return (false, "برای بارکد اسکن شده واکسنی یافت نشد.", null);

        if (uidInfo.ItemId != itemId)
            return (false, "بارکد متعلق به این کالا نمی‌باشد.", null);

        if (uidInfo.BatchNumber != batchNumber)
            return (false, "بارکد متعلق به این سری ساخت نمی‌باشد.", null);

        if (barcodeInfo.IsGs1 && !string.IsNullOrEmpty(barcodeInfo.BatchNumber)
            && barcodeInfo.BatchNumber != uidInfo.BatchNumber)
            return (false, "سری ساخت بارکد صحیح نیست.", null);

        UidLevel level;
        if (uidInfo.SecLevelUid == barcodeInfo.Uid)
            level = UidLevel.Second;
        else if (uidInfo.FirstLevelUid == barcodeInfo.Uid)
            level = UidLevel.First;
        else
            level = UidLevel.Parent;

        var result = new ScannedPackage
        {
            Uid = barcodeInfo.Uid,
            Level = level,
            ItemId = uidInfo.ItemId,
            BatchNumber = uidInfo.BatchNumber,
            Count = 0,
            DisplayName = level.ToString()
        };

        return (true, string.Empty, result);
    }


    public async Task<(bool isSuccess, string message, int count, int itemId)> SetUidAsync(
        string sessionId,
        int? dispatchLineId,
        int? itemDetailStoreId,
        string barcode)
    {
        var validation = await ValidateUidAsync(dispatchLineId, itemDetailStoreId, barcode);
        if (!validation.isSuccess)
            return (false, validation.message, 0, 0);

        var package = validation.data;

        var session = dispatchScan.GetSession(sessionId);
        if (session.Items.Any(x => x.Uid == package.Uid))
            return (false, "این شناسه قبلا اسکن شده است.", 0, package.ItemId);

        session.Items.Add(package);
        dispatchScan.SaveSession(session);

        return (true, string.Empty, 1, package.ItemId);
    }

    public async Task<(bool success, string message, int count)> ConfirmDispatchUidAsync(
    string sessionId,
    int dispatchId)
    {

        var session =
            dispatchScan.GetSession(sessionId);



        if (session == null || !session.Items.Any())
        {
            return (
                false,
                "موردی برای ثبت وجود ندارد.",
                0
            );
        }



        var finalUidIds =
            new HashSet<int>();



        foreach (var item in session.Items)
        {

            IQueryable<int> query =
                context.TblUidInfo
                .Where(x =>
                    x.FirstLevelUid == item.Uid ||
                    x.SecLevelUid == item.Uid ||
                    x.ParentUid == item.Uid
                )
                .Select(x => x.PkUidInfo);



            var ids =
                await query.ToListAsync();



            foreach (var id in ids)
            {
                finalUidIds.Add(id);
            }

        }



        var entities =
            finalUidIds.Select(x =>
                new TblDispatchUid
                {
                    FKDispatchId = dispatchId,
                    FKUidInfo = x
                })
            .ToList();



        await context.TblDispatchUids
            .AddRangeAsync(entities);



        await context.SaveChangesAsync();



        dispatchScan.Clear(sessionId);



        return (
            true,
            "ثبت با موفقیت انجام شد.",
            entities.Count
        );
    }

    public Task<bool> RemoveUidFromSessionAsync(string sessionId, string uid)
    {
        var result = dispatchScan.RemovePackage(sessionId, uid);
        return Task.FromResult(result);
    }

}
