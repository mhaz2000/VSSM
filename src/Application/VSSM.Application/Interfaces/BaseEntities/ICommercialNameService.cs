using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.CommercialName;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface ICommercialNameService
{
    Task<int> Create(CommercialNameDto dto, CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<CommercialNameDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);

    Task<CommercialNameDto> GetById(int id, CancellationToken cancellationToken);

    Task<bool> Update(CommercialNameDto dto, CancellationToken cancellationToken);

    Task<bool> Delete(int id, CancellationToken cancellationToken);
}