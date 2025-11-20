using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Store;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IStoreService
    {
        Task<List<StoreListDto>> GetAll(CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<StoreListDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken);

        Task<StoreEditDto> GetById(int id, CancellationToken cancellationToken);

        Task<int> Create(StoreCreateDto dto, CancellationToken cancellationToken);

        Task<bool> Update(StoreEditDto dto, CancellationToken cancellationToken);
    }
}
