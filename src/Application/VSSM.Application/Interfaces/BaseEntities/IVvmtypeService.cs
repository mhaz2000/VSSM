using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Vvmtype;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IVvmtypeService
{
    Task<int> Create(VvmtypeDto dto, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<VvmtypeDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    Task<VvmtypeDto> GetById(int id, CancellationToken cancellationToken);
    Task<bool> Update(VvmtypeDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}