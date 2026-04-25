using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Store;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IStoreService
    {
        Task<List<StoreListDto>> GetAll(CancellationToken cancellationToken);
        public Task<PagingSearchOrderDto<StoreListDto>> GetPagedList(
      string? search,
      int? typeId,
      int page,
      int pageSize,
      CancellationToken token);

        Task<StoreEditDto> GetById(int id, CancellationToken cancellationToken);

        Task<int> Create(StoreCreateDto dto, CancellationToken cancellationToken);

        Task<bool> Update(StoreEditDto dto, CancellationToken cancellationToken);
        Task<List<StoreListDto>> GetAllWithRecipientId(int recipientId, CancellationToken cancellationToken);

        Task<(List<StoreEquipmentReportDto> data, int count)> GetStoreEquipmentReport(int currentRecipientId,
            int? recipientId,
            int? storeTypeId,
            int? volumeStart,
            int? volumeEnd,
            int? freeVolumeStart,
            int? freeVolumeEnd,
            int? yearStart,
            int? yearEnd,
            bool isActive,
            int pageNumber,
            int pageSize,
            string sort = "Recipient",
            string direction = "asc",
            CancellationToken cancellationToken = default);

    }
}
