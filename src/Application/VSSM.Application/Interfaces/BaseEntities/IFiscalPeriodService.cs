using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;

namespace VSSM.Application.Interfaces.BaseEntities;
public interface IFiscalPeriodService
{
    Task<List<FiscalPeriodDto>> GetActives(CancellationToken cancellationToken);
    Task<int> Create(FiscalPeriodDto dto, CancellationToken cancellationToken);
    Task<bool> Update(FiscalPeriodDto dto, CancellationToken cancellationToken);
    Task<FiscalPeriodDto?> GetById(int id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<List<FiscalPeriodDto>> GetAllAsListAsync(CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<FiscalPeriodDto>> GetPagedList(
            string? search,
            string? status,
            int page,
            int pageSize,
            string sort,
            string direction,
            CancellationToken cancellationToken);
}