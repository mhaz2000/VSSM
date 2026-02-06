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

    public async Task CreateDispatchAsync(
    CreateDispatchDto createDispatcheDto,
    string submitType,
    CancellationToken cancellationToken)
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
            ConfirmationTime = DateTime.Now,
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

            if (itemInfo.DiluentId is not null && itemInfo.DiluentId > 0)
            {
                var diluentLine = new TblDispatchLine
                {
                    ItemId = itemInfo.DiluentId,
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
            }

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

        var query = from line in _context.TblDispatchLines
                    join dispatch in _context.TblDispatches on line.DispatchId equals dispatch.DispatchId
                    join itemDetailStore in _context.TblItemDetailStores on line.ItemDetailStoreId equals itemDetailStore.ItemDetailStoreId
                    join itemDetail in _context.TblItemDetails on itemDetailStore.ItemDetailId equals itemDetail.ItemDetailId
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

    public async Task<DispatchPagingSearchOrderDto> GetPagedDispatchesAsync(
    string? search,
    int page,
    int pageSize,
    string sortField,
    string sortOrder,
    string? status,
    int senderId,
    CancellationToken cancellationToken)
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
                        ReportNumber = d.ReportNumber,
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

            else if (status == "confirmed")
                query = query.Where(x => x.IsConfirmed == true);
        }

        // 1) Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.RequestNum.Contains(search) ||
                d.ReceiverName.Contains(search)
            );
        }

        // 2) Sorting
        query = (sortField, sortOrder.ToLower()) switch
        {
            ("Date", "asc") => query.OrderBy(d => d.Date),
            ("Date", "desc") => query.OrderByDescending(d => d.Date),
            ("RequestNum", "asc") => query.OrderBy(d => d.RequestNum),
            ("RequestNum", "desc") => query.OrderByDescending(d => d.RequestNum),
            ("ReceiverName", "asc") => query.OrderBy(d => d.ReceiverName),
            ("ReceiverName", "desc") => query.OrderByDescending(d => d.ReceiverName),
            _ => query.OrderByDescending(d => d.Date)
        };

        // 3) Total count before paging
        paging.TotalItems = await query.CountAsync(cancellationToken);

        // 4) Apply paging
        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return new DispatchPagingSearchOrderDto
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<PagingSearchOrderDto<ToBeArrivedDispatchListDto>> GetPagedToBeArrivedDispatchesAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        string? status,
        int receiverId,
        CancellationToken cancellationToken)
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
                        ReportNumber = d.ReportNumber,
                        RequestNum = d.RequestNum,
                        TotalVolume = d.TotalVolume,
                        EntryDate = d.EntryDate,
                        SenderName = r != null ? r.FullName : "-",
                    };


        // 1) Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.RequestNum.Contains(search) ||
                d.SenderName.Contains(search)
            );
        }

        // 2) Sorting
        query = (sortField, sortOrder.ToLower()) switch
        {
            ("Date", "asc") => query.OrderBy(d => d.Date),
            ("Date", "desc") => query.OrderByDescending(d => d.Date),
            ("RequestNum", "asc") => query.OrderBy(d => d.RequestNum),
            ("RequestNum", "desc") => query.OrderByDescending(d => d.RequestNum),
            ("SenderName", "asc") => query.OrderBy(d => d.SenderName),
            ("SenderName", "desc") => query.OrderByDescending(d => d.SenderName),
            _ => query.OrderByDescending(d => d.Date)
        };

        // 3) Total count before paging
        paging.TotalItems = await query.CountAsync(cancellationToken);

        // 4) Apply paging
        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<ToBeArrivedDispatchListDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

}
