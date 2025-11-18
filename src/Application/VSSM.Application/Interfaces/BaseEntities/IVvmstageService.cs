using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IVvmstageService
{
    Task<int> Create(VvmstageDto dto, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<VvmstageDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    Task<VvmstageDto> GetById(int id, CancellationToken cancellationToken);
    Task<bool> Update(VvmstageDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}