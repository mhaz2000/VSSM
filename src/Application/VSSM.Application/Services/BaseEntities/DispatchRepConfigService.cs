using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.DispatchRepConfig;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class DispatchRepConfigService : IDispatchRepConfigService
{
    private readonly VSSMDbContext _context;

    public DispatchRepConfigService(VSSMDbContext context)
    {
        _context = context;
    }

    public async Task<DispatchRepConfigDto?> GetAsync()
    {
        var entity = await _context.TblDispatchRepConfigs.FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return new DispatchRepConfigDto
        {
            DispatchDate = entity.DispatchDate ?? false,
            Recipient = entity.Recipient ?? false,
            RequestNumber = entity.RequestNumber ?? false,
            ItemCode = entity.ItemCode ?? false,
            ItemClass = entity.ItemClass ?? false,
            ItemName = entity.ItemName ?? false,
            BatchNumber = entity.BatchNumber ?? false,
            VoucherNumber = entity.VoucherNumber ?? false,
            Quantity = entity.Quantity ?? false,
            DosePerVial = entity.DosePerVial ?? false,
            NumberOfVial = entity.NumberOfVial ?? false,
            ExpDate = entity.ExpDate ?? false,
            Store = entity.Store ?? false,
            Activity = entity.Activity ?? false
        };
    }

    public async Task<bool> CreateOrUpdateAsync(DispatchRepConfigDto dto)
    {
        var sql = @"
IF EXISTS (SELECT 1 FROM TblDispatchRepConfig)
    UPDATE TblDispatchRepConfig
    SET DispatchDate = {0},
        Recipient = {1},
        RequestNumber = {2},
        ItemCode = {3},
        ItemClass = {4},
        ItemName = {5},
        BatchNumber = {6},
        VoucherNumber = {7},
        Quantity = {8},
        DosePerVial = {9},
        NumberOfVial = {10},
        ExpDate = {11},
        Store = {12},
        Activity = {13}
ELSE
    INSERT INTO TblDispatchRepConfig
    (DispatchDate, Recipient, RequestNumber, ItemCode, ItemClass, ItemName, BatchNumber, VoucherNumber, Quantity, DosePerVial, NumberOfVial, ExpDate, Store, Activity)
    VALUES
    ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13})";

        await _context.Database.ExecuteSqlRawAsync(sql,
            dto.DispatchDate,
            dto.Recipient,
            dto.RequestNumber,
            dto.ItemCode,
            dto.ItemClass,
            dto.ItemName,
            dto.BatchNumber,
            dto.VoucherNumber,
            dto.Quantity,
            dto.DosePerVial,
            dto.NumberOfVial,
            dto.ExpDate,
            dto.Store,
            dto.Activity);

        return true;
    }
}