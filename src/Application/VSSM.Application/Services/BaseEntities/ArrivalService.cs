using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ArrivalService(VSSMDbContext db) : IArrivalService
{
    public async Task<PagingSearchOrderDto<ArrivalListDto>> GetPagedList(
        string? search,
        int statusFilter,
        int page,
        int pageSize,
        CancellationToken token)
    {
        var query = db.TblArrivals
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.ReportNumber.StartsWith(search));

        if (statusFilter == 1)
            query = query.Where(x => x.IsDrafted == true && x.IsCancelled != true);
        else if (statusFilter == 2)
            query = query.Where(x => x.IsConfirmed == true && x.IsCancelled != true);

        var totalItems = await query.CountAsync(token);

        var paging = new PagingModel
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        var pagedArrivals = await query
            .OrderByDescending(x => x.Date)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(a => new
            {
                a.ArrivalId,
                a.ReportNumber,
                a.Date,
                a.IsConfirmed,
                a.IsDrafted,
                a.IsCancelled,
                a.FiscalPeriodId
            })
            .ToListAsync(token);

        var fiscalIds = pagedArrivals
            .Where(x => x.FiscalPeriodId.HasValue)
            .Select(x => x.FiscalPeriodId.Value)
            .Distinct()
            .ToList();

        var fiscalPeriods = fiscalIds.Any()
           ? await db.TblFiscalPeriods
               .Where(f => fiscalIds.Contains(f.FiscalPeriodId))
               .ToDictionaryAsync(f => f.FiscalPeriodId, f => f.FullName, token)
           : new Dictionary<int, string>();

        var list = pagedArrivals.Select(a => new ArrivalListDto
        {
            ArrivalId = a.ArrivalId,
            ReportNumber = a.ReportNumber,
            Date = a.Date,
            IsConfirmed = a.IsConfirmed,
            IsDrafted = a.IsDrafted,
            IsCancelled = a.IsCancelled,
            FiscalPeriodName = a.FiscalPeriodId.HasValue
                ? fiscalPeriods.GetValueOrDefault(a.FiscalPeriodId.Value)
                : null
        }).ToList();


        return new PagingSearchOrderDto<ArrivalListDto>
        {
            Paging = paging,
            Items = list,
            SortField = "Date",
            SortOrder = "desc"
        };
    }

    /* =========================
       GET BY ID
    ========================= */
    public async Task<ArrivalGetDto?> GetById(int id, CancellationToken token)
    {
        var arrival = await db.TblArrivals
            .Include(x => x.TblArrivalLines)
            .FirstOrDefaultAsync(x => x.ArrivalId == id, token);

        if (arrival == null) return null;

        return new ArrivalGetDto
        {
            ArrivalId = arrival.ArrivalId,
            Date = arrival.Date,
            ReportNumber = arrival.ReportNumber,
            FiscalPeriodId = arrival.FiscalPeriodId,
            EntryDate = arrival.EntryDate,
            IsConfirmed = arrival.IsConfirmed,
            IsDrafted = arrival.IsDrafted,
            IsCancelled = arrival.IsCancelled,
            Note = arrival.Note,
            Lines = arrival.TblArrivalLines
                .Where(l => l.ArrivalId == arrival.ArrivalId)
                .Select(l => new ArrivalLineDto
                {
                    ArrivalLineId = l.ArrivalLineId,
                    ArrivalId = l.ArrivalId,
                    ItemId = l.ItemId,
                    BatchNum = l.BatchNum,
                    CommercialNameId = l.CommertialNameId,
                    DosePerVialId = l.DosePerVialId,
                    ExpiryDate = l.ExpiryDate,
                    ProductionDate = l.ProductionDate,
                    ItemDetailId = l.ItemDetailId,
                    ManufacturerId = l.ManufacturerId,
                    Note = l.Note,
                    NumVials = l.NumVials,
                    Quantity = l.Quantity,
                    StoreId = l.StoreId,
                    UseId = l.UseId,
                    VvmStageId = l.VvmstageId,
                    VvmTypeId = l.VvmtypeId
                }).ToList()
        };
    }

    /* =========================
       CREATE HEADER ONLY
    ========================= */
    public async Task<int> Create(ArrivalCreateDto dto, CancellationToken token)
    {
        var arrival = new TblArrival
        {
            Date = dto.Date,
            ReportNumber = dto.ReportNumber,
            FiscalPeriodId = dto.FiscalPeriodId,
            EntryDate = DateTime.Now,
            IsDrafted = true,
            IsConfirmed = false,
            IsCancelled = false,
            Note = dto.Note,
            ReceiverId = dto.UserRecipientId,
            EnteredById = dto.EnterById
        };

        db.TblArrivals.Add(arrival);
        await db.SaveChangesAsync(token);
        return arrival.ArrivalId;
    }


    /* =========================
       CONFIRM
    ========================= */


    public async Task Confirm(int arrivalId, ArrivalCreateDto dto, CancellationToken token)
    {
        var now = DateTime.Now;

        var arrival = await db.TblArrivals
            .FirstOrDefaultAsync(x => x.ArrivalId == arrivalId, token);

        if (arrival == null) return;

        var lines = await db.TblArrivalLines
            .Where(x => x.ArrivalId == arrivalId)
            .ToListAsync(token);

        await using var tx = await db.Database.BeginTransactionAsync(token);

        // ----- update arrival -----
        arrival.ReportNumber = dto.ReportNumber;
        arrival.EntryDate = now;
        arrival.IsCancelled = false;
        arrival.Note = dto.Note;
        arrival.ReceiverId = dto.UserRecipientId;
        arrival.IsConfirmed = true;
        arrival.IsDrafted = false;
        arrival.ConfirmationTime = now;

        // ----- 1) create ItemDetails first -----
        var map = new Dictionary<TblArrivalLine, TblItemDetail>(lines.Count);

        foreach (var line in lines)
        {
            var detail = new TblItemDetail
            {
                EntryDate = now,
                FiscalPeriodId = arrival.FiscalPeriodId,
                ItemId = line.ItemId,
                DosePerVialId = line.DosePerVialId,
                ManufacturerId = line.ManufacturerId,
                CommercialNameId = line.CommertialNameId,
                VvmTypeId = line.VvmtypeId,
                BatchNumber = line.BatchNum,
                ExpiryDate = line.ExpiryDate,
                ProductionDate = line.ProductionDate,
                IsConfirmed = true,
            };

            map.Add(line, detail);
            db.TblItemDetails.Add(detail);
        }

        // Save so ItemDetailId is generated (no more 0)
        await db.SaveChangesAsync(token);

        // ----- 2) now set ArrivalLine.ItemDetailId + create stores using ONLY IDs -----
        var stores = new List<TblItemDetailStore>(lines.Count);

        foreach (var (line, detail) in map)
        {
            // FK on line (now we have real detail.ItemDetailId)
            line.ItemDetailId = detail.ItemDetailId;

            stores.Add(new TblItemDetailStore
            {
                ItemDetailId = detail.ItemDetailId,
                StoreId = line.StoreId,
                CurrentStock = line.Quantity,
                EntryDate = now,
                FkVvmStageId = line.FkVvmStageId  // ← ID نه object
            });
        }

        db.TblItemDetailStores.AddRange(stores);

        await db.SaveChangesAsync(token);
        await tx.CommitAsync(token);
    }


    /* =========================
       DRAFT
    ========================= */
    public async Task SetDraft(int arrivalId, ArrivalCreateDto dto, CancellationToken token)
    {
        var arrival = await db.TblArrivals.FindAsync(new object[] { arrivalId }, token);
        if (arrival == null) return;

        arrival.ReportNumber = dto.ReportNumber;
        arrival.EntryDate = DateTime.Now;
        arrival.IsCancelled = false;
        arrival.Note = dto.Note;
        arrival.ReceiverId = dto.UserRecipientId;
        arrival.IsDrafted = true;
        arrival.IsConfirmed = false;

        await db.SaveChangesAsync(token);
    }

    /* =========================
       DELETE WHOLE ARRIVAL
    ========================= */
    public async Task Cancel(int id, CancellationToken token)
    {
        var arrival = await db.TblArrivals.FindAsync(new object[] { id }, token);
        if (arrival == null) return;

        arrival.IsCancelled = true;
        await db.SaveChangesAsync(token);
    }

    /* =========================
       DELETE ONE LINE
    ========================= */
    public async Task CancelLine(int arrivalLineId, CancellationToken token)
    {
        var line = await db.TblArrivalLines.FindAsync(new object[] { arrivalLineId }, token);
        if (line == null) return;

        db.TblArrivalLines.Remove(line);
        await db.SaveChangesAsync(token);
    }

    public async Task<List<ArrivalCopyDto>> GetForCopy(
        string? reportNumber,
        CancellationToken token)
    {
        var query = db.TblArrivals
            .Where(x => x.IsCancelled != true); // ⬅ only cancelled excluded

        if (!string.IsNullOrWhiteSpace(reportNumber))
        {
            query = query.Where(x => x.ReportNumber.Contains(reportNumber));
        }

        return await query
            .OrderByDescending(x => x.EntryDate)
            .Select(x => new ArrivalCopyDto
            {
                ArrivalId = x.ArrivalId,
                DisplayText =
                    db.TblFiscalPeriods
                        .Where(f => f.FiscalPeriodId == x.FiscalPeriodId)
                        .Select(f => f.FullName)
                        .FirstOrDefault()
                    + " - " + x.ReportNumber
            })
            .ToListAsync(token);
    }



    public async Task Delete(int id)
    {
        var entity = await db.TblArrivals.FirstOrDefaultAsync(z => z.ArrivalId == id);
        db.TblArrivals.Remove(entity);
        await db.SaveChangesAsync();
    }

    /* =========================
ADD LINE (INCREMENTAL)
========================= */
    public async Task AddLine(int arrivalId, ArrivalLineDto dto, CancellationToken token)
    {
        var does = int.Parse(db.TblBaseInfo.First(z => z.BaseInfoId == dto.DosePerVialId).Title);
        if (dto.Quantity % does != 0)
            throw new ValidationException("تعداد باید مضربی از دوز بر ویال باشد");
        //var id = _db.TblArrivalLines.OrderByDescending(z => z.ArrivalLineId).First().ArrivalLineId + 1;
        var line = new TblArrivalLine
        {
            //ArrivalLineId = id,
            ArrivalId = arrivalId,
            ItemId = dto.ItemId,
            BatchNum = dto.BatchNum,
            CommertialNameId = dto.CommercialNameId,
            DosePerVialId = dto.DosePerVialId,
            ExpiryDate = dto.ExpiryDate,
            ProductionDate = dto.ProductionDate,
            ItemDetailId = dto.ItemDetailId,
            ManufacturerId = dto.ManufacturerId,
            Note = dto.Note,
            NumVials = dto.NumVials,
            Quantity = dto.Quantity,
            StoreId = dto.StoreId,
            UseId = dto.UseId,
            VvmstageId = dto.VvmStageId,
            VvmtypeId = dto.VvmTypeId,
            EntryDate = DateTime.Now
        };

        db.TblArrivalLines.Add(line);

        // update header total volume
        var arrival = await db.TblArrivals.FindAsync(new object[] { arrivalId }, token);

        await db.SaveChangesAsync(token);
    }

    public async Task<List<ArrivalLineDto>> GetArrivalLines(int userRecipientId)
    {
        var lines = await db.TblArrivalLines
            .Where(l => db.TblArrivals
                .Any(a => a.ReceiverId == userRecipientId && a.ArrivalId == l.ArrivalId))
            .Select(l => new
            {
                Line = l,
                Item = db.TblItems.FirstOrDefault(i => i.ItemId == l.ItemId),
                Manufacturer = db.TblBaseInfo.FirstOrDefault(m => m.BaseInfoId == l.ManufacturerId),
                Store = db.TblStores.FirstOrDefault(s => s.StoreId == l.StoreId),
                DoseVial = db.TblBaseInfo.FirstOrDefault(d => d.BaseInfoId == l.DosePerVialId)
            })
            .AsNoTracking()
            .ToListAsync();

        var result = lines.Select(x =>
        {
            var l = x.Line;

            return new ArrivalLineDto
            {
                ArrivalLineId = l.ArrivalLineId,
                ArrivalId = l.ArrivalId,
                ItemId = l.ItemId,
                BatchNum = l.BatchNum,
                CommercialNameId = l.CommertialNameId,
                DosePerVialId = l.DosePerVialId,
                ExpiryDate = l.ExpiryDate,
                ProductionDate = l.ProductionDate,
                ItemDetailId = l.ItemDetailId,
                ManufacturerId = l.ManufacturerId,
                Note = l.Note,
                NumVials = l.NumVials,
                Quantity = l.Quantity,
                StoreId = l.StoreId,
                UseId = l.UseId,
                VvmStageId = l.VvmstageId,
                VvmTypeId = l.VvmtypeId,
                DropDownText = string.Format(
                    "کالا: {0} | سازنده: {1} | تاریخ انقضا: {2} | سری ساخت: {3} | انبار: {4} | دوز بر ویال: {5} | مقدار موجود: {6}",
                    x.Item != null ? x.Item.FullName : "",
                    x.Manufacturer != null ? x.Manufacturer.Title : "",
                    l.ExpiryDate != null ? l.ExpiryDate.Value.ToShamsi() : "",
                    l.BatchNum,
                    x.Store != null ? x.Store.Name : "",
                    x.DoseVial != null ? x.DoseVial.Title : "",
                    l.Quantity
                )
            };
        }).ToList();

        return result;
    }

    public async Task<bool> DoesHaveArrivalLineByArrivalId(int id)
    {
        if (await db.TblArrivalLines.AnyAsync(c => c.ArrivalId == id))
            return true;
        return false;
    }
    public async Task<ArrivalLineDto> GetArrivalLineById(int id)
    {
        var line = await db.TblArrivalLines.FirstOrDefaultAsync(c => c.ArrivalLineId == id);

        var dto = new ArrivalLineDto
        {
            ArrivalLineId = line.ArrivalLineId,
            ArrivalId = line.ArrivalId,
            ItemId = line.ItemId,
            BatchNum = line.BatchNum,
            CommercialNameId = line.CommertialNameId,
            DosePerVialId = line.DosePerVialId,
            ExpiryDate = line.ExpiryDate,
            ProductionDate = line.ProductionDate,
            ItemDetailId = line.ItemDetailId,
            ManufacturerId = line.ManufacturerId,
            Note = line.Note,
            NumVials = line.NumVials,
            Quantity = line.Quantity,
            StoreId = line.StoreId,
            UseId = line.UseId,
            VvmStageId = line.VvmstageId,
            VvmTypeId = line.VvmtypeId
        };

        return dto;
    }

    public async Task<ArrivalLineDto> GetArrivalLineByStoreIdAndItemDetailId(long storeId, long itemDetailId)
    {
        var line = await db.TblArrivalLines.FirstOrDefaultAsync(c => c.StoreId == storeId && c.ItemDetailId == itemDetailId);

        var dto = new ArrivalLineDto
        {
            ArrivalLineId = line.ArrivalLineId,
            ArrivalId = line.ArrivalId,
            ItemId = line.ItemId,
            BatchNum = line.BatchNum,
            CommercialNameId = line.CommertialNameId,
            DosePerVialId = line.DosePerVialId,
            ExpiryDate = line.ExpiryDate,
            ProductionDate = line.ProductionDate,
            ItemDetailId = line.ItemDetailId,
            ManufacturerId = line.ManufacturerId,
            Note = line.Note,
            NumVials = line.NumVials,
            Quantity = line.Quantity,
            StoreId = line.StoreId,
            UseId = line.UseId,
            VvmStageId = line.VvmstageId,
            VvmTypeId = line.VvmtypeId
        };

        return dto;
    }

    public Task AddToTotal(int? id, float total)
    {
        throw new NotImplementedException();
    }

    public Task SubtractionToTotal(int lineId, int arrivalId)
    {
        throw new NotImplementedException();
    }
}