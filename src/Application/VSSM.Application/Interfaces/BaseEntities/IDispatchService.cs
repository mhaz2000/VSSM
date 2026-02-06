using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Dispatches;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDispatchService
{
    Task DeleteDispatchAsync(int id, CancellationToken cancellationToken);
    Task ConfirmDispatchAsync(int id, CancellationToken cancellationToken);
    Task CreateDispatchAsync(CreateDispatchDto createDispatcheDto, string submitType, CancellationToken cancellationToken);
    Task<DispatchPagingSearchOrderDto> GetPagedDispatchesAsync(string? search, int page, int pageSize,
        string sortField, string sortOrder, string? status, int senderId, CancellationToken cancellationToken);

    Task<PagingSearchOrderDto<ToBeArrivedDispatchListDto>> GetPagedToBeArrivedDispatchesAsync(string? search, int page, int pageSize,
        string sortField, string sortOrder, string? status, int receiverId, CancellationToken cancellationToken);
    Task<List<ToBeArrivedDispatchLineDto>> GetDispatchLinesAsync(long id, CancellationToken cancellationToken);
    Task DispatchArrivedAsync(DispatchArrivedDto arrivedDto, int userId, int recipientId, CancellationToken cancellationToken);
}
