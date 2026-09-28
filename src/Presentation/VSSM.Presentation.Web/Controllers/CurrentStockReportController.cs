using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.ExcelGenerators;
using VSSM.Application.Shared;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class CurrentStockReportController(IReportService _reportService,
    IReportExcelGenerator _reportExcelGenerator,
    IItemService _itemService,
    IStoreService _storeService,
    IRecipientService _recipientService,
    IMemoryCache _cache,
    IBaseInfoService _baseInfoService,
    IItemDetailsService _itemDetailsService,
    IFiscalPeriodService _fiscalPeriodService)
    : BaseController
{

    private const string AllItemsLabel = "کلیه کالاها";
    private const string AllStoresLabel = "کلیه انبارها";
    private const string AllRecipientsLabel = "همه مراکز";
    private const int AllRecipientsValue = 0;

    #region AvailableQuantity

    public async Task<IActionResult> AvailableQuantity(CancellationToken cancellationToken = default)
    {
        ViewBag.CommercialNames = await _baseInfoService.GetByParentIdAsync(35, cancellationToken);
        ViewBag.ItemClasses = await _baseInfoService.GetByParentIdAsync(BaseInfoIds.ItemClassesId, cancellationToken);
        ViewBag.Items = await _itemService.GetActives(cancellationToken);
        ViewBag.Sources = await _baseInfoService.GetByParentIdAsync(BaseInfoIds.EnergySources, cancellationToken);
        ViewBag.Stores = await _storeService.GetAllWithRecipientId(CurrentUserRecipientId!.Value, cancellationToken);
        ViewBag.Manufacturers = await _baseInfoService.GetByParentIdAsync(48, cancellationToken);
        ViewBag.Vvmstages = await _baseInfoService.GetByParentIdAsync(30, cancellationToken);
        ViewBag.Vvmtypes = await _baseInfoService.GetByParentIdAsync(22, cancellationToken);

        var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
        ViewBag.Recipients = recipients;

        var firstChainLevel = recipients
            .Where(t => t.ChainLevel is not null)
            .OrderBy(t => t.ChainLevel)
            .FirstOrDefault()?.ChainLevel ?? 0;

        var lastChainLevel = recipients
            .Where(t => t.ChainLevel is not null)
            .OrderByDescending(t => t.ChainLevel)
            .FirstOrDefault()?.ChainLevel ?? 0;

        ViewBag.ChainLevels = Enumerable
            .Range(firstChainLevel, lastChainLevel - firstChainLevel + 1)
            .ToList();

        return View(null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AvailableQuantityReport(
        AvailableQuantityReportQuery inputModel,
        string reportType,
        int pageNumber = 1,
        int pageSize = 1,
        string sort = "ItemName",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        ViewBag.IsPost = true;

        return reportType switch
        {
            "detail" => await AvailableQuantityDetailReport(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            "summary" => await AvailableQuantitySummaryReport(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            "summaryWithDosePerVial" => await AvailableQuantitySummaryWithDosePerVialReport(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            "quantitySummary" => await AvailableQuantityQuantitySummaryWithDosePerVialReport(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            _ => View(null)
        };
    }

    private async Task<IActionResult> AvailableQuantityQuantitySummaryWithDosePerVialReport(
    AvailableQuantityReportQuery inputModel,
    int pageNumber,
    int pageSize,
    string sort,
    string direction,
    bool toExcel,
    string? reportId,
    CancellationToken cancellationToken)
    {
        const int fixedPageSize = 20;
        pageSize = fixedPageSize;

        PrepareCachedReport(ref inputModel, ref reportId, "quantitySummary",
            pageSize, sort, direction);

        if (toExcel)
        {
            var excelData = await _reportService.GetAvailableQuantityQuantitySummaryReportAsync(
                inputModel, CurrentUserRecipientId.Value,
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            MemoryStream stream = _reportExcelGenerator.AvailableQuantityQuantitySummaryReportExcel(excelData.data);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "مقدار موجود - خلاصه گزارش.xlsx");
        }

        await PopulateHeaderAsync(inputModel, cancellationToken);

        var (data, count) = await _reportService.GetAvailableQuantityQuantitySummaryReportAsync(
            inputModel, CurrentUserRecipientId.Value,
            pageNumber, pageSize, sort, direction, false, cancellationToken);

        var groups = data
            .GroupBy(x => new { x.ItemClass, x.ItemCode, x.ItemName })
            .Select(g => new GroupItem<object, object>
            {
                Meta = new AvailableQuantityQuantitySummaryMetaDto
                {
                    ItemClass = g.Key.ItemClass,
                    ItemCode = g.Key.ItemCode,
                    ItemName = g.Key.ItemName,
                    TotalCount = g.Sum(x => x.CurrentStock ?? 0)
                },
                Details = g.Cast<object>().ToList()
            })
            .ToList();

        var model = new GroupedGridViewModel<object, object>
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Groups = groups,
            Columns = new List<GridColumn>
        {
            new() { Header = "سری ساخت",    PropertyName = nameof(AvailableQuantityQuantitySummaryDto.BatchNumber) },
            new() { Header = "دوز بر ویال", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.DoesPerVial) },
            new() { Header = "تاریخ انقضا", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.ExpiryDateText) },
            new() { Header = "تعداد",       PropertyName = nameof(AvailableQuantityQuantitySummaryDto.CurrentStock) },
        }
        };

        return View("AvailableQuantityQuantitySummaryReport", model);
    }

    private async Task<IActionResult> AvailableQuantitySummaryWithDosePerVialReport(
        AvailableQuantityReportQuery inputModel,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool toExcel,
        string? reportId,
        CancellationToken cancellationToken)
    {
        const int fixedPageSize = 20;
        pageSize = fixedPageSize;

        PrepareCachedReport(ref inputModel, ref reportId, "summaryWithDosePerVial",
            pageSize, sort, direction);

        if (toExcel)
        {
            var excelData = await _reportService.GetAvailableQuantitySummaryWithDosePerVialReportAsync(
                inputModel, CurrentUserRecipientId.GetValueOrDefault(),
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            MemoryStream stream = _reportExcelGenerator.AvailableQuantitySummaryWithDosePerVialReportExcel(excelData.data);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "مقدار موجود - خلاصه گزارش با دوز بر ویال.xlsx");
        }

        await PopulateHeaderAsync(inputModel, cancellationToken);

        var (data, count) = await _reportService.GetAvailableQuantitySummaryWithDosePerVialReportAsync(
            inputModel, CurrentUserRecipientId.GetValueOrDefault(),
            pageNumber, pageSize, sort, direction, false, cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "Item Code",     PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.ItemCode) },
                new() { Header = "کالا",          PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.ItemName) },
                new() { Header = "دوز بر ویال",   PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.DoesPerVial) },
                new() { Header = "تعداد",         PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.CurrentStock) },
            }
        };

        return View("AvailableQuantitySummaryWithDosePerVialReport", model);
    }

    private async Task<IActionResult> AvailableQuantitySummaryReport(
        AvailableQuantityReportQuery inputModel,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool toExcel,
        string? reportId,
        CancellationToken cancellationToken)
    {

        const int fixedPageSize = 20;
        pageSize = fixedPageSize;

        PrepareCachedReport(ref inputModel, ref reportId, "summary",
            pageSize, sort, direction);

        if (toExcel)
        {
            var excelData = await _reportService.GetAvailableQuantitySummaryReportAsync(
                inputModel, CurrentUserRecipientId.GetValueOrDefault(),
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            MemoryStream stream = _reportExcelGenerator.AvailableQuantitySummaryReportExcel(excelData.data);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "مقدار موجود - گزارش خلاصه موجودی.xlsx");
        }

        await PopulateHeaderAsync(inputModel, cancellationToken);

        var (data, count) = await _reportService.GetAvailableQuantitySummaryReportAsync(
            inputModel, CurrentUserRecipientId.GetValueOrDefault(),
            pageNumber, pageSize, sort, direction, false, cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "Item Code", PropertyName = nameof(AvailableQuantitySummaryDto.ItemCode) },
                new() { Header = "کالا",      PropertyName = nameof(AvailableQuantitySummaryDto.ItemName) },
                new() { Header = "تعداد",     PropertyName = nameof(AvailableQuantitySummaryDto.CurrentStock) },
            }
        };

        return View("AvailableQuantitySummaryReport", model);
    }

    private async Task<IActionResult> AvailableQuantityDetailReport(
        AvailableQuantityReportQuery inputModel,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool toExcel,
        string? reportId,
        CancellationToken cancellationToken)
    {
        PrepareCachedReport(ref inputModel, ref reportId, "detail",
            pageSize, sort, direction);

        if (toExcel)
        {
            var excelData = await _reportService.GetAvailableQuantityDetailReportAsync(
                inputModel, CurrentUserRecipientId.GetValueOrDefault(),
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            MemoryStream stream = _reportExcelGenerator.AvailableQuantityDetailReportExcel(excelData.data);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "مقدار موجود - جزئیات گزارش.xlsx");
        }

        await PopulateHeaderAsync(inputModel, cancellationToken);

        var (data, count) = await _reportService.GetAvailableQuantityDetailReportAsync(
            inputModel, CurrentUserRecipientId.GetValueOrDefault(),
            pageNumber, pageSize, sort, direction, false, cancellationToken);

        var model = new GroupedGridViewModel<object, object>
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Groups = data.Select(s => new GroupItem<object, object>
            {
                Details = s.Details.Cast<object>().ToList(),
                Meta = s.Meta
            }).ToList(),
            Columns = new List<GridColumn>
            {
                new() { Header = "سری ساخت",              PropertyName = nameof(AvailableQuantityDetailDto.BatchNumber) },
                new() { Header = "سازنده",                PropertyName = nameof(AvailableQuantityDetailDto.ManufacturerName) },
                new() { Header = "تاریخ انقضا",           PropertyName = nameof(AvailableQuantityDetailDto.ShamsiExpiryDate) },
                new() { Header = "روزهای مانده به انقضا", PropertyName = nameof(AvailableQuantityDetailDto.DaysToBeExpired) },
                new() { Header = "تعداد",                 PropertyName = nameof(AvailableQuantityDetailDto.CurrentStock) },
                new() { Header = "دوز بر ویال",           PropertyName = nameof(AvailableQuantityDetailDto.DoesPerVial) },
                new() { Header = "انبار",                 PropertyName = nameof(AvailableQuantityDetailDto.Stock) },
                new() { Header = "گیرنده",                PropertyName = nameof(AvailableQuantityDetailDto.Recipient) },
                new() { Header = "تاریخ تولید",           PropertyName = nameof(AvailableQuantityDetailDto.ProductionDateText) },
                new() { Header = "نام تجاری",             PropertyName = nameof(AvailableQuantityDetailDto.CommercialName) },
                new() { Header = "نوع VVM",               PropertyName = nameof(AvailableQuantityDetailDto.Vvmtype) },
                new() { Header = "مرحله VVM",             PropertyName = nameof(AvailableQuantityDetailDto.Vvmstage) },
            }
        };

        return View(model);
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Cache/reportId را مدیریت می‌کند و AdditionalParam را ست می‌کند
    /// تا در Viewهای Report برای Pagination و Excel Export قابل استفاده باشد.
    /// </summary>
    private void PrepareCachedReport(
        ref AvailableQuantityReportQuery inputModel,
        ref string? reportId,
        string reportType,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = _cache.Get<AvailableQuantityReportQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                _cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.AdditionalParam =
            $"reportType={reportType}" +
            $"&reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }

    /// <summary>
    /// هدر پارامترهای گزارش (انبارها، کالاها، همه مراکز) را آماده می‌کند.
    /// </summary>
    private async Task PopulateHeaderAsync(
        AvailableQuantityReportQuery inputModel,
        CancellationToken cancellationToken)
    {
        await SetStoresAndItemsViewBagAsync(inputModel.StoreId, inputModel.ItemId, cancellationToken);
        await SetRecipientTitleViewBagAsync(inputModel.RecipientId, cancellationToken);
    }

    private async Task SetStoresAndItemsViewBagAsync(
        string inputStoreIds,
        string inputItemIds,
        CancellationToken cancellationToken)
    {
        // ── Stores ──
        var stores = await _storeService.GetAllWithRecipientId(
            CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

        var storeIds = ParseIds(inputStoreIds);

        ViewBag.Stores =
            storeIds.Count == 0
                ? AllStoresLabel
                : !stores.Select(s => s.StoreId).Except(storeIds).Any()
                    ? AllStoresLabel
                    : await _storeService.GetStoreNamesAsync(storeIds);

        // ── Items ──
        var items = await _itemService.GetActives(cancellationToken);
        var itemIds = ParseIds(inputItemIds);

        ViewBag.Items =
            itemIds.Count == 0
                ? AllItemsLabel
                : !items.Select(s => s.ItemId).Except(itemIds).Any()
                    ? AllItemsLabel
                    : await _itemService.GetItemNamesAsync(itemIds);
    }

    private async Task SetRecipientTitleViewBagAsync(
        int? recipientId,
        CancellationToken cancellationToken)
    {
        if (recipientId is null || recipientId.Value == AllRecipientsValue)
        {
            ViewBag.RecipientTitle = AllRecipientsLabel;
            return;
        }

        var recipients = await _recipientService.GetActiveRecipientChildren(
            CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

        ViewBag.RecipientTitle =
            recipients.FirstOrDefault(r => r.RecipientId == recipientId.Value)?.FullName
            ?? AllRecipientsLabel;
    }

    /// <summary>
    /// رشته‌ی comma-separated را با TryParse به لیست int تبدیل می‌کند
    /// تا input نامعتبر باعث Exception نشود.
    /// </summary>
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

    #endregion

    #region PhysicalInventory

    public async Task<IActionResult> PhysicalInventory(CancellationToken cancellationToken = default)
    {
        ViewBag.ItemClasses = await _baseInfoService.GetByParentIdAsync(
            BaseInfoIds.ItemClassesId, cancellationToken);
        ViewBag.Items = await _itemService.GetActives(cancellationToken);
        ViewBag.Stores = await _storeService.GetAllWithRecipientId(
            CurrentUserRecipientId!.Value, cancellationToken);

        return View(null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhysicalInventoryReport(
        PhysicalInventoryQuery inputModel,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "ItemName",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        PrepareCachedPhysicalInventoryReport(
            ref inputModel, ref reportId, pageSize, sort, direction);

        var currentRecipientId = CurrentUserRecipientId!.Value;

        // ── Excel ──
        if (toExcel)
        {
            var excelData = await _reportService.GetInventoryQuantitiesAsync(
                inputModel.ItemClassId,
                inputModel.ItemId,
                inputModel.SourceId,
                inputModel.StoreId,
                pageNumber, pageSize, sort, direction,
                currentRecipientId,
                true,
                cancellationToken);

            MemoryStream stream = _reportExcelGenerator.PhysicalInventoryExcel(excelData.data);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PhysicalInventoryReport.xlsx");
        }

        // ── Grid ──
        var (data, count) = await _reportService.GetInventoryQuantitiesAsync(
            inputModel.ItemClassId,
            inputModel.ItemId,
            inputModel.SourceId,
            inputModel.StoreId,
            pageNumber, pageSize, sort, direction,
            currentRecipientId,
            false,
            cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "نوع کالا",  PropertyName = nameof(InventoryQuantityReportDto.ItemClass) },
            new() { Header = "کالا",      PropertyName = nameof(InventoryQuantityReportDto.Item) },
            new() { Header = "سازنده",    PropertyName = nameof(InventoryQuantityReportDto.Manufacturer) },
            new() { Header = "سری ساخت",  PropertyName = nameof(InventoryQuantityReportDto.BatchNumber) },
            new() { Header = "تاریخ انقضا", PropertyName = nameof(InventoryQuantityReportDto.ExpiryDate) },
            new() { Header = "نام انبار", PropertyName = nameof(InventoryQuantityReportDto.Store) },
            new() { Header = "تعداد",     PropertyName = nameof(InventoryQuantityReportDto.Quantity) },
        }
        };

        return View("PhysicalInventoryReport", model);
    }

    private void PrepareCachedPhysicalInventoryReport(
        ref PhysicalInventoryQuery inputModel,
        ref string? reportId,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = _cache.Get<PhysicalInventoryQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                _cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.ReportId = reportId;
        ViewBag.AdditionalParam =
            $"reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }

    #endregion

    #region NotDispatchedToRecipients

    public async Task<IActionResult> NotDispatchedToRecipients(CancellationToken cancellationToken = default)
    {
        ViewBag.ItemClasses = await _baseInfoService.GetByParentIdAsync(
            BaseInfoIds.ItemClassesId, cancellationToken);

        ViewBag.FiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

        return View(null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NotDispatchedToRecipientsReport(
        NotDispatchedToRecipientsQuery inputModel,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "FullName",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        PrepareCachedNotDispatchedReport(
            ref inputModel, ref reportId, pageSize, sort, direction);

        // ── Excel ──
        if (toExcel)
        {
            var excelData = await _reportService.GetNotDispatchedToRecipientsAsync(
                inputModel.ItemId,
                inputModel.FiscalPeriodId,
                inputModel.FromDate,
                inputModel.ToDate,
                pageNumber, pageSize, sort, direction,
                true,
                cancellationToken);

            MemoryStream stream = _reportExcelGenerator.NotDispatchedToRecipientsExcel(excelData.data);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ZeroDispatchReport.xlsx");
        }

        // ── Grid ──
        var (data, count) = await _reportService.GetNotDispatchedToRecipientsAsync(
            inputModel.ItemId,
            inputModel.FiscalPeriodId,
            inputModel.FromDate,
            inputModel.ToDate,
            pageNumber, pageSize, sort, direction,
            false,
            cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "گیرنده", PropertyName = nameof(NotDispatchedToRecipientsDto.FullName) },
        }
        };

        return View("NotDispatchedToRecipientsReport", model);
    }

    private void PrepareCachedNotDispatchedReport(
        ref NotDispatchedToRecipientsQuery inputModel,
        ref string? reportId,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = _cache.Get<NotDispatchedToRecipientsQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                _cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.ReportId = reportId;
        ViewBag.AdditionalParam =
            $"reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }

    #endregion

    #region ItemForRecipientInYear

    public async Task<IActionResult> ItemForRecipientInYear(CancellationToken cancellationToken = default)
    {
        ViewBag.Items = await _itemService.GetActives(cancellationToken);

        var recipients = await _recipientService.GetActiveRecipientChildren(
            CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);
        ViewBag.Recipients = recipients;

        var firstChainLevel = recipients
            .Where(t => t.ChainLevel is not null)
            .OrderBy(t => t.ChainLevel)
            .FirstOrDefault()?.ChainLevel ?? 0;

        var lastChainLevel = recipients
            .Where(t => t.ChainLevel is not null)
            .OrderByDescending(t => t.ChainLevel)
            .FirstOrDefault()?.ChainLevel ?? 0;

        ViewBag.ChainLevels = Enumerable
            .Range(firstChainLevel, lastChainLevel - firstChainLevel + 1)
            .ToList();

        var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

        var firstYear = fiscalPeriods
            .Where(t => t.StartDate is not null)
            .OrderBy(t => t.StartDate!.Value.Year)
            .FirstOrDefault()?.StartDate?.Year ?? 0;

        var lastYear = fiscalPeriods
            .Where(t => t.EndTime is not null)
            .OrderByDescending(t => t.EndTime!.Value.Year)
            .FirstOrDefault()?.EndTime?.Year ?? 0;

        ViewBag.Years = Enumerable
            .Range(firstYear, lastYear - firstYear + 1)
            .ToList();

        return View(null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ItemForRecipientInYearReport(
        ItemForRecipientInYearQuery inputModel,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "Reciever",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        PrepareCachedItemForRecipientInYearReport(
            ref inputModel, ref reportId, pageSize, sort, direction);

        var currentRecipientId = CurrentUserRecipientId.GetValueOrDefault();

        // ── Excel ──
        if (toExcel)
        {
            var excelData = await _reportService.GetItemForRecipientInYear(
                currentRecipientId,
                inputModel.ItemId,
                inputModel.Year ?? 0,
                inputModel.Level,
                inputModel.RecipientId,
                pageNumber,
                pageSize,
                sort,
                direction,
                true,
                cancellationToken);

            MemoryStream stream = _reportExcelGenerator.ItemForRecipientInYearExcel(excelData.data);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "گزارش کالا برای گیرندگان در سال.xlsx");
        }

        // ── Grid ──
        var (data, count) = await _reportService.GetItemForRecipientInYear(
            currentRecipientId,
            inputModel.ItemId,
            inputModel.Year ?? 0,
            inputModel.Level,
            inputModel.RecipientId,
            pageNumber,
            pageSize,
            sort,
            direction,
            false,
            cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "گیرنده",              PropertyName = nameof(ItemForRecipientInYearDto.Reciever) },
            new() { Header = "ژانویه",              PropertyName = nameof(ItemForRecipientInYearDto.Jan) },
            new() { Header = "فوریه",               PropertyName = nameof(ItemForRecipientInYearDto.Feb) },
            new() { Header = "مارس",                PropertyName = nameof(ItemForRecipientInYearDto.Mar) },
            new() { Header = "آپریل",               PropertyName = nameof(ItemForRecipientInYearDto.Apr) },
            new() { Header = "می",                  PropertyName = nameof(ItemForRecipientInYearDto.May) },
            new() { Header = "ژوئن",                PropertyName = nameof(ItemForRecipientInYearDto.Jun) },
            new() { Header = "جولای",               PropertyName = nameof(ItemForRecipientInYearDto.Jul) },
            new() { Header = "آگوست",               PropertyName = nameof(ItemForRecipientInYearDto.Agu) },
            new() { Header = "سپتامبر",             PropertyName = nameof(ItemForRecipientInYearDto.Sep) },
            new() { Header = "اکتبر",               PropertyName = nameof(ItemForRecipientInYearDto.Oct) },
            new() { Header = "نوامبر",              PropertyName = nameof(ItemForRecipientInYearDto.Nov) },
            new() { Header = "دسامبر",              PropertyName = nameof(ItemForRecipientInYearDto.Dec) },
            new() { Header = "Total Quota",         PropertyName = nameof(ItemForRecipientInYearDto.TotalQuota) },
            new() { Header = "مقدار احتیاج سالانه", PropertyName = nameof(ItemForRecipientInYearDto.YearlyNeed) },
        }
        };

        return View("ItemForRecipientInYearReport", model);
    }

    private void PrepareCachedItemForRecipientInYearReport(
        ref ItemForRecipientInYearQuery inputModel,
        ref string? reportId,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = _cache.Get<ItemForRecipientInYearQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                _cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.ReportId = reportId;
        ViewBag.AdditionalParam =
            $"reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }

    #endregion

    #region Inventory History

    public async Task<IActionResult> InventoryHistory(CancellationToken cancellationToken = default)
    {
        ViewBag.Items = await _itemService.GetActives(cancellationToken);
        var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);
        ViewBag.Recipients = recipients;

        var firstChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderBy(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;
        var lastChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderByDescending(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;

        List<int> chainLevels = new List<int>();
        chainLevels = Enumerable.Range(firstChainLevel, lastChainLevel - firstChainLevel + 1).ToList();
        ViewBag.ChainLevels = chainLevels;

        var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

        ViewBag.FiscalPeriods = fiscalPeriods;

        return View(null);
    }

    public async Task<IActionResult> InventoryHistoryReport(
    long? itemId,
    int? fiscalPeriodId,
    string? level,
    int? recipientId,
    string? fromDate,
    string? toDate,
    int pageNumber = 1,
    int pageSize = 20,
    string sort = "Reciever",
    string direction = "asc",
    bool toExcel = false,
    CancellationToken cancellationToken = default)
    {

        var (data, count) = await _reportService.GetInventoryHistory(
            CurrentUserRecipientId.GetValueOrDefault(),
            itemId,
            fiscalPeriodId,
            string.IsNullOrEmpty(level) ? null : int.Parse(level),
            recipientId,
            fromDate,
            toDate,
            pageNumber,
            pageSize,
            sort,
            direction,
            false,
            cancellationToken
        );

        if (toExcel)
        {
            var excelData = await _reportService.GetInventoryHistory(
            CurrentUserRecipientId.GetValueOrDefault(),
            itemId,
            fiscalPeriodId,
            string.IsNullOrEmpty(level) ? null : int.Parse(level),
            recipientId,
            fromDate,
            toDate,
            pageNumber,
            pageSize,
            sort,
            direction,
            false,
            cancellationToken
        );

            MemoryStream stream = _reportExcelGenerator.InventoryHistoryExcel(excelData.data);
            return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "تاریخچه موجودی.xlsx");
        }

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "مقدار در پایان ماه", PropertyName = nameof(InventoryHistoryDto.InStock) },
                new() { Header = "سال", PropertyName = nameof(InventoryHistoryDto.Year) },
                new() { Header = "ماه", PropertyName = nameof(InventoryHistoryDto.Month) },
            }
        };

        return View(model);
    }

    #endregion

    #region OutOfStockItems

    public async Task<IActionResult> OutOfStockItems(CancellationToken cancellationToken = default)
    {
        ViewBag.ItemClasses = await _baseInfoService.GetByParentIdAsync(
            BaseInfoIds.ItemClassesId, cancellationToken);

        return View(null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OutOfStockItemsReport(
        OutOfStockItemsQuery inputModel,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "ItemName",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        PrepareCachedOutOfStockReport(
            ref inputModel, ref reportId, pageSize, sort, direction);

        var currentRecipientId = CurrentUserRecipientId.GetValueOrDefault();

        if (toExcel)
        {
            var excelData = await _reportService.GetOutOfStockItems(
                currentRecipientId,
                inputModel.ItemClassId,
                pageNumber, pageSize, sort, direction,
                true,
                cancellationToken);

            MemoryStream stream = _reportExcelGenerator.OutOfStockItemsExcel(excelData.data);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "zeroStock.xlsx");
        }

        var (data, count) = await _reportService.GetOutOfStockItems(
            currentRecipientId,
            inputModel.ItemClassId,
            pageNumber, pageSize, sort, direction,
            false,
            cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "نام کالا", PropertyName = nameof(OutOfStockItemsDto.ItemName) }
        }
        };

        return View("OutOfStockItemsReport", model);
    }

    private void PrepareCachedOutOfStockReport(
        ref OutOfStockItemsQuery inputModel,
        ref string? reportId,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = _cache.Get<OutOfStockItemsQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                _cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.ReportId = reportId;
        ViewBag.AdditionalParam =
            $"reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }

    #endregion
    #region Indicator

    public async Task<IActionResult> Indicator(CancellationToken cancellationToken = default)
    {
        ViewBag.ItemClasses = await _baseInfoService.GetByParentIdAsync(
            BaseInfoIds.ItemClassesId, cancellationToken);

        return View(null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IndicatorReport(
        IndicatorReportQuery inputModel,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "Recipient",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        PrepareCachedIndicatorReport(ref inputModel, ref reportId, pageSize, sort, direction);

        if (toExcel)
        {
            var excelData = await _reportService.GetItemIndicator(
                CurrentUserRecipientId.Value,
                inputModel.ItemId,
                inputModel.BatchNumber,
                pageNumber, pageSize, sort, direction, true,
                cancellationToken);

            MemoryStream stream = _reportExcelGenerator.ItemIndicatorExcel(excelData.data);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "گزارش اندیکاتور.xlsx");
        }

        var itemDetail = await _reportService.GetItemIndicatorDetail(
            CurrentUserRecipientId.GetValueOrDefault(),
            inputModel.ItemId,
            inputModel.BatchNumber);

        if (inputModel.ItemId.HasValue)
        {
            itemDetail.CurrentStock = (await _itemDetailsService.GetItemBatchNumbers(
                    inputModel.ItemId.Value,
                    CurrentUserRecipientId.GetValueOrDefault(),
                    CancellationToken.None))
                .FirstOrDefault(t => t.BatchNumber == inputModel.BatchNumber)?.CurrentStock ?? 0;
        }

        ViewBag.ItemDetail = itemDetail;

        var (data, count) = await _reportService.GetItemIndicator(
            CurrentUserRecipientId.Value,
            inputModel.ItemId,
            inputModel.BatchNumber,
            pageNumber, pageSize, sort, direction, false,
            cancellationToken);

        var model = new GridViewModel
        {
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "شماره حواله / شماره گزارش", PropertyName = nameof(ItemIndicatorDto.ReportNumber) },
            new() { Header = "شماره درخواست",             PropertyName = nameof(ItemIndicatorDto.RequestNum) },
            new() { Header = "تاریخ",                     PropertyName = nameof(ItemIndicatorDto.DateText) },
            new() { Header = "از / به",                   PropertyName = nameof(ItemIndicatorDto.Recipient) },
            new() { Header = "ورودی (دریافتی)",           PropertyName = nameof(ItemIndicatorDto.Arrival) },
            new() { Header = "خروجی (ارسالی)",            PropertyName = nameof(ItemIndicatorDto.Dispatch) },
            new() { Header = "مانده",                     PropertyName = nameof(ItemIndicatorDto.CurrentStock) },
            new() { Header = "توضیح",                     PropertyName = nameof(ItemIndicatorDto.Description) },
        }
        };

        return View("IndicatorReport", model);
    }

    private void PrepareCachedIndicatorReport(
        ref IndicatorReportQuery inputModel,
        ref string? reportId,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = _cache.Get<IndicatorReportQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                _cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.ReportId = reportId;
        ViewBag.AdditionalParam =
            $"reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }

    #endregion

    [HttpGet]
    public async Task<IActionResult> GetItemsByClass(int id)
    {
        var apiResult = new ApiResult<List<ItemListDto>>();

        try
        {
            var items = await _itemService.GetItemsByClassAsync(id);
            apiResult.Success = true;
            apiResult.Message = string.Empty;
            apiResult.Result = items;
        }
        catch (Exception ex)
        {
            apiResult.Success = false;
            apiResult.Message = ex.Message;
        }

        return Json(apiResult);
    }

    [HttpGet("GetItemBatchNumbers/{itemId:required:int}")]
    public async Task<IActionResult> GetItemBatchNumbers(int itemId)
    {
        var apiResult = new ApiResult<List<ItemBatchNumbersDto>>();

        try
        {
            var itemDetails = await _itemDetailsService.GetItemBatchNumbers(itemId, CurrentUserRecipientId.Value, CancellationToken.None);

            apiResult.Success = true;
            apiResult.Message = string.Empty;
            apiResult.Result = itemDetails;
        }
        catch (Exception ex)
        {
        }

        return Json(apiResult);
    }

}
