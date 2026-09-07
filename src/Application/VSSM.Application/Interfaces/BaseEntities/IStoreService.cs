using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Store;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IStoreService
{
    Task<StoreEditDto?> GetById(int id, CancellationToken token);
    Task<int> Create(StoreCreateDto dto, CancellationToken token);
    Task<bool> Update(StoreEditDto dto, CancellationToken token);
    Task<bool> ToggleActive(int id, CancellationToken token);
    Task<PagingSearchOrderDto<StoreListDto>> GetPagedList(string? search, string? recipientId, int? typeId, int page, int pageSize, CancellationToken token);
    Task<List<StoreListDto>> GetAll(CancellationToken cancellationToken);
    Task<List<StoreListDto>> GetAllWithRecipientId(int recipientId, CancellationToken cancellationToken);
    Task<(List<StoreEquipmentReportDto> data, int count)> GetStoreEquipmentReport(
        int currentRecipientId,
        int? recipientId,
        int? storeTypeId,
        int? volumeStart,
        int? volumeEnd,
        int? freeVolumeStart,
        int? freeVolumeEnd,
        int? yearStart,
        int? yearEnd,
        int? isActive,
        int pageNumber,
        int pageSize,
        string sort = "Recipient",
        string direction = "asc",
        CancellationToken cancellationToken = default);
    Task<string> GetStoreNamesAsync(List<int> storeIds);
}