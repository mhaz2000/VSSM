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
    CreateDispatcheDto createDispatcheDto,
    CancellationToken cancellationToken)
    {
        var fiscalPeriod = _context.TblFiscalPeriods.FirstOrDefaultAsync(f => f.IsActive.HasValue && f.IsActive.Value);

        var dispatch = new TblDispatch
        {
            SenderId = createDispatcheDto.SenderId,
            ReceiverId = createDispatcheDto.ReceiverId,
            FiscalPeriodId = fiscalPeriod.Id,
            TransportModeId = createDispatcheDto.TransportModeId,
            EnteredById = createDispatcheDto.EnteredById,
            AirwaryWillReference = createDispatcheDto.AirwaryWillReference,
            RequestNum = createDispatcheDto.RequestNum,
            Note = createDispatcheDto.Note,
            Date = string.IsNullOrWhiteSpace(createDispatcheDto.Date) ? null : DateTimeHelper.ToGregorian(createDispatcheDto.Date),
            ArrivalId = -1,
            EntryDate = DateTime.Now,
            IsDrafted = true,
            ReceivingModeId = createDispatcheDto.ReceivingModeId,
            IsCancelled = false,
            IsConfirmed = false,
            DispatchVoucherPrinted = false,
            ConfirmationTime = DateTime.Now,
            DispatchVoucherNumber = createDispatcheDto.DispatchVoucherNumber.ToString(),
            ReportNumber = createDispatcheDto.ReportNumber
        };

        float totalPrice = 0;
        float totalVolume = 0;

        foreach (var item in createDispatcheDto.Items)
        {
            var itemInfo = await _context.TblItems.FindAsync(item.ItemId);

            var line = new TblDispatchLine
            {
                ItemId = item.ItemId,
                Quantity = item.DoseCount,
                NumVial = item.VialCount,
                DosePerVialId = item.DosePerVialId,
                TotalVolume = item.Volume,
                StoreId = item.StoreId,
                UseId = item.UseId,
                VvmtypeId = item.VvmtypeId,
                VvmstageId = item.VvmstageId,
                ManufacturerId = item.ManufacturerId,
                ItemDetailStoreId = item.ItemDetailStoreId,
                Note = item.Note,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate,
                BundleId = -1,
                EntryDate = DateTime.Now
            };

            if (itemInfo.DiluentId is not null)
            {
                var diluentLine = new TblDispatchLine
                {
                    ItemId = itemInfo.DiluentId,
                    Quantity = item.DoseCount,
                    NumVial = item.VialCount,
                    DosePerVialId = item.DosePerVialId,
                    TotalVolume = item.Volume,
                    StoreId = item.StoreId,
                    UseId = item.UseId,
                    VvmtypeId = item.VvmtypeId,
                    VvmstageId = item.VvmstageId,
                    ManufacturerId = item.ManufacturerId,
                    ItemDetailStoreId = item.ItemDetailStoreId,
                    Note = item.Note,
                    BatchNumber = item.BatchNumber,
                    ExpiryDate = item.ExpiryDate,
                    BundleId = -1,
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
                        ReportNumber = d.ReportNumber,
                        RequestNum = d.RequestNum,
                        TotalVolume = d.TotalVolume,
                        IsDrafted = d.IsDrafted,
                        IsConfirmed = d.IsConfirmed,
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

}
