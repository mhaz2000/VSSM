using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.Interfaces.BaseEntities;
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
                        join itmDtl in _db.TblItemDetails.Where(t => t.ItemId == itemId)
                            on itm.ItemId equals itmDtl.ItemId
                        join itmDtlStr in _db.TblItemDetailStores.Where(t => t.CurrentStock > 0)
                            on itmDtl.ItemDetailId equals itmDtlStr.ItemDetailId into itemStoreInfo
                        from i in itemStoreInfo.DefaultIfEmpty()
                        join str in _db.TblStores.Where(store => store.RecipientId == recipientId)
                            on i.StoreId equals str.StoreId into itemAndStore
                        from s in itemAndStore
                        join manufacture in _db.TblManufacturers
                            on itmDtl.ManufacturerId equals manufacture.ManufacturerId into manufactureInfo
                        from m in manufactureInfo.DefaultIfEmpty()
                        join stage in _db.TblVvmstages
                            on i.VvmstageId equals stage.VvmstageId into stageInfo
                        from si in stageInfo.DefaultIfEmpty()
                        join dpv in _db.TblDosePerVials
                            on itmDtl.DosePerVialId equals dpv.DosePerVialId into dosePervials
                        from dosePervial in dosePervials.DefaultIfEmpty()

                        let diluent = (from dItem in _db.TblItems
                                       where dItem.ItemId == itm.DiluentId
                                       join dDtl in _db.TblItemDetails
                                           on dItem.ItemId equals dDtl.ItemId
                                       join dStore in _db.TblItemDetailStores
                                           .Where(ds => ds.CurrentStock > 0)
                                           on dDtl.ItemDetailId equals dStore.ItemDetailId into dStoreInfo
                                       from ds in dStoreInfo.DefaultIfEmpty()
                                       join dStr in _db.TblStores.Where(store => store.RecipientId == recipientId)
                                           on ds.StoreId equals dStr.StoreId into dStoreFiltered
                                       from dS in dStoreFiltered
                                       join dMan in _db.TblManufacturers
                                           on dDtl.ManufacturerId equals dMan.ManufacturerId into dManInfo
                                       from dm in dManInfo.DefaultIfEmpty()
                                       join dStage in _db.TblVvmstages
                                           on ds.VvmstageId equals dStage.VvmstageId into dStageInfo
                                       from dsi in dStageInfo.DefaultIfEmpty()
                                       join dDpv in _db.TblDosePerVials
                                           on dDtl.DosePerVialId equals dDpv.DosePerVialId into dDoseInfo
                                       from ddpv in dDoseInfo.DefaultIfEmpty()
                                       select new ToDispatchItemDetailDto
                                       {
                                           NumDosePerVial = ddpv == null ? 1 : ddpv.NumDosePerVial,
                                           ItemId = dDtl.ItemId,
                                           ItemName = dItem.FullName,
                                           ExpiryDate = dDtl.ExpiryDate,
                                           VVMStep = dsi.FullName,
                                           CurrentStock = ds.CurrentStock ?? 0,
                                           BatchNumber = dDtl.BatchNumber,
                                           StoreName = dS.Name,
                                           ItemDetailStoreId = ds.ItemDetailStoreId,
                                           StoreId = dS.StoreId,
                                           Manufacture = dm.FullName
                                       }).FirstOrDefault() 

                        select new ToDispatchItemDetailDto
                        {
                            NumDosePerVial = dosePervial == null ? 1 : dosePervial.NumDosePerVial,
                            ItemId = itmDtl.ItemId,
                            DiluentId = itm.DiluentId,
                            ItemName = itm.FullName,
                            ExpiryDate = itmDtl.ExpiryDate,
                            VVMStep = si.FullName,
                            CurrentStock = i.CurrentStock ?? 0,
                            BatchNumber = itmDtl.BatchNumber,
                            StoreName = s.Name,
                            ItemDetailStoreId = i.ItemDetailStoreId,
                            StoreId = s.StoreId,
                            Manufacture = m.FullName,
                            Diluent = diluent
                        };

            var itemDetails = await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return itemDetails;
        }
    }
}
