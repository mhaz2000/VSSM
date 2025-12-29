using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.AdjustReason;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IAdjustReasonService
{
    Task<int> Create(AdjustreasonDto dto, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<AdjustreasonDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);
    Task<AdjustreasonDto?> GetById(int id, CancellationToken cancellationToken);
    Task<bool> Update(AdjustreasonDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(int id, CancellationToken cancellationToken);
    Task<List<AdjustreasonDto>> GetActives(CancellationToken cancellationToken);
}