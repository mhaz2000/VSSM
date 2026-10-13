using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.MinMaxItem;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IMinMaxItemService
{
    Task<PagingSearchOrderDto<MinMaxItemListDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken token);

    Task<MinMaxItemGetDto?> GetById(int id, CancellationToken token);

    Task<int> Create(MinMaxItemCreateDto dto, CancellationToken token);

    Task Update(MinMaxItemEditDto dto, CancellationToken token);

    Task<bool> DeleteAsync(int id, CancellationToken token);
}
