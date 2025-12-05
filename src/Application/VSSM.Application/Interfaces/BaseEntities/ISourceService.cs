using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Source;
using VSSM.Application.DTOs.BaseEntities.Store;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface ISourceService
    {
        Task<List<SourceGetDto>> GetAll(CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<SourceGetDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken token);
        Task<int> Create(SourceCreateDto dto, CancellationToken token);
        Task<bool> Update(SourceEditDto dto, CancellationToken token);
        Task<SourceEditDto> GetById(int id, CancellationToken token);
    }
}
