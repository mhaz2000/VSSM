using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.BaseEntities.ItemDetailStores;
using VSSM.Application.DTOs.BaseEntities.StoreTransfers;
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
        var itemDetailStores = await _db.TblItemDetailStores.Where(c => c.StoreId == storeId && c.CurrentStock > 0)
            .ToListAsync();

        var result = new List<ItemDetailStoreDto>();
        foreach (var itemDetailsStore in itemDetailStores)
        {
            var itemDetail = await _db.TblItemDetails.FirstOrDefaultAsync(c => c.ItemDetailId == itemDetailsStore.ItemDetailId);
            var item = await _db.TblItems.FirstOrDefaultAsync(c => c.ItemId == itemDetail!.ItemId);
            
            if (item is null)
                continue;

            var dto = new ItemDetailStoreDto()
            {
                ItemId = item.ItemId,
                CurrentStock = itemDetailsStore.CurrentStock ?? 0,
                ItemDetailId = itemDetailsStore.ItemDetailId,
                StoreId = itemDetailsStore.StoreId,
                DropDownText = item.FullName,
                ItemDetailStoreId = itemDetailsStore.PKItemDetailStoreId,
                ItemFullName = item.FullName
            };

            result.Add(dto);
        }


        return result;
    }

    public async Task TransferItems(StoreTransfersDto dto)
    {
        foreach (var item in dto.Items)
        {
            var itemDetailStore =
                await _db.TblItemDetailStores.FirstOrDefaultAsync(c => c.PKItemDetailStoreId == item.ItemDetailStoreId);

            if (itemDetailStore is null)
                continue;

            if (itemDetailStore.CurrentStock == item.Quantity)
            {
                itemDetailStore.StoreId = dto.ToStoreId;
                _db.TblItemDetailStores.Update(itemDetailStore);
            }
            else
            {
                itemDetailStore.CurrentStock = itemDetailStore.CurrentStock - item.Quantity;

                var newItemDetailStore = new TblItemDetailStore()
                {
                    ItemDetailId = itemDetailStore.ItemDetailId,
                    EntryDate = DateTime.Now,
                    StoreId = dto.ToStoreId,
                    CurrentStock = item.Quantity,
                    FkVvmStage = itemDetailStore.FkVvmStage,
                };

                _db.TblItemDetailStores.Add(newItemDetailStore);
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<ItemDetailStoreDto> GetById(long id)
    {
        var itemDetailStore =
            await _db.TblItemDetailStores.FirstOrDefaultAsync(c =>
                c.PKItemDetailStoreId == id);

        return new ItemDetailStoreDto()
        {
            ItemDetailId = itemDetailStore.ItemDetailId,
            EntryDate = itemDetailStore.EntryDate.Value,
            ItemDetailStoreId = itemDetailStore.PKItemDetailStoreId,
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
                Manufacturer = _db.TblBaseInfo.FirstOrDefault(m => m.BaseInfoId == l.ManufacturerId),
                Store = _db.TblStores.FirstOrDefault(s => s.StoreId == l.StoreId),
                DoseVial = _db.TblBaseInfo.FirstOrDefault(d => d.BaseInfoId == l.DosePerVialId),
                ItemDetail = _db.TblItemDetails.FirstOrDefault(d => d.ItemDetailId == l.ItemDetailId)
            })
            .AsNoTracking()
            .ToListAsync();

        var keys = lines.
            Where(c=>c.Store != null && c.Line.ItemDetailId != null)
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
                ItemDetailStoreId = itemDetailStore.PKItemDetailStoreId,
                CurrentStock = itemDetailStore.CurrentStock ?? 0,
                StoreId = itemDetailStore.StoreId,
                ItemId = arivalLine.Item.ItemId,
                DropDownText = string.Format(
                    "کالا: {0} | سازنده: {1} | تاریخ انقضا: {2} | سری ساخت: {3} | انبار: {4} | دوز بر ویال: {5} | مقدار موجود: {6}",
                    arivalLine.Item != null ? arivalLine.Item.FullName : "",
                    arivalLine.Manufacturer != null ? arivalLine.Manufacturer.Title : "",
                    line.ExpiryDate != null ? line.ExpiryDate.Value.ToShamsi() : "",
                    line.BatchNum,
                    arivalLine.Store != null ? arivalLine.Store.Name : "",
                    arivalLine.DoseVial != null ? arivalLine.DoseVial.Title : "",
                    line.Quantity
                )
            };

            result.Add(dto);
        }

        return result;
    }
}