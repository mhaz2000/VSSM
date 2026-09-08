using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Recipient;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IRecipientService
{
    Task<PagingSearchOrderDto<RecipientGetDto>> GetPagedList(
        string? search,
        int? levelId,           
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken token);

    Task<bool> ToggleActive(int id, CancellationToken token);          
    Task<bool> ExistsByCode(string? code, int? excludeId, CancellationToken token);  

    Task<List<RecipientSearchDto>> SearchActiveRecipientsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<RecipientEditDto?> GetById(int id, CancellationToken token);
    Task<int> Create(RecipientCreateDto dto, CancellationToken token);
    Task<bool> Update(RecipientEditDto dto, CancellationToken token);
    Task<List<RecipientGetDto>> GetAll(CancellationToken cancellationToken);
    Task<int?> GetChainLevelById(int id, CancellationToken token);
    Task<List<RecipientGetDto>> GetActiveRecipients(CancellationToken token);
    Task<List<RecipientGetDto>> GetActiveRecipientChildren(int currentRecipientId, CancellationToken token);
}