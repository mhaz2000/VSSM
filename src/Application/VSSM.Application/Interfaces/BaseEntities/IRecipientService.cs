using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.DTOs.BaseEntities.TransportMode;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IRecipientService
    {
        Task<List<RecipientGetDto>> GetAll(CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<RecipientGetDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken token);

        Task<RecipientEditDto> GetById(int id, CancellationToken token);

        Task<int> Create(RecipientCreateDto dto, CancellationToken token);

        Task<bool> Update(RecipientEditDto dto, CancellationToken token);
    }
}
