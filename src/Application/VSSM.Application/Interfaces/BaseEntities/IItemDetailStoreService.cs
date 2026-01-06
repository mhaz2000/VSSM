using VSSM.Application.DTOs.BaseEntities.ItemDetailStores;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IItemDetailStoreService
{
    Task<ItemDetailStoreDto> GetById(long id);
}