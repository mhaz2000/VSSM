using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDispatchService
{

    Task DeleteDispatchAsync(int id, CancellationToken cancellationToken);
    Task ConfirmDispatchAsync(int id, CancellationToken cancellationToken);
    Task<(int dispatchId, long seqPreview)> CreateDispatchAsync(CreateDispatchDto createDispatcheDto, string submitType, CancellationToken cancellationToken);
    Task<(List<DispatchListDto> data, int count)> GetPagedDispatchesAsync(string? search, int page, int pageSize,
        string sortField, string sortOrder, string? status, int senderId, CancellationToken cancellationToken);

    Task<(List<ToBeArrivedDispatchListDto> data, int count)> GetPagedToBeArrivedDispatchesAsync(string? search, int page, int pageSize,
        string sortField, string sortOrder, int receiverId, CancellationToken cancellationToken);
    Task<List<ToBeArrivedDispatchLineDto>> GetDispatchLinesAsync(long id, CancellationToken cancellationToken);
    Task DispatchArrivedAsync(DispatchArrivedDto arrivedDto, int userId, int recipientId, CancellationToken cancellationToken);
    Task<UpdateDispatchDto?> GetByIdAsync(int id);
    Task UpdateDispatchAsync(UpdateDispatchDto updateDispatcheDto, string submitType, CancellationToken cancellationToken);
    Task<List<ItemLookupDto>> GetItemsByClassAsync(int classId, CancellationToken ct = default);
    Task<List<BatchRowDto>> GetBatchesByItemAsync(
            int itemId,
            int recipientId,
            CancellationToken ct = default);
    Task DecrementStockOnFinalAsync(List<DispatchLineDto> lines, CancellationToken ct = default);
    Task RestoreStockAsync(int dispatchId, CancellationToken ct = default);
    Task LinkUidsToDispatchUidAsync(int dispatchId, CancellationToken ct = default);
    Task<(bool isSuccess, string message, ScannedPackage data)> ValidateUidAsync(
       int? dispatchLineId,
       int? itemDetailStoreId,
       string barcode,
       int recipientId);

    Task<(bool isSuccess, string message, int count, int itemId)> SetUidAsync(
        string sessionId,
        int? dispatchLineId,
        int? itemDetailStoreId,
        string barcode,
        int recipientId);

    Task<(bool success, string message, int count)> ConfirmDispatchUidAsync(
        string sessionId,
        int dispatchId);

    Task<bool> RemoveUidFromSessionAsync(string sessionId, string uid);

    Task<List<DispatchEditLineDto>> GetDispatchLinesForEditAsync(int dispatchId, CancellationToken ct = default);

    Task<DispatchPrintDto?> GetDispatchPrintDataAsync(int id);
    Task<string> GenerateReportNumberAsync();
}
