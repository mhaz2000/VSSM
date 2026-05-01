using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemClass;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IItemService
    {
        Task<int> Create(ItemCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(ItemCreateDto dto, CancellationToken cancellationToken);
        Task<List<ItemListDto>> GetAll(CancellationToken cancellationToken);
        Task<ItemSelectDto> GetById(int id, CancellationToken cancellationToken);
        Task<List<ItemListDto>> GetActives(CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<ItemListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
        Task<List<ItemListDto>> GetItemsByClassId(int classItemId);
        Task<string> GetItemNamesAsync(List<int> itemIds);
    }
}
