using VSSM.Application.DTOs.BaseEntities.RecipientConfiguration;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IRecipientConfigurationService
    {
        Task<RecipientConfigurationDto?> GetSingle(CancellationToken token);
        Task<RecipientConfigurationDto?> GetById(int id, CancellationToken token);
        Task<int> Create(RecipientConfigurationCreateDto dto, CancellationToken token);
        Task<bool> Update(RecipientConfigurationEditDto dto, CancellationToken token);
    }
}
