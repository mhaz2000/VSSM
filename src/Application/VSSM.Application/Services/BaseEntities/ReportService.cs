using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Entities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ReportService(VSSMDbContext _context) : IReportService
    {
        public async Task<(List<GroupedAvailableQuantityDetailDto> data, int count)> GetAvailableQuantityDetailReportAsync(AvailableQuantityReportQuery inputModel,
            int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var itemIds = (inputModel.ItemId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var storeIds = (inputModel.StoreId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();


            var itemQuery = itemIds.Any() ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId)) : _context.TblItems;

            var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber) ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber)) : _context.TblItemDetails;
            itemDetailQuery = inputModel.ManufacturerId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.ManufacturerId == inputModel.ManufacturerId);
            itemDetailQuery = inputModel.VvmtypeId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.VvmTypeId == inputModel.VvmtypeId);
            itemDetailQuery = inputModel.CommercialNameId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.CommercialNameId == inputModel.CommercialNameId);
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ProductinDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ProductionDate.HasValue &&
                t.ProductionDate == DateTimeHelper.ToGregorian(inputModel.ProductinDate));
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ExpiryDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ExpiryDate.HasValue &&
                t.ExpiryDate == DateTimeHelper.ToGregorian(inputModel.ExpiryDate));

            var itemDetailStoreQuery = storeIds.Any() ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value)) : _context.TblItemDetailStores;
            itemDetailStoreQuery = inputModel.VvmstageId is not null ? _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId) : itemDetailStoreQuery;

            var storeQuery = _context.TblStores.AsQueryable();
            if (inputModel.AllSubRecipientStocks)
            {
                var reciepients = _context.TblRecipients.Where(t => t.ParentId == currentRecipientId).Select(s => s.RecipientId).ToList();
                storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && reciepients.Contains(t.RecipientId.Value));
            }
            else if (inputModel.RecipientId is not null)
                storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
            else
                storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);

            var query =
                from i in itemQuery
                join id in itemDetailQuery
                    on i.ItemId equals id.ItemId
                join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0)
                    on id.ItemDetailId equals ids.ItemDetailId
                join s in storeQuery
                    on ids.StoreId equals s.Id
                join r in _context.TblRecipients
                    on s.RecipientId equals r.RecipientId

                join m in _context.TblBaseInfo
                    on id.ManufacturerId equals m.BaseInfoId into mJoin
                from m in mJoin.DefaultIfEmpty()

                join c in _context.TblBaseInfo
                    on id.CommercialNameId equals c.BaseInfoId into cJoin
                from c in cJoin.DefaultIfEmpty()

                join dpv in _context.TblBaseInfo
                    on id.DosePerVialId equals dpv.BaseInfoId into dpvJoin
                from dpv in dpvJoin.DefaultIfEmpty()

                join vt in _context.TblBaseInfo
                    on id.VvmTypeId equals vt.BaseInfoId into vtJoin
                from vt in vtJoin.DefaultIfEmpty()

                join vs in _context.TblBaseInfo
                    on ids.FkVvmStageId equals vs.BaseInfoId into vsJoin
                from vs in vsJoin.DefaultIfEmpty()

                group new { i, id, ids, s, m, c, dpv, vt, vs }
                by new
                {
                    id.ItemDetailId,
                    id.BatchNumber,
                    id.ExpiryDate,
                    id.ProductionDate,
                    Recipient = r.FullName,
                    ItemName = i.FullName,
                    ManufacturerName = m.Title,
                    StoreName = s.Name,
                    DoesPerVial = dpv.Title,
                    CommercialName = c.Title,
                    Vvmtype = vt.Title,
                    Vvmstage = vs.Title
                } into g
                select new AvailableQuantityDetailDto
                {
                    ItemName = g.Key.ItemName,
                    BatchNumber = g.Key.BatchNumber,
                    ManufacturerName = g.Key.ManufacturerName,
                    ExpiryDate = g.Key.ExpiryDate,
                    DoesPerVial = int.Parse(g.Key.DoesPerVial),
                    Stock = g.Key.StoreName,
                    Recipient = g.Key.Recipient,
                    ProductionDate = g.Key.ProductionDate,
                    CommercialName = g.Key.CommercialName,
                    Vvmstage = g.Key.Vvmstage,
                    Vvmtype = g.Key.Vvmtype,
                    CurrentStock = g.Sum(t => t.ids.CurrentStock)
                };

            query = query.AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            var list = fetchAll ? await query.ToListAsync() : await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var grouped = list
                .GroupBy(x => x.ItemName)
                .Select(g => new GroupedAvailableQuantityDetailDto()
                {
                    Meta = new AvailableQuantityDetailMetaDto()
                    {
                        ItemName = g.Key,
                        Count = g.Sum(t => t.CurrentStock) ?? 0
                    },
                    Details = g.Select(d => new AvailableQuantityDetailDto()
                    {
                        Stock = d.Stock,
                        Vvmtype = d.Vvmtype,
                        Vvmstage = d.Vvmstage,
                        BatchNumber = d.BatchNumber,
                        ManufacturerName = d.ManufacturerName,
                        ExpiryDate = d.ExpiryDate,
                        ProductionDate = d.ProductionDate,
                        CommercialName = d.CommercialName,
                        DoesPerVial = d.DoesPerVial,
                        CurrentStock = d.CurrentStock,
                        Recipient = d.Recipient
                    }).ToList()
                })
                .ToList();

            return (grouped, total);

        }

        public async Task<(List<AvailableQuantityQuantitySummaryDto> data, int count)> GetAvailableQuantityQuantitySummaryReportAsync(AvailableQuantityReportQuery inputModel,
            int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var itemIds = (inputModel.ItemId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var storeIds = (inputModel.StoreId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var itemQuery = itemIds.Any() ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId)) : _context.TblItems;

            var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber) ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber)) : _context.TblItemDetails;
            itemDetailQuery = inputModel.ManufacturerId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.ManufacturerId == inputModel.ManufacturerId);
            itemDetailQuery = inputModel.VvmtypeId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.VvmTypeId == inputModel.VvmtypeId);
            itemDetailQuery = inputModel.CommercialNameId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.CommercialNameId == inputModel.CommercialNameId);
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ProductinDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ProductionDate.HasValue &&
                t.ProductionDate == DateTimeHelper.ToGregorian(inputModel.ProductinDate));
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ExpiryDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ExpiryDate.HasValue &&
                t.ExpiryDate == DateTimeHelper.ToGregorian(inputModel.ExpiryDate));

            var itemDetailStoreQuery = storeIds.Any() ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value)) : _context.TblItemDetailStores;
            itemDetailStoreQuery = inputModel.VvmstageId is not null ? _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId) : itemDetailStoreQuery;

            var storeQuery = _context.TblStores.AsQueryable();
            if (inputModel.AllSubRecipientStocks)
            {
                var reciepients = _context.TblRecipients.Where(t => t.ParentId == currentRecipientId).Select(s => s.RecipientId).ToList();
                storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && reciepients.Contains(t.RecipientId.Value));
            }
            else if (inputModel.RecipientId is not null)
                storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
            else
                storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);

            var query =
                from i in itemQuery
                join ic in _context.TblItemsClasses
                    on i.ItemId equals ic.ItemClassId
                join id in itemDetailQuery
                    on i.ItemId equals id.ItemId
                join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0)
                    on id.ItemDetailId equals ids.ItemDetailId
                join s in storeQuery
                    on ids.StoreId equals s.Id
                join v in _context.TblBaseInfo
                    on id.DosePerVialId equals v.BaseInfoId
                group new { i, ids, v, ic }
                by new { ItemName = i.FullName, i.Code, DosePerVial = v.Title, ItemClass = ic.FullName, id.BatchNumber, id.ExpiryDate }
                    into g
                select new AvailableQuantityQuantitySummaryDto
                {
                    ItemName = g.Key.ItemName,
                    ItemCode = g.Key.Code,
                    ItemClass = g.Key.ItemClass,
                    BatchNumber = g.Key.BatchNumber,
                    ExpiryDate = g.Key.ExpiryDate,
                    DoesPerVial = int.Parse(g.Key.DosePerVial),
                    CurrentStock = g.Sum(s => s.ids.CurrentStock)
                };

            query = query.AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<AvailableQuantitySummaryDto> data, int count)> GetAvailableQuantitySummaryReportAsync(AvailableQuantityReportQuery inputModel,
            int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var itemIds = (inputModel.ItemId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var storeIds = (inputModel.StoreId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var itemQuery = itemIds.Any() ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId)) : _context.TblItems;

            var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber) ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber)) : _context.TblItemDetails;
            itemDetailQuery = inputModel.ManufacturerId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.ManufacturerId == inputModel.ManufacturerId);
            itemDetailQuery = inputModel.VvmtypeId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.VvmTypeId == inputModel.VvmtypeId);
            itemDetailQuery = inputModel.CommercialNameId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.CommercialNameId == inputModel.CommercialNameId);
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ProductinDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ProductionDate.HasValue &&
                t.ProductionDate == DateTimeHelper.ToGregorian(inputModel.ProductinDate));
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ExpiryDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ExpiryDate.HasValue &&
                t.ExpiryDate == DateTimeHelper.ToGregorian(inputModel.ExpiryDate));

            var itemDetailStoreQuery = storeIds.Any() ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value)) : _context.TblItemDetailStores;
            itemDetailStoreQuery = inputModel.VvmstageId is not null ? _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId) : itemDetailStoreQuery;

            var storeQuery = _context.TblStores.AsQueryable();
            if (inputModel.AllSubRecipientStocks)
            {
                var reciepients = _context.TblRecipients.Where(t => t.ParentId == currentRecipientId).Select(s => s.RecipientId).ToList();
                storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && reciepients.Contains(t.RecipientId.Value));
            }
            else if (inputModel.RecipientId is not null)
                storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
            else
                storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);

            var query =
                from i in itemQuery
                join id in itemDetailQuery
                    on i.ItemId equals id.ItemId
                join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0)
                    on id.ItemDetailId equals ids.ItemDetailId
                join s in storeQuery
                    on ids.StoreId equals s.Id
                group new { i, ids }
                by new { i.FullName, i.Code }
                    into g
                select new AvailableQuantitySummaryDto
                {
                    ItemName = g.Key.FullName,
                    ItemCode = g.Key.Code,
                    CurrentStock = g.Sum(s => s.ids.CurrentStock)
                };

            query = query.AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");


            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<AvailableQuantitySummaryWithDosePerVialDto> data, int count)> GetAvailableQuantitySummaryWithDosePerVialReportAsync(AvailableQuantityReportQuery inputModel,
            int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var itemIds = (inputModel.ItemId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var storeIds = (inputModel.StoreId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var itemQuery = itemIds.Any() ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId)) : _context.TblItems;

            var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber) ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber)) : _context.TblItemDetails;
            itemDetailQuery = inputModel.ManufacturerId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.ManufacturerId == inputModel.ManufacturerId);
            itemDetailQuery = inputModel.VvmtypeId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.VvmTypeId == inputModel.VvmtypeId);
            itemDetailQuery = inputModel.CommercialNameId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.CommercialNameId == inputModel.CommercialNameId);
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ProductinDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ProductionDate.HasValue &&
                t.ProductionDate == DateTimeHelper.ToGregorian(inputModel.ProductinDate));
            itemDetailQuery = string.IsNullOrEmpty(inputModel.ExpiryDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ExpiryDate.HasValue &&
                t.ExpiryDate == DateTimeHelper.ToGregorian(inputModel.ExpiryDate));

            var itemDetailStoreQuery = storeIds.Any() ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value)) : _context.TblItemDetailStores;
            itemDetailStoreQuery = inputModel.VvmstageId is not null ? _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId) : itemDetailStoreQuery;

            var storeQuery = _context.TblStores.AsQueryable();
            if (inputModel.AllSubRecipientStocks)
            {
                var reciepients = _context.TblRecipients.Where(t => t.ParentId == currentRecipientId).Select(s => s.RecipientId).ToList();
                storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && reciepients.Contains(t.RecipientId.Value));
            }
            else if (inputModel.RecipientId is not null)
                storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
            else
                storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);

            var query =
                from i in itemQuery
                join id in itemDetailQuery
                    on i.ItemId equals id.ItemId
                join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0)
                    on id.ItemDetailId equals ids.ItemDetailId
                join s in storeQuery
                    on ids.StoreId equals s.Id
                join v in _context.TblBaseInfo
                    on id.DosePerVialId equals v.BaseInfoId
                group new { i, ids, v }
                by new { i.FullName, i.Code, NumDosePerVial = v.Title }
                    into g
                select new AvailableQuantitySummaryWithDosePerVialDto
                {
                    ItemName = g.Key.FullName,
                    ItemCode = g.Key.Code,
                    DoesPerVial = int.Parse(g.Key.NumDosePerVial),
                    CurrentStock = g.Sum(s => s.ids.CurrentStock)
                };

            query = query.AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");


            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<InventoryHistoryDto> data, int count)> GetInventoryHistory(int currentRecipientId, long? itemId,
            int? fiscalPeriodId, int? level, int? recipientId, string? fromDate, string? toDate, int pageNumber, int pageSize, string sort,
            string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var itemQuery = _context.TblItems.Where(t => t.ItemId == itemId);

            var dispatches = fiscalPeriodId is not null ? _context.TblDispatches.Where(t => t.FiscalPeriodId == fiscalPeriodId) : _context.TblDispatches;
            var arrivals = fiscalPeriodId is not null ? _context.TblArrivals.Where(t => t.FiscalPeriodId == fiscalPeriodId) : _context.TblArrivals;
            var reciepients = recipientId is null ? _context.TblRecipients.Where(t => t.ParentId == currentRecipientId) : _context.TblRecipients.Where(t => t.RecipientId == recipientId);
            reciepients = level is not null ? reciepients.Where(t => t.ChainLevel == level) : reciepients;

            var query =
                from a in arrivals
                join al in _context.TblArrivalLines on a.Id equals al.ArrivalId
                join i in itemQuery on al.ItemId equals i.ItemId
                join r in reciepients on a.ReceiverId equals r.RecipientId
                join d in dispatches on a.DispatchId equals d.DispatchId into dispatchJoin
                from d in dispatchJoin.DefaultIfEmpty()
                join dl in _context.TblDispatchLines on d.DispatchId equals dl.DispatchId into dispatchLineJoin
                from dl in dispatchLineJoin.DefaultIfEmpty()
                select new
                {
                    ArrivalDate = a.Date,
                    ArrivalQuantity = al.Quantity,
                    DispatchDate = d.Date,
                    DispatchQuantity = dl.Quantity
                };

            if (!string.IsNullOrEmpty(fromDate))
            {
                if (!DateTime.TryParse(fromDate, out DateTime from))
                    from = DateTimeHelper.ToGregorian(fromDate);

                query = query.Where(x => x.ArrivalDate >= from);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                if (!DateTime.TryParse(fromDate, out DateTime to))
                    to = DateTimeHelper.ToGregorian(toDate);

                query = query.Where(x => x.ArrivalDate <= to);
            }

            var grouped = from q in query
                          group q by new
                          {
                              q.ArrivalDate.Value.Year,
                              q.ArrivalDate.Value.Month
                          }
                          into g
                          select new InventoryHistoryDto
                          {
                              Year = g.Key.Year,
                              Month = g.Key.Month,

                              InStock = g.Sum(x => x.ArrivalQuantity)
                                       - g.Sum(x => x.DispatchDate != null ? x.DispatchQuantity : 0) ?? 0
                          };

            grouped = grouped.OrderBy(t => t.Year).ThenBy(t => t.Month).AsNoTracking();

            var total = await grouped.CountAsync();

            grouped = grouped.ApplySorting(sort, direction == "desc");

            if (fetchAll)
                return (await grouped.ToListAsync(), 0);

            return (await grouped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<InventoryQuantityReportDto> data, int count)> GetInventoryQuantitiesAsync(long? itemClassId,
            long? itemId, long? sourceId, long? storeId, int pageNumber, int pageSize, string sort,
            string direction, int recipientId, bool fetchAll, CancellationToken cancellationToken)
        {

            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            // base query using joins

            var items = itemId is not null ? _context.TblItems.Where(item => item.FkClassId == itemClassId && item.ItemId == itemId)
                : _context.TblItems.Where(item => item.FkClassId == itemClassId);

            var stores = storeId is not null ? _context.TblStores.Where(t => t.RecipientId == recipientId && t.Id == storeId)
                : _context.TblStores.Where(t => t.RecipientId == recipientId);

            var query =
                from i in items
                join ic in _context.TblItemsClasses on i.FkClassId equals ic.ItemClassId
                join id in _context.TblItemDetails on i.ItemId equals id.ItemId
                join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                join s in stores on ids.StoreId equals s.Id
                join m in _context.TblBaseInfo on id.ManufacturerId equals m.BaseInfoId

                group new { ids, id, i, ic, s, m }
                by new
                {
                    id.ItemDetailId,
                    id.BatchNumber,
                    id.ExpiryDate,
                    ItemClassName = ic.FullName,
                    ItemName = i.FullName,
                    ManufacturerName = m.Title,
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

            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<ItemForRecipientInYearDto> data, int count)> GetItemForRecipientInYear(int currentRecipientId, long? itemId, int? year,
            int? level, int? recipentId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var arrivals = _context.TblArrivals.Where(t => t.Date != null && t.Date.Value.Year == year);
            var reciepients = recipentId is null ? _context.TblRecipients : _context.TblRecipients.Where(t => t.RecipientId == recipentId);
            var dispatches = _context.TblDispatches.Where(t => t.SenderId == currentRecipientId);

            var query = from a in arrivals
                        join d in dispatches on a.DispatchId equals d.DispatchId
                        join r in reciepients on a.ReceiverId equals r.RecipientId
                        join l in _context.TblArrivalLines on a.Id equals l.ArrivalId
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

            var pageData = fetchAll ? await query.ToListAsync() : await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (isLastPage || fetchAll)
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

        public async Task<(List<ItemIndicatorDto> data, int count)> GetItemIndicator(int currentRecipientId, int? itemId, string batchNumber, int pageNumber,
            int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var dispatchQuery = from d in _context.TblDispatches
                                join r in _context.TblRecipients on d.ReceiverId equals r.RecipientId
                                join dl in _context.TblDispatchLines on d.DispatchId equals dl.DispatchId
                                join id in _context.TblItemDetails.Where(t => t.BatchNumber == batchNumber && t.ItemId == itemId) on dl.ItemId equals id.ItemId
                                join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                                join s in _context.TblStores.Where(t => t.RecipientId == currentRecipientId) on ids.StoreId equals s.Id
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
                               join al in _context.TblArrivalLines on a.Id equals al.ArrivalId
                               join id in _context.TblItemDetails.Where(t => t.BatchNumber == batchNumber && t.ItemId == itemId) on al.ItemId equals id.ItemId
                               join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                               join s in _context.TblStores.Where(t => t.RecipientId == currentRecipientId) on ids.StoreId equals s.Id
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

            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<ItemIndicatorDetailDto> GetItemIndicatorDetail(int currentRecipientId, int? itemId, string? batchNumber)
        {
            var query = from i in _context.TblItems.Where(t => t.ItemId == itemId)
                        join id in _context.TblItemDetails.Where(t => t.BatchNumber == batchNumber) on i.ItemId equals id.ItemId
                        join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                        join m in _context.TblBaseInfo on id.ManufacturerId equals m.BaseInfoId
                        join v in _context.TblBaseInfo on id.DosePerVialId equals v.BaseInfoId
                        select new ItemIndicatorDetailDto()
                        {
                            BatchNumber = id.BatchNumber,
                            ItemName = i.FullName,
                            CurrentStock = ids.CurrentStock,
                            Manufacturer = m.Title,
                            NumDosePerVial = int.Parse(v.Title),
                            ExpiryDate = id.ExpiryDate
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<(List<NotDispatchedToRecipientsDto> data, int count)> GetNotDispatchedToRecipientsAsync(long? itemId,
            long? fiscalPeriodId, string? fromDate, string? toDate, int pageNumber, int pageSize, string sort,
            string direction, bool fetchAll, CancellationToken cancellationToken)
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

            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<OutOfStockItemsDto> data, int count)> GetOutOfStockItems(int currentRecipientId, int? itemClassId, int pageNumber,
            int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var nonZeroStockItems = from i in _context.TblItems.Where(t => t.IsActive != null && t.IsActive.Value && t.FkClassId == itemClassId)
                                    join id in _context.TblItemDetails on i.ItemId equals id.ItemId
                                    join ids in _context.TblItemDetailStores.Where(t => t.CurrentStock != 0) on id.ItemDetailId equals ids.ItemDetailId
                                    join s in _context.TblStores.Where(t => t.RecipientId == currentRecipientId) on ids.StoreId equals s.Id
                                    select new { i.ItemId, i.FullName };

            var allItems = _context.TblItems.Where(t => t.IsActive.HasValue && t.IsActive.Value && t.FkClassId == itemClassId)
                .Select(s => new { s.ItemId, s.FullName });

            var query = allItems.Except(nonZeroStockItems)
                .Select(s => new OutOfStockItemsDto() { ItemName = s.FullName }).AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<TotalLossReportDto> data, int count)> GetTotalLossAsync(long? itemId, DateTime fromDateTime, DateTime toDateTime, int pageNumber,
            int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            // base query using joins

            var query = _context.TblDestroyUids.Include(t => t.UidInfo).ThenInclude(t => t.OrderInfo).ThenInclude(t => t.Item)
                                                .Include(t => t.DestroyReason)
                                                .Include(t => t.DestroyedByUser)
                .Select(s => new TotalLossReportDto()
                {
                    Item = s.UidInfo.OrderInfo.Item.FullName,
                    DestroyReason = s.DestroyReason.Title,
                    Date = s.DestroyDate,
                    DestroyBy = s.DestroyedByUser.FirstName + " " + s.DestroyedByUser.LastName,
                });

            var shamsiToDate = toDateTime.ToShamsi();
            var shamsiFromDate = fromDateTime.ToShamsi();

            query = query.Where(t => t.Date.CompareTo(shamsiToDate) <= 0 && shamsiFromDate.CompareTo(t.Date) <= 0);

            query = query.AsNoTracking();

            query = query.Distinct();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            if (fetchAll)
                return (await query.ToListAsync(), 0);

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);
        }

        public async Task<(List<GroupedExpiredItemDto> data, int count)> GetExpiredItemAsync(int currentRecipientId,
            int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };


            var itemQuery = _context.TblItems;
            var itemDetailQuery = _context.TblItemDetails.Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value < DateTime.Now);
            var itemDetailStoreQuery = _context.TblItemDetailStores;

            var storeQuery = _context.TblStores.AsQueryable();
            storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);

            var query =
                from i in itemQuery
                join id in itemDetailQuery
                    on i.ItemId equals id.ItemId
                join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0)
                    on id.ItemDetailId equals ids.ItemDetailId
                join s in storeQuery
                    on ids.StoreId equals s.Id
                join r in _context.TblRecipients
                    on s.RecipientId equals r.RecipientId

                join m in _context.TblBaseInfo
                    on id.ManufacturerId equals m.BaseInfoId into mJoin
                from m in mJoin.DefaultIfEmpty()


                join dpv in _context.TblBaseInfo
                    on id.DosePerVialId equals dpv.BaseInfoId into dpvJoin
                from dpv in dpvJoin.DefaultIfEmpty()

                join vs in _context.TblBaseInfo
                    on ids.FkVvmStageId equals vs.BaseInfoId into vsJoin
                from vs in vsJoin.DefaultIfEmpty()

                group new { i, id, ids, s, m, dpv, vs }
                by new
                {
                    id.ItemDetailId,
                    id.BatchNumber,
                    id.ExpiryDate,
                    id.ProductionDate,
                    Recipient = r.FullName,
                    ItemName = i.FullName,
                    ManufacturerName = m.Title,
                    StoreName = s.Name,
                    DoesPerVial = dpv.Title,
                    Vvmstage = vs.Title
                } into g
                select new ExpiredItemDto
                {
                    ItemName = g.Key.ItemName,
                    BatchNumber = g.Key.BatchNumber,
                    ManufacturerName = g.Key.ManufacturerName,
                    ExpiryDate = g.Key.ExpiryDate,
                    DoesPerVial = int.Parse(g.Key.DoesPerVial),
                    Stock = g.Key.StoreName,
                    VialNumber = int.Parse(g.Key.DoesPerVial),
                    Vvmstage = g.Key.Vvmstage,
                    CurrentStock = g.Sum(t => t.ids.CurrentStock)
                };

            query = query.AsNoTracking();

            var total = await query.CountAsync();

            query = query.ApplySorting(sort, direction == "desc");

            var list = fetchAll ? await query.ToListAsync() : await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var grouped = list
                .GroupBy(x => x.ItemName)
                .Select(g => new GroupedExpiredItemDto()
                {
                    Meta = new ExpiredItemDtoDetailMetaDto()
                    {
                        ItemName = g.Key,
                        Count = g.Sum(t => t.CurrentStock) ?? 0
                    },
                    Details = g.Select(d => new ExpiredItemDto()
                    {
                        Stock = d.Stock,
                        Vvmstage = d.Vvmstage,
                        BatchNumber = d.BatchNumber,
                        ManufacturerName = d.ManufacturerName,
                        ExpiryDate = d.ExpiryDate,
                        DoesPerVial = d.DoesPerVial,
                        CurrentStock = d.CurrentStock,
                        VialNumber = d.VialNumber
                    }).ToList()
                })
                .ToList();

            return (grouped, total);

        }

    }
}
