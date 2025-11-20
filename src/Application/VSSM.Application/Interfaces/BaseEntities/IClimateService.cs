using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Climate;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IClimateService
{
    Task CreateAsync(CreateClimateDto dto, CancellationToken cancellationToken);
    Task<List<ClimateDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ClimateDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagingSearchOrderDto<ClimateDto>> GetPagedListAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);

    Task UpdateAsync(UpdateClimateDto dto, CancellationToken cancellationToken);
}
