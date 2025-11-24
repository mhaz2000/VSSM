using VSSM.Application.DTOs.BaseEntities.StoreUsageWarning;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IStoreUsageWarningService
{
    Task<StoreUsageWarningDto> GetAsync();
    Task CreateOrUpdateAsync(StoreUsageWarningDto dto);
}