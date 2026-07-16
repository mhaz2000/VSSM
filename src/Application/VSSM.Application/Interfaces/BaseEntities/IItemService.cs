using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.Item;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IItemService
{
    Task<int> Create(ItemCreateDto dto, CancellationToken cancellationToken);
    Task<bool> Update(ItemCreateDto dto, CancellationToken cancellationToken);
    Task<List<ItemListDto>> GetAll(CancellationToken cancellationToken);
    Task<ItemSelectDto> GetById(int id, CancellationToken cancellationToken);
    Task<List<ItemListDto>> GetActives(CancellationToken cancellationToken);
    Task<List<ItemListDto>> GetActiveVaccines(CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<ItemListDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            string? status,
            CancellationToken cancellationToken);
    Task<List<ItemListDto>> GetItemsByClassAsync(int classItemId);
    Task<string> GetItemNamesAsync(List<int> itemIds);
    Task<IEnumerable<DosePerVialListDto>> GetItemDoesPerVialsAsync(int itemId, CancellationToken token);
}
