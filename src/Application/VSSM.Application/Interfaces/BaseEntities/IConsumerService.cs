using VSSM.Application.DTOs.BaseEntities.Consumer;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IConsumerService
{
    Task<ConsumeUidResponse> ConsumeUidAsync(ConsumeUidRequest request, CancellationToken token);
}