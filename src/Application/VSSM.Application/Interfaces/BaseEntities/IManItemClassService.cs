using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IManItemClassService
{
    Task<long> Create(ManItemClassDto dto, CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<ManItemClassDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken);

    Task<ManItemClassDto?> GetById(long id, CancellationToken cancellationToken);
    Task<List<ManufacturerDropDownDto>> ManufacturerDropDown(CancellationToken cancellationToken);
    Task<bool> Update(ManItemClassDto dto, CancellationToken cancellationToken);
    Task<bool> Delete(long id, CancellationToken cancellationToken);
}