using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IItemDoseAssignmentService
    {
        Task<int> Create(ItemDoseAssignmentCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(ItemDoseAssignmentCreateDto dto, CancellationToken cancellationToken);
        Task<List<ItemDoseAssignmentListDto>> GetAll(CancellationToken cancellationToken);
        Task<List<ItemDoseAssignmentListDto>> GetActives(CancellationToken cancellationToken);
        Task<ItemDoseAssignmentSelectDto> GetById(int id, CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<ItemDoseAssignmentListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    }
}
