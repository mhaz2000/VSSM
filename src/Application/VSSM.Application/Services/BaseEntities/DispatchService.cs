using VSSM.Application.DTOs.BaseEntities.Dispatches;
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

    public async Task CreateDispatchAsync(
    CreateDispatcheDto createDispatcheDto,
    CancellationToken cancellationToken)
    {
        var dispatch = new TblDispatch
        {
            SenderId = createDispatcheDto.SenderId,
            ReceiverId = createDispatcheDto.ReceiverId,
            FiscalPeriodId = createDispatcheDto.FiscalPeriodId,
            TransportModeId = createDispatcheDto.TransportModeId,
            EnteredById = createDispatcheDto.EnteredById,
            AirwaryWillReference = createDispatcheDto.AirwaryWillReference,
            RequestNum = createDispatcheDto.RequestNum,
            Note = createDispatcheDto.Note,
            Date = string.IsNullOrWhiteSpace(createDispatcheDto.Date) ? null : DateTime.Parse(createDispatcheDto.Date),
            ArrivalId = createDispatcheDto.ArrivalId,
            EntryDate = DateTime.Now,
            IsDrafted = false,
            IsCancelled = false,
            IsConfirmed = false
        };

        float totalPrice = 0;
        float totalVolume = 0;

        foreach (var item in createDispatcheDto.DispatchLines)
        {
            var lineTotalPrice = item.Quantity * item.UnitPrice;

            var line = new TblDispatchLine
            {
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = lineTotalPrice,
                NumVial = item.NumVial,
                DosePerVialId = item.DosePerVialId,
                TotalVolume = item.TotalVolume,
                StoreId = item.StoreId,
                UseId = item.UseId,
                VvmtypeId = item.VvmtypeId,
                VvmstageId = item.VvmstageId,
                ManufacturerId = item.ManufacturerId,
                ItemDetailStoreId = item.ItemDetailStoreId,
                Note = item.Note,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate,
                BundleId = item.BundleId,
                EntryDate = DateTime.Now
            };

            totalPrice += lineTotalPrice;
            totalVolume += item.TotalVolume ?? 0;

            dispatch.TblDispatchLines.Add(line);
        }

        dispatch.TotalPrice = totalPrice;
        dispatch.TotalVolume = totalVolume;

        await _context.TblDispatches.AddAsync(dispatch, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
