using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.JobType;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IJobTypeService
    {
        Task<PagingSearchOrderDto<JobTypeGetDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken);

        Task<JobTypeEditDto> GetById(int id, CancellationToken token);

        Task<int> Create(JobTypeCreateDto dto, CancellationToken token);

        Task<bool> Update(JobTypeEditDto dto, CancellationToken token);
    }
}
