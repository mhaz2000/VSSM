using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IFiscalPeriodService
{
    Task<int> Create(FiscalPeriodDto dto, CancellationToken cancellationToken);
    Task<bool> Update(FiscalPeriodDto dto, CancellationToken cancellationToken);
    Task<FiscalPeriodDto?> GetById(int id, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<FiscalPeriodDto>> GetPagedList(
        string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);

    Task<bool> Delete(int id, CancellationToken cancellationToken);
    
}