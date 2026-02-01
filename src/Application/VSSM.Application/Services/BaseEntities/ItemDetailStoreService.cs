using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.BaseEntities.ItemDetailStores;
using VSSM.Application.Helpers;
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

    public async Task<List<ItemDetailStoreDto>> GetActivesByStoreId(int storeId, CancellationToken cancellationToken)
    {
        var itemsDetails = await _db.TblItemDetails
            .AsNoTracking()
            .Include(c => c.TblItemDetailStores)
            .Where(c => c.TblItemDetailStores.Any(v => v.StoreId == storeId))
            .ToListAsync(cancellationToken);

        var itemDetailsStores = itemsDetails.SelectMany(c => c.TblItemDetailStores);

        var result = new List<ItemDetailStoreDto>();
        foreach (var itemDetailsStore in itemDetailsStores)
        {
            var item = _db.TblItems.FirstOrDefault(c => c.ItemId == itemDetailsStore.ItemDetail.ItemId);

            if (item is null)
                continue;

            var dto = new ItemDetailStoreDto()
            {
                ItemId = item.ItemId,
                CurrentStock = itemDetailsStore.CurrentStock ?? 0,
                ItemDetailId = itemDetailsStore.ItemDetailId,
                StoreId = itemDetailsStore.StoreId,
                DropDownText = item.FullName,
                ItemDetailStoreId = itemDetailsStore.ItemDetailStoreId
            };
            
            result.Add(dto);
        }


        return result;
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


    public async Task<List<ItemDetailStoreDto>> GetActives(long userRecipientId)
    {
        var lines = await _db.TblArrivalLines
            .Where(l => _db.TblArrivals
                .Any(a => a.ReceiverId == userRecipientId && a.ArrivalId == l.ArrivalId))
            .Select(l => new
            {
                Line = l,
                Item = _db.TblItems.FirstOrDefault(i => i.ItemId == l.ItemId),
                Manufacturer = _db.TblManufacturers.FirstOrDefault(m => m.ManufacturerId == l.ManufacturerId),
                Store = _db.TblStores.FirstOrDefault(s => s.StoreId == l.StoreId),
                DoseVial = _db.TblDosePerVials.FirstOrDefault(d => d.DosePerVialId == l.DosePerVialId),
                ItemDetail = _db.TblItemDetails.FirstOrDefault(d => d.ItemDetailId == l.ItemDetailId)
            })
            .AsNoTracking()
            .ToListAsync();

        var keys = lines
            .Select(c => new
            {
                StoreId = (int)c.Store.StoreId,
                ItemDetailId = (int)c.Line.ItemDetailId
            })
            .ToList();

        var itemDetailStores =
            (from ids in keys
                join s in _db.TblItemDetailStores
                    on new { ids.StoreId, ids.ItemDetailId }
                    equals new { StoreId = s.StoreId.Value, ItemDetailId = s.ItemDetailId.Value }
                select s).ToList();
        var result = new List<ItemDetailStoreDto>();

        foreach (var itemDetailStore in itemDetailStores)
        {
            var arivalLine = lines.FirstOrDefault(c =>
                c.Store.StoreId == itemDetailStore.StoreId &&
                c.ItemDetail.ItemDetailId == itemDetailStore.ItemDetailId);

            var line = arivalLine.Line;

            var dto = new ItemDetailStoreDto()
            {
                ItemDetailId = itemDetailStore.ItemDetailId,
                EntryDate = itemDetailStore.EntryDate.Value,
                ItemDetailStoreId = itemDetailStore.ItemDetailStoreId,
                CurrentStock = itemDetailStore.CurrentStock ?? 0,
                StoreId = itemDetailStore.StoreId,
                ItemId = arivalLine.Item.ItemId,
                DropDownText = string.Format(
                    "کالا: {0} | سازنده: {1} | تاریخ انقضا: {2} | سری ساخت: {3} | انبار: {4} | دوز بر ویال: {5} | مقدار موجود: {6}",
                    arivalLine.Item != null ? arivalLine.Item.FullName : "",
                    arivalLine.Manufacturer != null ? arivalLine.Manufacturer.FullName : "",
                    line.ExpiryDate != null ? line.ExpiryDate.Value.ToShamsi() : "",
                    line.BatchNum,
                    arivalLine.Store != null ? arivalLine.Store.Name : "",
                    arivalLine.DoseVial != null ? arivalLine.DoseVial.NumDosePerVial : "",
                    line.Quantity
                )
            };

            result.Add(dto);
        }

        return result;
    }
}