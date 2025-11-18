using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.StoreType;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IStoreTypeService
{
    Task<int> Create(StoreTypeDto dto, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<StoreTypeDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    Task<StoreTypeDto> GetById(int id, CancellationToken cancellationToken);
    Task<bool> Update(StoreTypeDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}