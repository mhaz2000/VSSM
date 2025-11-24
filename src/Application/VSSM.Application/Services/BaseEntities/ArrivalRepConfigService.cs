using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ArrivalRepConfig;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ArrivalRepConfigService : IArrivalRepConfigService
{
    private readonly VSSMDbContext _context;

    public ArrivalRepConfigService(VSSMDbContext context)
    {
        _context = context;
    }

    public async Task<ArrivalRepConfigDto?> GetAsync()
    {
        var entity = await _context.TblArrivalRepConfigs.FirstOrDefaultAsync();

        if (entity == null)
            return new ArrivalRepConfigDto();

        return new ArrivalRepConfigDto
        {
            ReportNumber = entity.ReportNumber ?? false,
            Source = entity.Source ?? false,
            OrderPurchaseNumber = entity.OrderPurchaseNumber ?? false,
            ItemClass = entity.ItemClass ?? false,
            ItemCode = entity.ItemCode ?? false,
            ItemName = entity.ItemName ?? false,
            Manufacturer = entity.Manufacturer ?? false,
            BatchNumber = entity.BatchNumber ?? false,
            DosePerVial = entity.DosePerVial ?? false,
            NumberOfVial = entity.NumberOfVial ?? false,
            Quantity = entity.Quantity ?? false,
            UnitPrice = entity.UnitPrice ?? false,
            TotalPrice = entity.TotalPrice ?? false,
            Activity = entity.Activity ?? false,
            ExpireDate = entity.ExpireDate ?? false,
            UnitVolume = entity.UnitVolume ?? false,
            TotalVolume = entity.TotalVolume ?? false,
            ArrivalDate = entity.ArrivalDate ?? false
        };
    }

    public async Task<bool> SaveAsync(ArrivalRepConfigDto dto)
    {
        var sql = @"
IF EXISTS (SELECT 1 FROM TblArrivalRepConfig)
    UPDATE TblArrivalRepConfig
    SET 
        ReportNumber = {0},
        Source = {1},
        OrderPurchaseNumber = {2},
        ItemClass = {3},
        ItemCode = {4},
        ItemName = {5},
        Manufacturer = {6},
        BatchNumber = {7},
        DosePerVial = {8},
        NumberOfVial = {9},
        Quantity = {10},
        UnitPrice = {11},
        TotalPrice = {12},
        Activity = {13},
        ExpireDate = {14},
        UnitVolume = {15},
        TotalVolume = {16},
        ArrivalDate = {17}
ELSE
    INSERT INTO TblArrivalRepConfig
    (
        ReportNumber, Source, OrderPurchaseNumber, ItemClass, ItemCode, ItemName,
        Manufacturer, BatchNumber, DosePerVial, NumberOfVial, Quantity, UnitPrice,
        TotalPrice, Activity, ExpireDate, UnitVolume, TotalVolume, ArrivalDate
    )
    VALUES
    ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17})";

        await _context.Database.ExecuteSqlRawAsync(sql,
            dto.ReportNumber,
            dto.Source,
            dto.OrderPurchaseNumber,
            dto.ItemClass,
            dto.ItemCode,
            dto.ItemName,
            dto.Manufacturer,
            dto.BatchNumber,
            dto.DosePerVial,
            dto.NumberOfVial,
            dto.Quantity,
            dto.UnitPrice,
            dto.TotalPrice,
            dto.Activity,
            dto.ExpireDate,
            dto.UnitVolume,
            dto.TotalVolume,
            dto.ArrivalDate
        );

        return true;
    }
}
