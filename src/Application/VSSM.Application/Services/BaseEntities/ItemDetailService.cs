using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Entities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ItemDetailService : IItemDetailsService
    {
        private readonly VSSMDbContext _db;

        public ItemDetailService(VSSMDbContext db)
        {
            _db = db;
        }
        public async Task<List<ItemDetailsDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _db.TblItemDetails
                                  .AsNoTracking()
                                  .ToListAsync(cancellationToken);

            var result = items.Select(x => new ItemDetailsDto
            {
                Id = x.ItemDetailId,
                Name = x.UnitVolume,
                itemid = x.ItemId,


            }).ToList();

            return result;
        }

        public async Task<List<ToDispatchItemDetailDto>> GetItemDetailsAsync(int itemId, int recipientId, CancellationToken cancellationToken)
        {
            var query = from itm in _db.TblItems
                        join itmDtl in _db.TblItemDetails.Where(t => t.ItemId == itemId) on itm.ItemId equals itmDtl.ItemId
                        join itmDtlStr in _db.TblItemDetailStores.Where(t => t.CurrentStock > 0) on itmDtl.ItemDetailId equals itmDtlStr.ItemDetailId into itemStoreInfo
                        from i in itemStoreInfo.DefaultIfEmpty()
                        join str in _db.TblStores.Where(store => store.RecipientId == recipientId) on i.StoreId equals str.StoreId into itemAndStore
                        from s in itemAndStore.DefaultIfEmpty()
                        join manufacture in _db.TblManufacturers on itmDtl.ManufacturerId equals manufacture.ManufacturerId into manufactureInfo
                        from m in manufactureInfo.DefaultIfEmpty()
                        join stage in _db.TblVvmstages on i.VvmstageId equals stage.VvmstageId into stageInfo
                        from si in stageInfo.DefaultIfEmpty()
                        select new ToDispatchItemDetailDto
                        {
                            ItemId = itmDtl.ItemId,
                            ItemName = itm.FullName,
                            ExpiryDate = itmDtl.ExpiryDate,
                            VVMStep = si.FullName,
                            CurrentStock = i.CurrentStock ?? 0,
                            BatchNumber = itmDtl.BatchNumber,
                            StoreName = s.Name,
                            ItemDetailStoreId= i.ItemDetailStoreId,
                            StoreId = s.StoreId,
                            Manufacture = m.FullName
                        };

            var itemDetails = await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return itemDetails;
        }
    }
}
