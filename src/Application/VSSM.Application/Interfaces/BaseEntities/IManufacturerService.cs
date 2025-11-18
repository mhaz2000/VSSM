using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Manufacturer;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IManufacturerService
{
    Task<int> Create(ManufacturerDto dto, CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<ManufacturerDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);

    Task<ManufacturerDto> GetById(int id, CancellationToken cancellationToken);
    Task<bool> Update(ManufacturerDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}