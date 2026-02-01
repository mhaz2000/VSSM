using VSSM.Application.DTOs.BaseEntities.ItemDetailStores;
using VSSM.Application.DTOs.BaseEntities.StoreTransfers;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IItemDetailStoreService
{
    Task<ItemDetailStoreDto> GetById(long id);
    Task<List<ItemDetailStoreDto>> GetActives(long userRecipientId);
    Task<List<ItemDetailStoreDto>> GetActivesByStoreId(int storeId, CancellationToken cancellationToken);
    Task TransferItems(StoreTransfersDto dto);
}