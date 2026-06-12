using VSSM.Application.DTOs.BaseEntities.DestroyUid;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDestroyUidService
{
    Task<(bool Success, string Message, List<DestroyUidSearchResultDto> Data)>
        SearchByBatchAsync(string batchNumber, int userStoreId, CancellationToken token);

    Task<(bool Success, string Message, List<DestroyUidSearchResultDto> Data)>
        SearchByUidAsync(string uid, int userStoreId, CancellationToken token);

    Task<List<(int Id, string Title)>>
        GetDestroyReasonsAsync(CancellationToken token);


    Task<int> CountDestroyableUidsAsync(
        List<DestroyUidSubmitDto> items, int userStoreId, CancellationToken token);

    Task<(bool Success, string Message)> SubmitDestroyAsync(
        List<DestroyUidSubmitDto> items, int userId, int userStoreId, CancellationToken token,
        Func<int, Task>? onProgress = null);
}
