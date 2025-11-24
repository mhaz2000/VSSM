using VSSM.Application.DTOs.BaseEntities.ArrivalRepConfig;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IArrivalRepConfigService
{
    Task<ArrivalRepConfigDto?> GetAsync();
    Task<bool> SaveAsync(ArrivalRepConfigDto dto);
}
