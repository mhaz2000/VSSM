using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Diagnostics;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VSSM.Application.Services.BaseEntities
{
    public class ReportService(VSSMDbContext _context) : IReportService
    {
        public async Task<(List<InventoryQuantityReportDto> data, int count)> GetInventoryQuantitiesAsync(long? itemClassId,
            long? itemId, long? sourceId, long? storeId, int pageNumber, int pageSize, string sort,
            string direction, int recipientId, CancellationToken cancellationToken)
        {

            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            // base query using joins

            var items = itemId is not null ? _context.TblItems.Where(item => item.ItemClassId == itemClassId && item.ItemId == itemId)
                : _context.TblItems.Where(item => item.ItemClassId == itemClassId);

            var stores = storeId is not null ? _context.TblStores.Where(t => t.RecipientId == recipientId && t.StoreId == storeId)
                : _context.TblStores.Where(t => t.RecipientId == recipientId);

            var query =
                from i in items
                join ic in _context.TblItemsClasses on i.ItemClassId equals ic.ItemClassId
                join id in _context.TblItemDetails on i.ItemId equals id.ItemId
                join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                join s in stores on ids.StoreId equals s.StoreId
                join m in _context.TblManufacturers on id.ManufacturerId equals m.ManufacturerId

                group new { ids, id, i, ic, s, m }
                by new
                {
                    id.ItemDetailId,
                    id.BatchNumber,
                    id.ExpiryDate,
                    ItemClassName = ic.FullName,
                    ItemName = i.FullName,
                    ManufacturerName = m.FullName,
                    StoreName = s.Name
                }
                into g

                select new InventoryQuantityReportDto
                {
                    ItemClass = g.Key.ItemClassName,
                    Item = g.Key.ItemName,
                    Manufacturer = g.Key.ManufacturerName,
                    BatchNumber = g.Key.BatchNumber,
                    ExpiryDate = g.Key.ExpiryDate.Value.ToShamsi(),
                    Store = g.Key.StoreName,
                    Quantity = g.Count()
                };

            query = query.AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<ItemForRecipientInYearDto> data, int count)> GetItemForRecipientInYear(int currentRecipientId, long? itemId, int? year, int? level, int? recipentId, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken)
        {
            var arrivals = _context.TblArrivals.Where(t => t.Date != null && t.Date.Value.Year == year);
            var reciepients = recipentId is null ? _context.TblRecipients : _context.TblRecipients.Where(t => t.RecipientId == recipentId);
            var dispatches = _context.TblDispatches.Where(t => t.SenderId == currentRecipientId);

            var query = from a in arrivals
                        join d in dispatches on a.DispatchId equals d.DispatchId
                        join r in reciepients on a.ReceiverId equals r.RecipientId
                        join l in _context.TblArrivalLines on a.ArrivalId equals l.ArrivalId
                        group new { a, d, l } by r into g
                        select new ItemForRecipientInYearDto()
                        {
                            Reciever = g.Key.FullName,
                            Jan = g.Where(t => t.a.Date.Value.Month == 1).Count(),
                            Feb = g.Where(t => t.a.Date.Value.Month == 2).Count(),
                            Mar = g.Where(t => t.a.Date.Value.Month == 3).Count(),
                            Apr = g.Where(t => t.a.Date.Value.Month == 4).Count(),
                            May = g.Where(t => t.a.Date.Value.Month == 5).Count(),
                            Jun = g.Where(t => t.a.Date.Value.Month == 6).Count(),
                            Jul = g.Where(t => t.a.Date.Value.Month == 7).Count(),
                            Agu = g.Where(t => t.a.Date.Value.Month == 8).Count(),
                            Sep = g.Where(t => t.a.Date.Value.Month == 9).Count(),
                            Oct = g.Where(t => t.a.Date.Value.Month == 10).Count(),
                            Nov = g.Where(t => t.a.Date.Value.Month == 11).Count(),
                            Dec = g.Where(t => t.a.Date.Value.Month == 12).Count(),
                            TotalQuota = 0,
                            YearlyNeed = 0
                        };


            var total = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            bool isLastPage = pageNumber == totalPages;


            query = query.ApplySorting(sort, direction == "desc");

            var pageData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (isLastPage)
            {
                var totalRow = new ItemForRecipientInYearDto
                {
                    Reciever = "مجموع",
                    Jan = await query.SumAsync(x => x.Jan, cancellationToken),
                    Feb = await query.SumAsync(x => x.Feb, cancellationToken),
                    Mar = await query.SumAsync(x => x.Mar, cancellationToken),
                    Apr = await query.SumAsync(x => x.Apr, cancellationToken),
                    May = await query.SumAsync(x => x.May, cancellationToken),
                    Jun = await query.SumAsync(x => x.Jun, cancellationToken),
                    Jul = await query.SumAsync(x => x.Jul, cancellationToken),
                    Agu = await query.SumAsync(x => x.Agu, cancellationToken),
                    Sep = await query.SumAsync(x => x.Sep, cancellationToken),
                    Oct = await query.SumAsync(x => x.Oct, cancellationToken),
                    Nov = await query.SumAsync(x => x.Nov, cancellationToken),
                    Dec = await query.SumAsync(x => x.Dec, cancellationToken),
                    TotalQuota = 0,
                    YearlyNeed = 0
                };

                pageData.Add(totalRow);
            }

            return (pageData, total);

        }

        public async Task<(List<ItemIndicatorDto> data, int count)> GetItemIndicator(int currentRecipientId, int? itemId, string batchNumber, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken)
        {
            var dispatchQuery = from d in _context.TblDispatches
                                join r in _context.TblRecipients on d.ReceiverId equals r.RecipientId
                                join dl in _context.TblDispatchLines on d.DispatchId equals dl.DispatchId
                                join id in _context.TblItemDetails.Where(t => t.BatchNumber == batchNumber && t.ItemId == itemId) on dl.ItemId equals id.ItemId
                                join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                                join s in _context.TblStores.Where(t => t.RecipientId == currentRecipientId) on ids.StoreId equals s.StoreId
                                select new ItemIndicatorDto()
                                {
                                    Arrival = 0,
                                    RequestNum = d.RequestNum,
                                    ReportNumber = d.ReportNumber,
                                    Date = d.Date,
                                    Recipient = r.FullName,
                                    Dispatch = dl.Quantity,
                                    CurrentStock = ids.CurrentStock,
                                    Description = "توزیع"
                                };

            var arrivalQuery = from a in _context.TblArrivals
                               join d in _context.TblDispatches on a.DispatchId equals d.DispatchId
                               join r in _context.TblRecipients on a.ReceiverId equals r.RecipientId
                               join al in _context.TblArrivalLines on a.ArrivalId equals al.ArrivalId
                               join id in _context.TblItemDetails.Where(t => t.BatchNumber == batchNumber && t.ItemId == itemId) on al.ItemId equals id.ItemId
                               join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                               join s in _context.TblStores.Where(t => t.RecipientId == currentRecipientId) on ids.StoreId equals s.StoreId
                               select new ItemIndicatorDto()
                               {
                                   RequestNum = d.RequestNum,
                                   ReportNumber = a.ReportNumber,
                                   Date = a.Date,
                                   Dispatch = 0,
                                   Recipient = r.FullName,
                                   Arrival = al.Quantity,
                                   CurrentStock = ids.CurrentStock,
                                   Description = "-"
                               };

            var query = arrivalQuery.Union(dispatchQuery);

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<ItemIndicatorDetailDto> GetItemIndicatorDetail(int currentRecipientId, int? itemId, string? batchNumber)
        {
            var query = from i in _context.TblItems.Where(t=> t.ItemId == itemId)
                        join id in _context.TblItemDetails.Where(t=> t.BatchNumber == batchNumber) on i.ItemId equals id.ItemId
                        join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                        join m in _context.TblManufacturers on id.ManufacturerId equals m.ManufacturerId
                        join v in _context.TblDosePerVials on id.DosePerVialId equals v.DosePerVialId
                        select new ItemIndicatorDetailDto()
                        {
                            BatchNumber = id.BatchNumber,
                            ItemName = i.FullName,
                            CurrentStock = ids.CurrentStock,
                            Manufacturer = m.FullName,
                            NumDosePerVial = v.NumDosePerVial,
                            ExpiryDate = id.ExpiryDate
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<(List<NotDispatchedToRecipientsDto> data, int count)> GetNotDispatchedToRecipientsAsync(long? itemId,
            long? fiscalPeriodId, string? fromDate, string? toDate, int pageNumber, int pageSize, string sort,
            string direction, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var dispatches = fiscalPeriodId is not null ? _context.TblDispatches.Where(t => t.FiscalPeriodId == fiscalPeriodId) : _context.TblDispatches;

            if (!string.IsNullOrWhiteSpace(fromDate))
                dispatches.Where(t => t.Date.HasValue && t.Date >= DateTimeHelper.ToGregorian(fromDate));
            if (!string.IsNullOrWhiteSpace(toDate))
                dispatches.Where(t => t.Date.HasValue && t.Date <= DateTimeHelper.ToGregorian(toDate));

            var receivers = from d in dispatches
                            join dl in _context.TblDispatchLines.Where(t => t.ItemId == itemId) on d.DispatchId equals dl.DispatchId
                            join r in _context.TblRecipients on d.ReceiverId equals r.RecipientId
                            select new NotDispatchedToRecipientsDto { RecipientId = r.RecipientId, FullName = r.FullName };

            var allRecipients = _context.TblRecipients.Where(t => t.IsActive.HasValue && t.IsActive.Value)
                .Select(t => new NotDispatchedToRecipientsDto { RecipientId = t.RecipientId, FullName = t.FullName });

            var query = allRecipients.Except(receivers).AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<OutOfStockItemsDto>, int count)> GetOutOfStockItems(int currentRecipientId, int? itemClassId, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken)
        {
            var nonZeroStockItems = from i in _context.TblItems.Where(t => t.IsActive != null && t.IsActive.Value && t.ItemClassId == itemClassId)
                                    join id in _context.TblItemDetails on i.ItemId equals id.ItemId
                                    join ids in _context.TblItemDetailStores.Where(t => t.CurrentStock != 0) on id.ItemDetailId equals ids.ItemDetailId
                                    join s in _context.TblStores.Where(t => t.RecipientId == currentRecipientId) on ids.StoreId equals s.StoreId
                                    select new { i.ItemId, i.FullName };

            var allItems = _context.TblItems.Where(t => t.IsActive.HasValue && t.IsActive.Value && t.ItemClassId == itemClassId)
                .Select(s => new { s.ItemId, s.FullName });

            var query = allItems.Except(nonZeroStockItems)
                .Select(s => new OutOfStockItemsDto() { ItemName = s.FullName }).AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }
    }
}
