using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IItemDoseAssignmentService
    {
        Task<int> Create(ItemDoseAssignmentCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(ItemDoseAssignmentCreateDto dto, CancellationToken cancellationToken);
        Task<List<ItemDoseAssignmentListDto>> GetAll(CancellationToken cancellationToken);
        Task<ItemDoseAssignmentSelectDto> GetById(int id, CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<ItemDoseAssignmentListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
        Task<bool> Delete(int id, CancellationToken cancellationToken);
    }
}
