using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.StoreUsageWarning;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class StoreUsageWarningService : IStoreUsageWarningService
{
    private readonly VSSMDbContext _context;

    public StoreUsageWarningService(VSSMDbContext context)
    {
        _context = context;
    }

    public async Task<StoreUsageWarningDto> GetAsync()
    {
        var entity = await _context.TblStoreUsageWarnings.FirstOrDefaultAsync();

        if (entity == null)
        {
            // Insert default row if none exists
            var sql = @"
INSERT INTO TblStoreUsageWarning (Dry, Bz, Az)
VALUES (0, 0, 0)";
            await _context.Database.ExecuteSqlRawAsync(sql);

            entity = new TblStoreUsageWarning { Dry = 0, Bz = 0, Az = 0 };
        }

        return new StoreUsageWarningDto
        {
            Dry = entity.Dry ?? 0,
            Bz = entity.Bz ?? 0,
            Az = entity.Az ?? 0
        };
    }

    public async Task CreateOrUpdateAsync(StoreUsageWarningDto dto)
    {
        var sql = @"
IF EXISTS (SELECT 1 FROM TblStoreUsageWarning)
    UPDATE TblStoreUsageWarning
    SET Dry = {0}, Bz = {1}, Az = {2}
ELSE
    INSERT INTO TblStoreUsageWarning (Dry, Bz, Az)
    VALUES ({0}, {1}, {2})";

        await _context.Database.ExecuteSqlRawAsync(sql, dto.Dry, dto.Bz, dto.Az);
    }
}