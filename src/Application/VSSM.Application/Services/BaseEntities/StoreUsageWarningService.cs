using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.StoreUsageWarning;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class StoreUsageWarningService(
    VSSMDbContext context) : IStoreUsageWarningService
{
    public async Task<StoreUsageWarningDto> GetAsync()
    {
        var entity = await context.TblStoreUsageWarning
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            entity = new TblStoreUsageWarning
            {
                Dry = 0,
                Bz = 0,
                Az = 0
            };

            context.TblStoreUsageWarning.Add(entity);
            await context.SaveChangesAsync();
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
        var entity = await context.TblStoreUsageWarning
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            entity = new TblStoreUsageWarning
            {
                Dry = dto.Dry,
                Bz = dto.Bz,
                Az = dto.Az
            };

            context.TblStoreUsageWarning.Add(entity);
        }
        else
        {
            entity.Dry = dto.Dry;
            entity.Bz = dto.Bz;
            entity.Az = dto.Az;
        }

        await context.SaveChangesAsync();
    }
}