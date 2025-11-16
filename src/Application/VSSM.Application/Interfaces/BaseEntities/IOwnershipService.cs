using VSSM.Application.DTOs;

public interface IOwnershipService
{
    Task<PagingSearchOrderDto<OwnershipGetDto>> GetPagedList(string? search,
        int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);

    Task<OwnershipEditDto> GetById(int id, CancellationToken cancellationToken);

    Task<int> Create(OwnershipCreateDto dto, CancellationToken cancellationToken);

    Task<bool> Update(OwnershipEditDto dto, CancellationToken cancellationToken);
}
