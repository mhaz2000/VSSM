using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.TransportMode;

public interface IOwnershipService
{
    Task<List<OwnershipGetDto>> GetAll(CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<OwnershipGetDto>> GetPagedList(string? search,
        int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);

    Task<OwnershipEditDto> GetById(int id, CancellationToken cancellationToken);

    Task<int> Create(OwnershipCreateDto dto, CancellationToken cancellationToken);

    Task<bool> Update(OwnershipEditDto dto, CancellationToken cancellationToken);
}
