using VSSM.Application.DTOs.BaseEntities.Dispatches;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDispatchService
{

    Task<(bool isSuccess, string message)> ValidateUidAsync(int itemDetailStoreId, string uid);
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
    Task<(bool isSuccess, string message, int count, int itemId)> SetUidAsync(int dispatchLineId, int id, string uid);

    Task<List<ItemLookupDto>> GetItemsByClassAsync(int classId, CancellationToken ct = default);

    /// <summary>
    /// بند ۱۱، ۶، ۷a و ۱۰: لود سری ساخت‌های موجود به همراه اطلاعات حلال و موجودی جاری
    /// </summary>
    Task<List<BatchRowDto>> GetBatchesByItemAsync(int itemId, CancellationToken ct = default);

    /// <summary>
    /// بند ۱۰a و ۹: کسر موجودی از انبار هنگام ثبت نهایی
    /// </summary>
    Task DecrementStockOnFinalAsync(List<DispatchLineDto> lines, CancellationToken ct = default);

    /// <summary>
    /// بند ۱۰c: برگشت موجودی به انبار هنگام حذف حواله یا سطرهای آن
    /// </summary>
    Task RestoreStockAsync(int dispatchId, CancellationToken ct = default);

    /// <summary>
    /// بند ۹: لینک کردن UIDها به ردیف‌های حواله در جدول اطلاعات UID
    /// </summary>
    Task LinkUidsToDispatchUidAsync(int dispatchId, CancellationToken ct = default);
}
