using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ReportService(VSSMDbContext _context) : IReportService
{
    #region Entry/Exit Reports

    private static List<int> ParseIds(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new List<int>();

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }

    public async Task<(List<GroupedArrivalByItemDto> data, int count)> GetArrivalByItemReportAsync(
    EntryExitReportQuery input,
    int currentRecipientId,
    int pageNumber,
    int pageSize,
    string sort,
    string direction,
    bool fetchAll,
    CancellationToken cancellationToken)
    {
        var itemIds = ParseIds(input.ItemId);
        var storeIds = ParseIds(input.StoreId);

        var arrivals = _context.TblArrivals.AsQueryable();

        if (input.FiscalPeriodId is not null)
            arrivals = arrivals.Where(a => a.FiscalPeriodId == input.FiscalPeriodId);

        if (!string.IsNullOrWhiteSpace(input.ReportNumber))
            arrivals = arrivals.Where(a => a.ReportNumber.Contains(input.ReportNumber));

        if (!string.IsNullOrEmpty(input.FromArrivalDate))
        {
            var from = DateTimeHelper.ToGregorian(input.FromArrivalDate);
            arrivals = arrivals.Where(a => a.Date >= from);
        }

        if (!string.IsNullOrEmpty(input.ToArrivalDate))
        {
            var to = DateTimeHelper.ToGregorian(input.ToArrivalDate).AddDays(1).AddTicks(-1);
            arrivals = arrivals.Where(a => a.Date <= to);
        }

        var stores = _context.TblStores.AsQueryable();
        if (storeIds.Count > 0)
            stores = stores.Where(s => storeIds.Contains(s.Id));
        else
            stores = stores.Where(s => s.RecipientId == currentRecipientId);

        var items = _context.TblItems.AsQueryable();
        if (itemIds.Count > 0)
            items = items.Where(i => itemIds.Contains(i.ItemId));
        if (input.ItemClassId is not null)
            items = items.Where(i => i.FkClassId == input.ItemClassId);

        var rawQuery =
            from a in arrivals
            join al in _context.TblArrivalLines on a.Id equals al.ArrivalId
            join i in items on al.ItemId equals i.ItemId
            join s in stores on al.StoreId equals s.Id
            join ic in _context.TblBaseInfo on i.FkClassId equals ic.BaseInfoId into icj
            from ic in icj.DefaultIfEmpty()
            join m in _context.TblBaseInfo on al.ManufacturerId equals m.BaseInfoId into mj
            from m in mj.DefaultIfEmpty()
            join dpv in _context.TblBaseInfo on al.DosePerVialId equals dpv.BaseInfoId into dj
            from dpv in dj.DefaultIfEmpty()
            join cn in _context.TblBaseInfo on al.CommercialNameId equals cn.BaseInfoId into cnj
            from cn in cnj.DefaultIfEmpty()
            join vt in _context.TblBaseInfo on al.VvmTypeId equals vt.BaseInfoId into vtj
            from vt in vtj.DefaultIfEmpty()
            join vs in _context.TblBaseInfo on al.VvmStageId equals vs.BaseInfoId into vsj
            from vs in vsj.DefaultIfEmpty()
            join u in _context.TblBaseInfo on al.UseId equals u.BaseInfoId into uj
            from u in uj.DefaultIfEmpty()
            join ent in _context.TblUsers on a.EnteredById equals ent.UserId into entj
            from ent in entj.DefaultIfEmpty()
            join rcp in _context.TblRecipients on s.RecipientId equals rcp.RecipientId into rcpj
            from rcp in rcpj.DefaultIfEmpty()
            join o in _context.TblOrderInfo on a.Id equals o.FkArrivalId into oj
            from o in oj.DefaultIfEmpty()
            where ic.ParentId == 53
                  && (input.VvmstageId == null || al.VvmStageId == input.VvmstageId)
                  && (input.VvmtypeId == null || al.VvmTypeId == input.VvmtypeId)
                  && (input.ManufacturerId == null || al.ManufacturerId == input.ManufacturerId)
                  && (input.CommercialNameId == null || al.CommercialNameId == input.CommercialNameId)
                  && (input.SourceId == null || al.UseId == input.SourceId)
                  && (string.IsNullOrEmpty(input.BatchNumber) || al.BatchNum.Contains(input.BatchNumber))
            select new
            {
                ItemName = i.FullName ?? string.Empty,
                ItemClass = ic != null ? ic.Title : string.Empty,

                ArrivalDate = a.Date,
                ArrivalEntryDate = a.EntryDate,
                ArrivalReportNumber = a.ReportNumber,

                LineQuantity = al.Quantity,
                LineNumVials = al.NumVials,
                LineBatchNum = al.BatchNum,
                LineExpiryDate = al.ExpiryDate,
                LineProductionDate = al.ProductionDate,
                LineNote = al.Note,

                Manufacturer = m != null ? m.Title : null,
                DosePerVial = dpv != null ? dpv.Title : null,
                CommercialName = cn != null ? cn.Title : null,
                VvmType = vt != null ? vt.Title : null,
                VvmStage = vs != null ? vs.Title : null,
                Use = u != null ? u.Title : null,

                StoreName = s.Name,
                RecipientName = rcp != null ? rcp.FullName : null,

                EnteredByFirst = ent != null ? ent.FirstName : null,
                EnteredByLast = ent != null ? ent.LastName : null,

                OrderNumber = o != null ? (int?)o.NumberOrder : null
            };

        rawQuery = rawQuery.AsNoTracking();

        var total = await rawQuery
            .Select(x => x.ItemName)
            .Distinct()
            .CountAsync(cancellationToken);

        List<string> selectedItemNames;
        if (fetchAll)
        {
            selectedItemNames = await rawQuery
                .Select(x => x.ItemName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else
        {
            selectedItemNames = await rawQuery
                .Select(x => x.ItemName)
                .Distinct()
                .OrderBy(n => n)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        if (selectedItemNames.Count == 0)
            return (new List<GroupedArrivalByItemDto>(), total);

        var rawList = await rawQuery
            .Where(x => selectedItemNames.Contains(x.ItemName))
            .ToListAsync(cancellationToken);

        var grouped = rawList
            .GroupBy(x => new { x.ItemName, x.ItemClass })
            .Select(g => new GroupedArrivalByItemDto
            {
                Meta = new ArrivalByItemMetaDto
                {
                    ItemName = g.Key.ItemName,
                    ItemClass = g.Key.ItemClass,
                    TotalQuantity = g.Sum(x => x.LineQuantity ?? 0)
                },
                Details = g.Select(x => new ArrivalByItemDetailDto
                {
                    ReceiveDate = x.ArrivalEntryDate,
                    ArrivalDate = x.ArrivalDate,
                    EnteredBy = ((x.EnteredByFirst ?? string.Empty) + " " +
                                 (x.EnteredByLast ?? string.Empty)).Trim(),
                    ReportNumber = x.ArrivalReportNumber,
                    Source = x.Use,
                    OrderNumber = x.OrderNumber ?? 0,
                    Quantity = x.LineQuantity ?? 0,
                    NumVials = x.LineNumVials ?? 0,
                    DosePerVial = x.DosePerVial,
                    BatchNumber = x.LineBatchNum,
                    Manufacturer = x.Manufacturer,
                    Use = x.Use,
                    ExpiryDate = x.LineExpiryDate,
                    ProductionDate = x.LineProductionDate,
                    Store = x.StoreName,
                    CommercialName = x.CommercialName,
                    VvmType = x.VvmType,
                    VvmStage = x.VvmStage,
                    Note = x.LineNote
                }).OrderBy(d => d.BatchNumber).ToList()
            })
            .OrderBy(g => g.Meta.ItemName)
            .ToList();

        return (grouped, total);
    }

    public async Task<(List<DispatchReportDto> data, int count)> GetDispatchByRecipientReportAsync(
        EntryExitReportQuery input,
        int currentRecipientId,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool fetchAll,
        CancellationToken cancellationToken)
    {
        var itemIds = ParseIds(input.ItemId);
        var storeIds = ParseIds(input.StoreId);

        var dispatches = _context.TblDispatches.AsQueryable();

        if (input.FiscalPeriodId is not null)
            dispatches = dispatches.Where(d => d.FiscalPeriodId == input.FiscalPeriodId);

        if (!string.IsNullOrEmpty(input.FromDispatchDate))
        {
            var from = DateTimeHelper.ToGregorian(input.FromDispatchDate);
            dispatches = dispatches.Where(d => d.Date >= from);
        }

        if (!string.IsNullOrEmpty(input.ToDispatchDate))
        {
            var to = DateTimeHelper.ToGregorian(input.ToDispatchDate).AddDays(1).AddTicks(-1);
            dispatches = dispatches.Where(d => d.Date <= to);
        }

        if (!string.IsNullOrWhiteSpace(input.VoucherNumber))
            dispatches = dispatches.Where(d => d.ReportNumber.Contains(input.VoucherNumber));

        var stores = _context.TblStores.AsQueryable();
        if (storeIds.Count > 0)
            stores = stores.Where(s => storeIds.Contains(s.Id));
        else
            stores = stores.Where(s => s.RecipientId == currentRecipientId);

        var items = _context.TblItems.AsQueryable();
        if (itemIds.Count > 0)
            items = items.Where(i => itemIds.Contains(i.ItemId));
        if (input.ItemClassId is not null)
            items = items.Where(i => i.FkClassId == input.ItemClassId);

        var query =
            from d in dispatches
            join dl in _context.TblDispatchLines on d.DispatchId equals dl.DispatchId
            join i in items on dl.ItemId equals i.ItemId
            join s in stores on dl.StoreId equals s.Id
            join ic in _context.TblBaseInfo on i.FkClassId equals ic.BaseInfoId
            join recv in _context.TblRecipients on d.ReceiverId equals recv.RecipientId
            join dpv in _context.TblBaseInfo on dl.DosePerVialId equals dpv.BaseInfoId into dj
            from dpv in dj.DefaultIfEmpty()
            join m in _context.TblBaseInfo on dl.ManufacturerId equals m.BaseInfoId into mj
            from m in mj.DefaultIfEmpty()
            where ic.ParentId == 53
                  && (input.RecipientId == null || d.ReceiverId == input.RecipientId)
                  && (input.ManufacturerId == null || dl.ManufacturerId == input.ManufacturerId)
                  && (input.VvmstageId == null || dl.FkVvmStageBaseInfo == input.VvmstageId)
                  && (string.IsNullOrEmpty(input.BatchNumber) ||
                      dl.BatchNumber.Contains(input.BatchNumber))
            select new DispatchReportDto
            {
                ItemClass = ic.Title,
                Item = i.FullName,
                Code = i.Code,
                Recipient = recv.FullName,
                DispatchDate = d.Date.HasValue ? d.Date.Value.ToShamsi() : "-",
                VoucherNumber = d.ReportNumber ?? "-",
                RequestNumber = d.RequestNum ?? "-",
                BatchNumber = dl.BatchNumber ?? "-",
                Quantity = dl.Quantity ?? 0,
                DosePerVial = dpv != null ? dpv.Title : "-",
                NumVials = dl.NumVial ?? 0,
                ExpiryDate = dl.ExpiryDate.HasValue ? dl.ExpiryDate.Value.ToShamsi() : "-",
                Store = s.Name
            };

        query = query.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(
            string.IsNullOrWhiteSpace(sort) ? "DispatchDate" : sort,
            direction == "desc");

        if (fetchAll)
            return (await query.ToListAsync(cancellationToken), total);

        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (list, total);
    }



    #endregion
    public async Task<(List<ArrivalSummaryDto> data, int count)> GetArrivalSummaryReportAsync(
    EntryExitReportQuery input,
    int currentRecipientId,
    int pageNumber,
    int pageSize,
    string sort,
    string direction,
    bool fetchAll,
    CancellationToken cancellationToken)
    {
        var itemIds = ParseIds(input.ItemId);
        var storeIds = ParseIds(input.StoreId);

        var arrivals = _context.TblArrivals.AsQueryable();

        if (input.FiscalPeriodId is not null)
            arrivals = arrivals.Where(a => a.FiscalPeriodId == input.FiscalPeriodId);

        if (!string.IsNullOrWhiteSpace(input.ReportNumber))
            arrivals = arrivals.Where(a => a.ReportNumber.Contains(input.ReportNumber));

        if (!string.IsNullOrEmpty(input.FromArrivalDate))
        {
            var from = DateTimeHelper.ToGregorian(input.FromArrivalDate);
            arrivals = arrivals.Where(a => a.Date >= from);
        }

        if (!string.IsNullOrEmpty(input.ToArrivalDate))
        {
            var to = DateTimeHelper.ToGregorian(input.ToArrivalDate).AddDays(1).AddTicks(-1);
            arrivals = arrivals.Where(a => a.Date <= to);
        }

        var stores = _context.TblStores.AsQueryable();
        if (storeIds.Count > 0)
            stores = stores.Where(s => storeIds.Contains(s.Id));
        else
            stores = stores.Where(s => s.RecipientId == currentRecipientId);

        var items = _context.TblItems.AsQueryable();
        if (itemIds.Count > 0)
            items = items.Where(i => itemIds.Contains(i.ItemId));
        if (input.ItemClassId is not null)
            items = items.Where(i => i.FkClassId == input.ItemClassId);

        var query =
            from a in arrivals
            join al in _context.TblArrivalLines on a.Id equals al.ArrivalId
            join i in items on al.ItemId equals i.ItemId
            join s in stores on al.StoreId equals s.Id
            join ic in _context.TblBaseInfo on i.FkClassId equals ic.BaseInfoId into icj
            from ic in icj.DefaultIfEmpty()
            join m in _context.TblBaseInfo on al.ManufacturerId equals m.BaseInfoId into mj
            from m in mj.DefaultIfEmpty()
            join dpv in _context.TblBaseInfo on al.DosePerVialId equals dpv.BaseInfoId into dj
            from dpv in dj.DefaultIfEmpty()
            join u in _context.TblBaseInfo on al.UseId equals u.BaseInfoId into uj
            from u in uj.DefaultIfEmpty()
            where ic.ParentId == 53
                  && (input.VvmstageId == null || al.VvmStageId == input.VvmstageId)
                  && (input.VvmtypeId == null || al.VvmTypeId == input.VvmtypeId)
                  && (input.ManufacturerId == null || al.ManufacturerId == input.ManufacturerId)
                  && (input.CommercialNameId == null || al.CommercialNameId == input.CommercialNameId)
                  && (input.SourceId == null || al.UseId == input.SourceId)
                  && (string.IsNullOrEmpty(input.BatchNumber) || al.BatchNum.Contains(input.BatchNumber))
            select new
            {
                ItemClass = ic != null ? ic.Title : string.Empty,
                Item = i.FullName ?? string.Empty,
                ItemCode = i.Code ?? string.Empty,
                Manufacturer = m != null ? m.Title : null,
                Source = u != null ? u.Title : null,

                ArrivalDate = a.Date,
                ReportNumber = a.ReportNumber,

                Quantity = al.Quantity,
                BatchNumber = al.BatchNum,
                DosePerVial = dpv != null ? dpv.Title : null,
                NumVials = al.NumVials,
                ExpiryDate = al.ExpiryDate
            };

        query = query.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(
            string.IsNullOrWhiteSpace(sort) ? "ArrivalDate" : sort,
            direction == "desc");

        List<ArrivalSummaryDto> list;

        if (fetchAll)
        {
            var all = await query.ToListAsync(cancellationToken);
            list = all.Select(MapToSummaryDto).ToList();
        }
        else
        {
            var rawList = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            list = rawList.Select(MapToSummaryDto).ToList();
        }

        return (list, total);
    }

    private static ArrivalSummaryDto MapToSummaryDto(dynamic x)
    {
        return new ArrivalSummaryDto
        {
            ItemClass = x.ItemClass,
            Item = x.Item,
            ItemCode = x.ItemCode,
            Manufacturer = x.Manufacturer,
            Source = x.Source,
            Activity = null,
            ArrivalDate = x.ArrivalDate,
            ReportNumber = x.ReportNumber,
            OrderNumber = 0,
            Quantity = x.Quantity ?? 0,
            BatchNumber = x.BatchNumber,
            DosePerVial = x.DosePerVial,
            NumVials = x.NumVials ?? 0,
            UnitPrice = 0m,
            TotalPrice = 0m,
            ExpiryDate = x.ExpiryDate
        };
    }

    public async Task<(List<AvailableQuantityQuantitySummaryDto> data, int count)>
    GetAvailableQuantityQuantitySummaryReportAsync(
        AvailableQuantityReportQuery inputModel,
        int currentRecipientId,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool fetchAll,
        CancellationToken cancellationToken)
    {
        var itemIds = (inputModel.ItemId ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var storeIds = (inputModel.StoreId ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var itemQuery = itemIds.Any()
            ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId))
            : _context.TblItems;

        var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber)
            ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber))
            : _context.TblItemDetails;

        if (inputModel.ManufacturerId is not null)
            itemDetailQuery = itemDetailQuery.Where(t => t.ManufacturerId == inputModel.ManufacturerId);

        if (inputModel.VvmtypeId is not null)
            itemDetailQuery = itemDetailQuery.Where(t => t.VvmTypeId == inputModel.VvmtypeId);

        if (inputModel.CommercialNameId is not null)
            itemDetailQuery = itemDetailQuery.Where(t => t.CommercialNameId == inputModel.CommercialNameId);

        if (!string.IsNullOrEmpty(inputModel.ProductinDate))
        {
            var pd = DateTimeHelper.ToGregorian(inputModel.ProductinDate);
            itemDetailQuery = itemDetailQuery.Where(t => t.ProductionDate.HasValue && t.ProductionDate == pd);
        }

        if (!string.IsNullOrEmpty(inputModel.ExpiryDate))
        {
            var ed = DateTimeHelper.ToGregorian(inputModel.ExpiryDate);
            itemDetailQuery = itemDetailQuery.Where(t => t.ExpiryDate.HasValue && t.ExpiryDate == ed);
        }

        var itemDetailStoreQuery = storeIds.Any()
            ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value))
            : _context.TblItemDetailStores;

        if (inputModel.VvmstageId is not null)
            itemDetailStoreQuery = _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId);

        var storeQuery = _context.TblStores.AsQueryable();
        if (inputModel.AllSubRecipientStocks)
        {
            var recipients = _context.TblRecipients
                .Where(t => t.ParentId == currentRecipientId)
                .Select(s => s.RecipientId)
                .ToList();
            storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && recipients.Contains(t.RecipientId.Value));
        }
        else if (inputModel.RecipientId is not null && inputModel.RecipientId != 0)
        {
            storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
        }
        else
        {
            storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);
        }

        var parentQuery =
            from i in itemQuery
            join ic in _context.TblBaseInfo on i.FkClassId equals ic.BaseInfoId
            join id in itemDetailQuery on i.ItemId equals id.ItemId
            join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0) on id.ItemDetailId equals ids.ItemDetailId
            join s in storeQuery on ids.StoreId equals s.Id
            where ic.ParentId == 53
            select new
            {
                ItemName = i.FullName,
                ItemClass = ic.Title
            };


        var total = await parentQuery
            .Distinct()
            .CountAsync(cancellationToken);

        List<(string ItemName, string ItemClass)> selectedGroups;

        if (fetchAll)
        {
            var allGroups = await parentQuery
                .Distinct()
                .ToListAsync(cancellationToken);

            selectedGroups = allGroups
                .Select(x => (x.ItemName, x.ItemClass))
                .ToList();
        }
        else
        {
            var pagedGroups = await parentQuery
                .Distinct()
                .OrderBy(x => x.ItemName)
                .ThenBy(x => x.ItemClass)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            selectedGroups = pagedGroups
                .Select(x => (x.ItemName, x.ItemClass))
                .ToList();
        }

        if (selectedGroups.Count == 0)
            return (new List<AvailableQuantityQuantitySummaryDto>(), total);

        var selectedItemNames = selectedGroups
            .Select(g => g.ItemName)
            .Distinct()
            .ToList();


        var detailQuery =
            from i in itemQuery.Where(t => selectedItemNames.Contains(t.FullName))
            join ic in _context.TblBaseInfo on i.FkClassId equals ic.BaseInfoId
            join id in itemDetailQuery on i.ItemId equals id.ItemId
            join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0) on id.ItemDetailId equals ids.ItemDetailId
            join s in storeQuery on ids.StoreId equals s.Id
            join v in _context.TblBaseInfo on id.DosePerVialId equals v.BaseInfoId
            where ic.ParentId == 53
            group new { i, ids, v, ic }
            by new
            {
                ItemName = i.FullName,
                ItemClass = ic.Title,
                i.Code,
                DosePerVial = v.Title,
                id.BatchNumber,
                id.ExpiryDate
            }
            into g
            select new AvailableQuantityQuantitySummaryDto
            {
                ItemName = g.Key.ItemName,
                ItemCode = g.Key.Code,
                ItemClass = g.Key.ItemClass,
                BatchNumber = g.Key.BatchNumber,
                ExpiryDate = g.Key.ExpiryDate,
                DoesPerVial = g.Key.DosePerVial,
                CurrentStock = g.Sum(s => s.ids.CurrentStock)
            };

        detailQuery = detailQuery
            .AsNoTracking()
            .ApplySorting(sort, direction == "desc");

        var list = await detailQuery.ToListAsync(cancellationToken);
        return (list, total);
    }
    //public async Task<(List<AvailableQuantityQuantitySummaryDto> data, int count)> GetAvailableQuantityQuantitySummaryReportAsync(AvailableQuantityReportQuery inputModel,
    //    int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken)
    //{
    //    var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

    //    var itemIds = (inputModel.ItemId ?? "")
    //        .Split(',', StringSplitOptions.RemoveEmptyEntries)
    //        .Select(int.Parse)
    //        .ToList();

    //    var storeIds = (inputModel.StoreId ?? "")
    //        .Split(',', StringSplitOptions.RemoveEmptyEntries)
    //        .Select(int.Parse)
    //        .ToList();

    //    var itemQuery = itemIds.Any() ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId)) : _context.TblItems;

    //    var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber) ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber)) : _context.TblItemDetails;
    //    itemDetailQuery = inputModel.ManufacturerId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.ManufacturerId == inputModel.ManufacturerId);
    //    itemDetailQuery = inputModel.VvmtypeId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.VvmTypeId == inputModel.VvmtypeId);
    //    itemDetailQuery = inputModel.CommercialNameId is null ? itemDetailQuery : _context.TblItemDetails.Where(t => t.CommercialNameId == inputModel.CommercialNameId);
    //    itemDetailQuery = string.IsNullOrEmpty(inputModel.ProductinDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ProductionDate.HasValue &&
    //        t.ProductionDate == DateTimeHelper.ToGregorian(inputModel.ProductinDate));
    //    itemDetailQuery = string.IsNullOrEmpty(inputModel.ExpiryDate) ? itemDetailQuery : itemDetailQuery.Where(t => t.ExpiryDate.HasValue &&
    //        t.ExpiryDate == DateTimeHelper.ToGregorian(inputModel.ExpiryDate));

    //    var itemDetailStoreQuery = storeIds.Any() ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value)) : _context.TblItemDetailStores;
    //    itemDetailStoreQuery = inputModel.VvmstageId is not null ? _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId) : itemDetailStoreQuery;

    //    var storeQuery = _context.TblStores.AsQueryable();
    //    if (inputModel.AllSubRecipientStocks)
    //    {
    //        var reciepients = _context.TblRecipients.Where(t => t.ParentId == currentRecipientId).Select(s => s.RecipientId).ToList();
    //        storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && reciepients.Contains(t.RecipientId.Value));
    //    }
    //    else if (inputModel.RecipientId is not null)
    //        storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
    //    else
    //        storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);

    //    var query =
    //       from i in itemQuery
    //       join ic in _context.TblBaseInfo
    //           on i.FkClassId equals ic.BaseInfoId
    //       join id in itemDetailQuery
    //           on i.ItemId equals id.ItemId
    //       join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0)
    //           on id.ItemDetailId equals ids.ItemDetailId
    //       join s in storeQuery
    //           on ids.StoreId equals s.Id
    //       join v in _context.TblBaseInfo
    //           on id.DosePerVialId equals v.BaseInfoId
    //       where ic.ParentId == 53
    //       group new { i, ids, v, ic }
    //       by new
    //       {
    //           ItemName = i.FullName,
    //           i.Code,
    //           DosePerVial = v.Title,
    //           ItemClass = ic.Title,
    //           id.BatchNumber,
    //           id.ExpiryDate
    //       }
    //       into g
    //       select new AvailableQuantityQuantitySummaryDto
    //       {
    //           ItemName = g.Key.ItemName,
    //           ItemCode = g.Key.Code,
    //           ItemClass = g.Key.ItemClass,
    //           BatchNumber = g.Key.BatchNumber,
    //           ExpiryDate = g.Key.ExpiryDate,
    //           DoesPerVial = g.Key.DosePerVial,
    //           CurrentStock = g.Sum(s => s.ids.CurrentStock)
    //       };

    //    query = query.AsNoTracking();

    //    var total = await query.CountAsync();

    //    query = query.ApplySorting(sort, direction == "desc");

    //    if (fetchAll)
    //        return (await query.ToListAsync(), 0);

    //    return (await query
    //        .Skip((pageNumber - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync(), total);
    //}

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
                DoesPerVial = g.Key.NumDosePerVial,
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
            join ic in _context.TblBaseInfo
                on i.FkClassId equals ic.BaseInfoId
            join id in _context.TblItemDetails
                on i.ItemId equals id.ItemId
            join ids in _context.TblItemDetailStores
                on id.ItemDetailId equals ids.ItemDetailId
            join s in stores
                on ids.StoreId equals s.Id
            join m in _context.TblBaseInfo
                on id.ManufacturerId equals m.BaseInfoId
            where ic.ParentId == 53

            group new { ids, id, i, ic, s, m }
            by new
            {
                id.ItemDetailId,
                id.BatchNumber,
                id.ExpiryDate,
                ItemClassName = ic.Title,
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

    public async Task<(List<GroupedAvailableQuantityDetailDto> data, int count)> GetAvailableQuantityDetailReportAsync(
        AvailableQuantityReportQuery inputModel,
        int currentRecipientId,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool fetchAll,
        CancellationToken cancellationToken)
    {
        var itemIds = (inputModel.ItemId ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var storeIds = (inputModel.StoreId ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var itemQuery = itemIds.Any()
            ? _context.TblItems.Where(t => itemIds.Contains(t.ItemId))
            : _context.TblItems;

        if (inputModel.ItemClassId is not null)
            itemQuery = itemQuery.Where(t => t.FkClassId == inputModel.ItemClassId);

        var itemDetailQuery = !string.IsNullOrEmpty(inputModel.BatchNumber)
            ? _context.TblItemDetails.Where(t => t.BatchNumber.Contains(inputModel.BatchNumber))
            : _context.TblItemDetails;

        if (inputModel.ManufacturerId is not null)
            itemDetailQuery = itemDetailQuery.Where(t => t.ManufacturerId == inputModel.ManufacturerId);

        if (inputModel.VvmtypeId is not null)
            itemDetailQuery = itemDetailQuery.Where(t => t.VvmTypeId == inputModel.VvmtypeId);

        if (inputModel.CommercialNameId is not null)
            itemDetailQuery = itemDetailQuery.Where(t => t.CommercialNameId == inputModel.CommercialNameId);

        if (!string.IsNullOrEmpty(inputModel.ProductinDate))
        {
            var pd = DateTimeHelper.ToGregorian(inputModel.ProductinDate);
            itemDetailQuery = itemDetailQuery.Where(t => t.ProductionDate.HasValue && t.ProductionDate == pd);
        }

        if (!string.IsNullOrEmpty(inputModel.ExpiryDate))
        {
            var ed = DateTimeHelper.ToGregorian(inputModel.ExpiryDate);
            itemDetailQuery = itemDetailQuery.Where(t => t.ExpiryDate.HasValue && t.ExpiryDate == ed);
        }

        var itemDetailStoreQuery = storeIds.Any()
            ? _context.TblItemDetailStores.Where(t => t.StoreId.HasValue && storeIds.Contains(t.StoreId.Value))
            : _context.TblItemDetailStores;

        if (inputModel.VvmstageId is not null)
            itemDetailStoreQuery = _context.TblItemDetailStores.Where(t => t.FkVvmStageId == inputModel.VvmstageId);

        var storeQuery = _context.TblStores.AsQueryable();
        if (inputModel.AllSubRecipientStocks)
        {
            var recipients = _context.TblRecipients
                .Where(t => t.ParentId == currentRecipientId)
                .Select(s => s.RecipientId)
                .ToList();
            storeQuery = storeQuery.Where(t => t.RecipientId.HasValue && recipients.Contains(t.RecipientId.Value));
        }
        else if (inputModel.RecipientId is not null && inputModel.RecipientId != 0)
        {
            storeQuery = storeQuery.Where(t => t.RecipientId == inputModel.RecipientId);
        }
        else
        {
            storeQuery = storeQuery.Where(t => t.RecipientId == currentRecipientId);
        }

        var parentQuery =
            from i in itemQuery
            join id in itemDetailQuery on i.ItemId equals id.ItemId
            join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0) on id.ItemDetailId equals ids.ItemDetailId
            join s in storeQuery on ids.StoreId equals s.Id
            select new { i.ItemId, i.FullName };

        var total = await parentQuery
            .Select(x => x.ItemId)
            .Distinct()
            .CountAsync(cancellationToken);

        List<int> selectedItemIds;

        if (fetchAll)
        {
            selectedItemIds = await parentQuery
                .Select(x => x.ItemId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else
        {
            selectedItemIds = await parentQuery
                .Select(x => x.ItemId)
                .Distinct()
                .OrderBy(x => x)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        if (selectedItemIds.Count == 0)
            return (new List<GroupedAvailableQuantityDetailDto>(), total);

        var detailQuery =
            from i in itemQuery.Where(t => selectedItemIds.Contains(t.ItemId))
            join id in itemDetailQuery on i.ItemId equals id.ItemId
            join ids in itemDetailStoreQuery.Where(t => t.CurrentStock > 0) on id.ItemDetailId equals ids.ItemDetailId
            join s in storeQuery on ids.StoreId equals s.Id
            join r in _context.TblRecipients on s.RecipientId equals r.RecipientId

            join m in _context.TblBaseInfo on id.ManufacturerId equals m.BaseInfoId into mJoin
            from m in mJoin.DefaultIfEmpty()

            join c in _context.TblBaseInfo on id.CommercialNameId equals c.BaseInfoId into cJoin
            from c in cJoin.DefaultIfEmpty()

            join dpv in _context.TblBaseInfo on id.DosePerVialId equals dpv.BaseInfoId into dpvJoin
            from dpv in dpvJoin.DefaultIfEmpty()

            join vt in _context.TblBaseInfo on id.VvmTypeId equals vt.BaseInfoId into vtJoin
            from vt in vtJoin.DefaultIfEmpty()

            join vs in _context.TblBaseInfo on ids.FkVvmStageId equals vs.BaseInfoId into vsJoin
            from vs in vsJoin.DefaultIfEmpty()

            group new { i, id, ids, s, m, c, dpv, vt, vs }
            by new
            {
                i.ItemId,
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
                DoesPerVial = g.Key.DoesPerVial,
                Stock = g.Key.StoreName,
                Recipient = g.Key.Recipient,
                ProductionDate = g.Key.ProductionDate,
                CommercialName = g.Key.CommercialName,
                Vvmstage = g.Key.Vvmstage,
                Vvmtype = g.Key.Vvmtype,
                CurrentStock = g.Sum(t => t.ids.CurrentStock)
            };

        detailQuery = detailQuery
            .AsNoTracking()
            .ApplySorting(sort, direction == "desc");

        var list = await detailQuery.ToListAsync(cancellationToken);

        var grouped = list
            .GroupBy(x => x.ItemName)
            .Select(g => new GroupedAvailableQuantityDetailDto
            {
                Meta = new AvailableQuantityDetailMetaDto
                {
                    ItemName = g.Key,
                    Count = g.Sum(t => t.CurrentStock) ?? 0
                },
                Details = g.ToList()
            })
            .ToList();

        return (grouped, total);
    }
}
