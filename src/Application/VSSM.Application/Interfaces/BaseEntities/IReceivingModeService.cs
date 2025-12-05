using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ReceivingMode;
using VSSM.Application.DTOs.BaseEntities.TransportMode;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IReceivingModeService
{
   
    public Task CreateAsync(CreateReceivingModeDto dto, CancellationToken cancellationToken);
    public Task<List<ReceivingModeDto>> GetAllAsync(CancellationToken cancellationToken);
    public Task<ReceivingModeDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    public Task<PagingSearchOrderDto<ReceivingModeDto>> GetPagedListAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);
    public Task UpdateAsync(UpdateReceivingModeDto dto, CancellationToken cancellationToken);
}
