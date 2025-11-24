using VSSM.Application.DTOs.BaseEntities.DispatchRepConfig;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDispatchRepConfigService
{
    Task<DispatchRepConfigDto?> GetAsync();
    Task<bool> CreateOrUpdateAsync(DispatchRepConfigDto dto);
}