using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ItemClass;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IItemClassService
    {
        Task<int> Create(ItemClassCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(ItemClassCreateDto dto, CancellationToken cancellationToken);
        Task<List<ItemClassListDto>> GetAll(CancellationToken cancellationToken);
        Task<List<ItemClassListDto>> GetActives(CancellationToken cancellationToken);
        Task<ItemClassSelectDto> GetById(int id, CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<ItemClassListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    }
}
