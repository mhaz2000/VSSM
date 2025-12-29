using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Adjustments;

namespace VSSM.Application.Services.BaseEntities;

public interface IAdjustmentService
{
    Task<int> Create(
        AdjustmentDto dto,
        CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<AdjustmentDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);

    Task<AdjustmentDto?> GetById(
        int id,
        CancellationToken cancellationToken);

    Task<bool> Update(
        AdjustmentDto dto,
        CancellationToken cancellationToken);

    Task<bool> Delete(
        int id,
        CancellationToken cancellationToken);
}