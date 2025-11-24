using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.EduType;
using VSSM.Application.DTOs.BaseEntities.PowerSource;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IEduTypeService
    {
        Task<List<EduTypeGetDto>> GetAll(CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<EduTypeGetDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken);

        Task<EduTypeEditDto> GetById(int id, CancellationToken cancellationToken);

        Task<int> Create(EduTypeCreateDto dto, CancellationToken cancellationToken);

        Task<bool> Update(EduTypeEditDto dto, CancellationToken cancellationToken);
    }
}
