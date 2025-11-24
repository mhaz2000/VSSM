using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.RecipientMaxItem;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IRecipientMaxItemService
{
    Task<int> Create(RecipientMaxItemDto dto, CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<RecipientMaxItemDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);

    Task<RecipientMaxItemDto> GetById(int id, CancellationToken cancellationToken);
    Task<bool> Update(RecipientMaxItemDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}