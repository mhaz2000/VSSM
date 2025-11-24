using VSSM.Application.DTOs.BaseEntities.VoucherConfig;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IVoucherConfigService
{
    Task<VoucherConfigDto?> GetAsync();
    Task<VoucherConfigDto> AddOrUpdateAsync(VoucherConfigDto dto);
}
