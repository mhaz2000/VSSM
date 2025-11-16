using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.TransportMode;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface ITransportModeService
{
    Task CreateAsync(CreateTransportModeDto dto, CancellationToken cancellationToken);
    Task<List<TransportModeDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TransportModeDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<TransportModeDto>> GetPagedListAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);
    Task UpdateAsync(UpdateTransportModeDto dto, CancellationToken cancellationToken);
}
