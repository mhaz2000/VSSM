using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ItemDetailStores;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ItemDetailStoreService : IItemDetailStoreService
{
    private readonly VSSMDbContext _db;

    public ItemDetailStoreService(VSSMDbContext db)
    {
        _db = db;
    }

    public async Task<ItemDetailStoreDto> GetById(long id)
    {
        var itemDetailStore =
            await _db.TblItemDetailStores.FirstOrDefaultAsync(c =>
                c.ItemDetailStoreId == id);

        return new ItemDetailStoreDto()
        {
            ItemDetailId = itemDetailStore.ItemDetailId,
            EntryDate = itemDetailStore.EntryDate.Value,
            ItemDetailStoreId = itemDetailStore.ItemDetailStoreId,
            CurrentStock = itemDetailStore.CurrentStock ?? 0,
            StoreId = itemDetailStore.StoreId
        };
    }
}