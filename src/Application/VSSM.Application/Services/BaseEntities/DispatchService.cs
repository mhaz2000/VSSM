using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class DispatchService : IDispatchService
{
    private readonly VSSMDbContext _context;

    public DispatchService(VSSMDbContext context)
    {
        _context = context;
    }

    public async Task ConfirmDispatchAsync(int id, CancellationToken cancellationToken)
    {
        var dispatch = await _context.TblDispatches.FindAsync(id);

        if (dispatch.IsConfirmed.HasValue && dispatch.IsConfirmed.Value)
            return;

        dispatch.IsConfirmed = true;
        dispatch.IsDrafted = false;
        dispatch.ConfirmationTime = DateTime.Now;
        dispatch.DispatchVoucherNumber = (int.Parse(dispatch.DispatchVoucherNumber) + 1).ToString();

        await _context.SaveChangesAsync(cancellationToken);

    }

    public async Task UpdateDispatchAsync(UpdateDispatchDto updateDispatcheDto, string submitType, CancellationToken cancellationToken)
    {
        var dispatch = await _context.TblDispatches.FindAsync(updateDispatcheDto.Id);

        dispatch.SenderId = updateDispatcheDto.SenderId;
        dispatch.ReceiverId = updateDispatcheDto.ReceiverId;
        dispatch.TransportModeId = updateDispatcheDto.TransportModeId;
        dispatch.EnteredById = updateDispatcheDto.EnteredById;
        dispatch.AirwaryWillReference = updateDispatcheDto.AirwaryWillReference;
        dispatch.RequestNum = updateDispatcheDto.RequestNum;
        dispatch.Note = updateDispatcheDto.Note;
        dispatch.Date = string.IsNullOrWhiteSpace(updateDispatcheDto.Date) ? null : DateTimeHelper.ToGregorian(updateDispatcheDto.Date);
        dispatch.IsDrafted = submitType == "draft" ? true : false;
        dispatch.ReceivingModeId = updateDispatcheDto.ReceivingModeId;
        dispatch.IsConfirmed = submitType == "final" ? true : false;
        dispatch.ConfirmationTime = submitType == "final" ? DateTime.Now : null;
        dispatch.DispatchVoucherNumber = submitType == "final" ? (int.Parse(dispatch.DispatchVoucherNumber) + 1).ToString() : dispatch.DispatchVoucherNumber;
        dispatch.ReportNumber = updateDispatcheDto.ReportNumber;

        var dispatchLines = await _context.TblDispatchLines.Where(t => t.DispatchId == updateDispatcheDto.Id).ToListAsync();

        _context.TblDispatchLines.RemoveRange(dispatchLines);

        float totalPrice = 0;
        float totalVolume = 0;

        foreach (var item in updateDispatcheDto.Items)
        {
            var itemInfo = await _context.TblItems.FindAsync(item.ItemId);
            var itemDetailStore = await _context.TblItemDetailStores.FindAsync(item.ItemDetailStoreId);
            var itemDetail = await _context.TblItemDetails.FindAsync(itemDetailStore.ItemDetailId);


            var line = new TblDispatchLine
            {
                DispatchId = updateDispatcheDto.Id,
                ItemId = item.ItemId,
                Quantity = item.DoseCount,
                DosePerVialId = itemDetail.DosePerVialId,
                StoreId = item.StoreId,
                VvmtypeId = itemDetail.VvmtypeId,
                VvmstageId = item.VvmstageId,
                ManufacturerId = itemDetail.ManufacturerId,
                ItemDetailStoreId = item.ItemDetailStoreId,
                Note = item.Note,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate,
                EntryDate = DateTime.Now
            };

            totalVolume += item.Volume ?? 0;

            _context.TblDispatchLines.Add(line);
        }

        dispatch.TotalVolume = totalVolume;

        _context.TblDispatches.Update(dispatch);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateDispatchAsync(CreateDispatchDto createDispatcheDto, string submitType, CancellationToken cancellationToken)
    {
        var fiscalPeriod = await _context.TblFiscalPeriods.FirstOrDefaultAsync(f => f.IsActive.Value);

        var dispatch = new TblDispatch
        {
            SenderId = createDispatcheDto.SenderId,
            ReceiverId = createDispatcheDto.ReceiverId,
            FiscalPeriodId = fiscalPeriod.FiscalPeriodId,
            TransportModeId = createDispatcheDto.TransportModeId,
            EnteredById = createDispatcheDto.EnteredById,
            AirwaryWillReference = createDispatcheDto.AirwaryWillReference,
            RequestNum = createDispatcheDto.RequestNum,
            Note = createDispatcheDto.Note,
            Date = string.IsNullOrWhiteSpace(createDispatcheDto.Date) ? null : DateTimeHelper.ToGregorian(createDispatcheDto.Date),
            EntryDate = DateTime.Now,
            IsDrafted = submitType == "draft" ? true : false,
            ReceivingModeId = createDispatcheDto.ReceivingModeId,
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
            var itemInfo = await _context.TblItems.FindAsync(item.ItemId);
            var itemDetailStore = await _context.TblItemDetailStores.FindAsync(item.ItemDetailStoreId);
            var itemDetail = await _context.TblItemDetails.FindAsync(itemDetailStore.ItemDetailId);


            var line = new TblDispatchLine
            {
                ItemId = item.ItemId,
                Quantity = item.DoseCount,
                DosePerVialId = itemDetail.DosePerVialId,
                StoreId = item.StoreId,
                VvmtypeId = itemDetail.VvmtypeId,
                VvmstageId = item.VvmstageId,
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

        dispatch.TotalVolume = totalVolume;

        await _context.TblDispatches.AddAsync(dispatch, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDispatchAsync(int id, CancellationToken cancellationToken)
    {
        var dispatch = await _context.TblDispatches.FindAsync(id);

        _context.TblDispatches.Remove(dispatch);

        await _context.SaveChangesAsync(cancellationToken);

    }

    public async Task DispatchArrivedAsync(DispatchArrivedDto arrivedDto, int userId, int recipientId, CancellationToken cancellationToken)

    {
        var fiscalPeriod = await _context.TblFiscalPeriods.FirstOrDefaultAsync(f => f.IsActive.Value);

        var lineIds = arrivedDto.Lines
    .Select(t => t.DispatchLineId)
    .ToList();

        var query =
            from line in _context.TblDispatchLines
                .Where(l => lineIds.Contains(l.DispatchLineId))
            join dispatch in _context.TblDispatches
                .Where(d => d.ReceiverStatus == 0 &&
                            d.ReceiverId == recipientId &&
                            d.IsConfirmed == true)
                on line.DispatchId equals dispatch.DispatchId
            join itemDetailStore in _context.TblItemDetailStores
                on line.ItemDetailStoreId equals itemDetailStore.ItemDetailStoreId
            join itemDetail in _context.TblItemDetails
                on itemDetailStore.ItemDetailId equals itemDetail.ItemDetailId
            select new
            {
                line.DispatchLineId,
                dispatch.DispatchId,
                dispatch.SenderId,
                line.BatchNumber,
                itemDetail.CommercialNameId,
                line.DosePerVialId,
                line.ExpiryDate,
                itemDetail.ItemDetailId,
                line.ItemId,
                line.ManufacturerId,
                line.NumVial,
                itemDetail.PackageSize,
                itemDetail.PackingTypeId,
                itemDetail.ProductionDate,
                line.Quantity,
                line.TotalVolume,
                itemDetail.UnitVolume,
                line.UseId,
                line.VvmstageId,
                line.VvmtypeId,
            };

            var data = await query.ToListAsync();

        var currentDispatch = await _context.TblDispatches.FindAsync(data.FirstOrDefault().DispatchId);


        var arrival = new TblArrival()
        {
            Date = DateTime.Now,
            FiscalPeriodId = fiscalPeriod.FiscalPeriodId,
            EntryDate = DateTime.Now,
            EnteredById = userId.ToString(),
            IsConfirmed = true,
            IsDrafted = false,
            IsFirstTime = false,
            ConfirmationTime = DateTime.Now,
            IsFirstTimeArrangedAsked = false,
            PackedVolume = 0,
            TotalWeight = 0,
            TotalVolume = 0,
            SenderId = data.FirstOrDefault().SenderId,
            ReceiverId = recipientId,
            DispatchId = data.FirstOrDefault().DispatchId,
            Frzindc = arrivedDto.Frzindc,
            FrzindcNok = arrivedDto.FrzindcNok,
        };

        foreach (var item in data)
        {
            var arrivalLine = new TblArrivalLine()
            {
                BatchNum = item.BatchNumber,
                CommertialNameId = item.CommercialNameId,
                DosePerVialId = item.DosePerVialId,
                EntryDate = DateTime.Now,
                ExpiryDate = item.ExpiryDate,
                ItemDetailId = item.ItemDetailId,
                ItemId = item.ItemId,
                ManufacturerId = item.ManufacturerId,
                PackageSize = item.PackageSize,
                PackingTypeId = item.PackingTypeId,
                ProductionDate = item.ProductionDate,
                Quantity = item.Quantity,
                StoreId = arrivedDto.Lines.FirstOrDefault(line => line.DispatchLineId == item.DispatchLineId).StoreId,
                TempDosePerVial = "0",
                TotalVolume = item.TotalVolume,
                UnitVolume = item.UnitVolume,
                UseId = item.UseId,
                VvmstageId = item.VvmstageId,
                VvmtypeId = item.VvmtypeId,
            };

            var itemDetailStore = new TblItemDetailStore()
            {
                ItemDetailId = item.ItemDetailId,
                StoreId = arrivedDto.Lines.FirstOrDefault(line => line.DispatchLineId == item.DispatchLineId).StoreId,
                CurrentStock = item.Quantity,
                EntryDate = DateTime.Now
            };

            await _context.TblItemDetailStores.AddAsync(itemDetailStore);

            arrival.TblArrivalLines.Add(arrivalLine);
        }


        await _context.TblArrivals.AddAsync(arrival, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        currentDispatch.ArrivalId = arrival.ArrivalId;
        currentDispatch.ReceiverStatus = 2;

        await _context.SaveChangesAsync(cancellationToken);

    }

    public async Task<UpdateDispatchDto?> GetByIdAsync(int id)
    {
        var dispatch = await _context.TblDispatches.FindAsync(id);
        if (dispatch is null) return null;

        var items = from line in _context.TblDispatchLines.Where(t => t.DispatchId == id)
                    join item in _context.TblItems on line.ItemId equals item.ItemId
                    join vvmstage in _context.TblVvmstages on line.VvmstageId equals vvmstage.VvmstageId into vvmstages
                    from vvmstage in vvmstages.DefaultIfEmpty()
                    select new { line, item, vvmstage };

        var model = new UpdateDispatchDto
        {
            IsConfirmed = dispatch.IsConfirmed ?? false,
            Id = dispatch.DispatchId,
            Date = dispatch.Date?.Date.ToString("yyyy/MM/dd"),
            RequestNum = dispatch.RequestNum,
            DispatchVoucherNumber = ulong.TryParse(dispatch.DispatchVoucherNumber, out ulong res) ? res : 0,
            ReportNumber = dispatch.ReportNumber,
            AirwaryWillReference = dispatch.AirwaryWillReference,
            ReceiverId = dispatch.ReceiverId ?? 0,
            TransportModeId = dispatch.TransportModeId ?? 0,
            ReceivingModeId = dispatch.ReceivingModeId ?? 0,
            Note = dispatch.Note,
            Items = items.Select(i => new DispatchLineDto
            {
                ItemDetailStoreId = i.line.ItemDetailStoreId,
                ItemClassId = i.item.ItemClassId ?? 0,
                ItemName = i.item.FullName,
                ItemId = i.line.ItemId ?? 0,
                VvmstageName = i.vvmstage.FullName,
                ExpiryDate = i.line.ExpiryDate,
                DoseCount = i.line.Quantity ?? 0,
                VvmstageId = i.line.VvmstageId ?? 0,
                Note = i.line.Note,
                BatchNumber = i.line.BatchNumber,
                StoreId = i.line.StoreId

            }).ToList()
        };

        return model;
    }

    public Task<List<ToBeArrivedDispatchLineDto>> GetDispatchLinesAsync(long dispatchId, CancellationToken cancellationToken)
    {
        var query = from dispatchLine in _context.TblDispatchLines
                        .Where(dl => dl.DispatchId == dispatchId)
                        .AsNoTracking()

                    join itemDetailStore in _context.TblItemDetailStores
                        on dispatchLine.ItemDetailStoreId equals itemDetailStore.ItemDetailStoreId
                    join itemDetail in _context.TblItemDetails
                        on itemDetailStore.ItemDetailId equals itemDetail.ItemDetailId
                    join item in _context.TblItems
                        on dispatchLine.ItemId equals item.ItemId

                    join manufacturer in _context.TblManufacturers
                        on dispatchLine.ManufacturerId equals manufacturer.ManufacturerId
                        into manufacturerJoin
                    from manufacturer in manufacturerJoin.DefaultIfEmpty()

                    join doesPerVial in _context.TblDosePerVials
                        on dispatchLine.DosePerVialId equals doesPerVial.DosePerVialId
                        into doesPerVialJoin
                    from doesPerVial in doesPerVialJoin.DefaultIfEmpty()

                    join vvmStage in _context.TblVvmstages
                        on dispatchLine.VvmstageId equals vvmStage.VvmstageId
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
                        DoesPerVial = doesPerVial.NumDosePerVial,
                        ExpiryDate = dispatchLine.ExpiryDate,
                        Manufacure = manufacturer.FullName,
                        Vvmstage = vvmStage.FullName
                    };

        return query.ToListAsync(cancellationToken);
    }

    public async Task<(List<DispatchListDto> data, int count)> GetPagedDispatchesAsync(string? search, int page, int pageSize,
    string sortField, string sortOrder, string? status, int senderId, CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        // base query using joins
        var query = from d in _context.TblDispatches.Where(dispatch => dispatch.SenderId == senderId)
                    join r in _context.TblUsers on d.ReceiverId equals r.UserId into rj
                    from r in rj.DefaultIfEmpty()
                    join t in _context.TblTransportModes on d.TransportModeId equals t.TransportModeId into tj
                    from t in tj.DefaultIfEmpty()
                    join rm in _context.TblReceivingModes on d.ReceivingModeId equals rm.ReceivingModeId into rmj
                    from rm in rmj.DefaultIfEmpty()
                    select new DispatchListDto
                    {
                        Id = d.DispatchId,
                        Date = d.Date,
                        IsConfirmed = d.IsConfirmed,
                        DispatchVoucherNumber = d.DispatchVoucherNumber,
                        RequestNum = d.RequestNum,
                        TotalVolume = d.TotalVolume,
                        IsDrafted = d.IsDrafted,
                        ReceiverStatus = d.ReceiverStatus ?? 0,
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
        var query = from d in _context.TblDispatches.Where(dispatch => dispatch.ReceiverStatus == 0 &&
                            dispatch.ReceiverId == receiverId && dispatch.IsConfirmed.HasValue && dispatch.IsConfirmed.Value)
                    join r in _context.TblRecipients on d.SenderId equals r.RecipientId into rj
                    from r in rj.DefaultIfEmpty()
                    select new ToBeArrivedDispatchListDto
                    {
                        Id = d.DispatchId,
                        Date = d.Date,
                        DispatchVoucherNumber = d.DispatchVoucherNumber,
                        RequestNum = d.RequestNum,
                        TotalVolume = d.TotalVolume,
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

}
