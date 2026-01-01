using VSSM.Application.DTOs.BaseEntities.Dispatches;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDispatchService
{
    Task CreateDispatchAsync(CreateDispatcheDto createDispatcheDto, CancellationToken cancellationToken);
}
