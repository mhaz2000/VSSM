using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.Warnings;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class WarningService(VSSMDbContext _context) : IWarningService
{
    public async Task<(List<CloseToExpirationDto> Data, int Count)>GetCloseToExpirationAsync(
         string? search,
         int page,
         int pageSize,
         string sortField,
         string sortOrder,
         int recipientId,
         CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        var stockQuery =
            from stock in _context.TblItemDetailStores
            group stock by new
            {
                stock.ItemDetailId,
                stock.StoreId
            }
            into g
            select new
            {
                g.Key.ItemDetailId,
                g.Key.StoreId,
                Quantity = g.Sum(x => x.CurrentStock ?? 0)
            };

        var query =
            from line in _context.TblArrivalLines

            join stock in stockQuery
                on new
                {
                    line.ItemDetailId,
                    line.StoreId
                }
                equals new
                {
                    stock.ItemDetailId,
                    stock.StoreId
                }

            join itemDetail in _context.TblItemDetails
                on line.ItemDetailId equals itemDetail.ItemDetailId

            join item in _context.TblItems
                on itemDetail.ItemId equals item.ItemId

            join manufacturer in _context.TblBaseInfo
                on line.ManufacturerId equals manufacturer.BaseInfoId

            join vvmStage in _context.TblBaseInfo
                on line.VvmStageId equals vvmStage.BaseInfoId

            join store in _context.TblStores
                on line.StoreId equals store.Id

            where line.ExpiryDate != null
                  && today <= line.ExpiryDate.Value
                  && today >= line.ExpiryDate.Value
                      .AddDays(-(item.DayToWarmExpire ?? 0))

            select new CloseToExpirationDto
            {
                ItemName = item.FullName,
                BatchNumber = line.BatchNum,
                Manufacturer = manufacturer.Title,
                VVMStage = vvmStage.Title,
                StockName = store.Name,
                ExpiredDate = line.ExpiryDate,
                Quantity = stock.Quantity
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.ItemName.Contains(search));
        }

        query = query.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(
            sortField,
            sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase));

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (data, total);
    }

    public async Task<(List<HighItemsMaxDto> data, int count)> GetHighItemsMaxAsync(string? search, int page, int pageSize, string sortField,
        string sortOrder, int recipientId, CancellationToken cancellationToken)
    {
        var query =
            from itm in _context.TblItems
            join itmDtl in _context.TblItemDetails
                on itm.ItemId equals itmDtl.ItemId
            join itmDtlStr in _context.TblItemDetailStores
                on itmDtl.ItemDetailId equals itmDtlStr.ItemDetailId
            join str in _context.TblStores.Where(s => s.RecipientId == recipientId)
                on itmDtlStr.StoreId equals str.Id
            join mmitm in _context.TblMinMaxItems.Where(m => m.RecipientId == recipientId)
                on itm.ItemId equals mmitm.ItemId
            group new { itm, itmDtlStr, mmitm } by new
            {
                itm.ItemId,
                itm.FullName,
                mmitm.Max,
                mmitm.Min,
                mmitm.OrderLevel
            }
            into g
            select new HighItemsMaxDto
            {
                ItemName = g.Key.FullName,
                MaxQuantity = g.Key.Max ?? 0,
                MinQuantity = g.Key.Min ?? 0,
                OrderLevel = g.Key.OrderLevel ?? 0,
                Quantity = g.Sum(x => x.itmDtlStr.CurrentStock ?? 0)
            };

        query = query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.ItemName.Contains(search));
        }

        var total = await query.CountAsync();

        query = query.ApplySorting(sortField, sortOrder == "desc");

        return (await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(), total);

    }

    public async Task<(List<LowItemsMinDto> data, int count)> GetLowItemsMinAsync(string? search, int page, int pageSize, string sortField, string sortOrder, int recipientId, CancellationToken cancellationToken)
    {
        var query =
            from itm in _context.TblItems
            join itmDtl in _context.TblItemDetails
                on itm.ItemId equals itmDtl.ItemId
            join itmDtlStr in _context.TblItemDetailStores
                on itmDtl.ItemDetailId equals itmDtlStr.ItemDetailId
            join str in _context.TblStores.Where(s => s.RecipientId == recipientId)
                on itmDtlStr.StoreId equals str.Id
            join mmitm in _context.TblMinMaxItems.Where(m => m.RecipientId == recipientId)
                on itm.ItemId equals mmitm.ItemId
            group new { itm, itmDtlStr, mmitm } by new
            {
                itm.ItemId,
                itm.FullName,
                mmitm.Max,
                mmitm.Min,
                mmitm.OrderLevel
            }
            into g
            select new LowItemsMinDto
            {
                ItemName = g.Key.FullName,
                MaxQuantity = g.Key.Max ?? 0,
                MinQuantity = g.Key.Min ?? 0,
                OrderLevel = g.Key.OrderLevel ?? 0,
                Quantity = g.Sum(x => x.itmDtlStr.CurrentStock ?? 0)
            };

        query = query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.ItemName.Contains(search));
        }

        var total = await query.CountAsync();

        query = query.ApplySorting(sortField, sortOrder == "desc");

        return (await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(), total);
    }
}
