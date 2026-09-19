using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.ExcelGenerators;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class EntryExitReportController(
    IReportService reportService,
    IReportExcelGenerator excelGenerator,
    IItemService itemService,
    IStoreService storeService,
    IRecipientService recipientService,
    IBaseInfoService baseInfoService,
    IFiscalPeriodService fiscalPeriodService,
    IUserService userService,
    IMemoryCache cache)
    : BaseController
{
    private const int DefaultPageSize = 10;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        await PopulateFilterLookupsAsync(cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Report(
        EntryExitReportQuery inputModel,
        string reportType,
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string sort = "ArrivalDate",
        string direction = "desc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {
        ViewBag.IsPost = true;

        return reportType switch
        {
            "arrival" or
            "arrivalByItem" => await ArrivalByItemAsync(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            "dispatchByRecipient" => await DispatchByRecipientAsync(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            "arrivalSummary" => await ArrivalSummaryAsync(
                inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId, cancellationToken),

            _ => View("Index")
        };
    }

    #region Report Handlers

    private async Task<IActionResult> ArrivalByItemAsync(
     EntryExitReportQuery input,
     int pageNumber,
     int pageSize,
     string sort,
     string direction,
     bool toExcel,
     string? reportId,
     CancellationToken cancellationToken)
    {

        const int fixedPageSize = 1;
        pageSize = fixedPageSize;

        PrepareCachedReport(ref input, ref reportId, "arrivalByItem",
            pageSize, sort, direction);

        if (toExcel)
        {
            var (excelData, _) = await reportService.GetArrivalByItemReportAsync(
                input, CurrentUserRecipientId.GetValueOrDefault(),
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            var stream = excelGenerator.ArrivalByItemReportExcel(excelData);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ورود بر اساس کالا.xlsx");
        }

        var (data, count) = await reportService.GetArrivalByItemReportAsync(
            input, CurrentUserRecipientId.GetValueOrDefault(),
            pageNumber, pageSize, sort, direction, false, cancellationToken);

        await PopulateHeaderAsync(input, cancellationToken);

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
            new() { Header = "تاریخ دریافت",   PropertyName = nameof(ArrivalByItemDetailDto.ReceiveDateText) },
            new() { Header = "تاریخ ورود",     PropertyName = nameof(ArrivalByItemDetailDto.ArrivalDateText) },
            new() { Header = "شماره گزارش",    PropertyName = nameof(ArrivalByItemDetailDto.ReportNumber) },
            new() { Header = "منبع",           PropertyName = nameof(ArrivalByItemDetailDto.Source) },
            new() { Header = "شماره سفارش",    PropertyName = nameof(ArrivalByItemDetailDto.OrderNumber) },
            new() { Header = "Entered by",     PropertyName = nameof(ArrivalByItemDetailDto.EnteredBy) },
            new() { Header = "سری ساخت",       PropertyName = nameof(ArrivalByItemDetailDto.BatchNumber) },
            new() { Header = "سازنده",         PropertyName = nameof(ArrivalByItemDetailDto.Manufacturer) },
            new() { Header = "دوز-مقدار",      PropertyName = nameof(ArrivalByItemDetailDto.Quantity) },
            new() { Header = "تعداد ویال",     PropertyName = nameof(ArrivalByItemDetailDto.NumVials) },
            new() { Header = "دوز بر ویال",    PropertyName = nameof(ArrivalByItemDetailDto.DosePerVial) },
            new() { Header = "تاریخ انقضا",    PropertyName = nameof(ArrivalByItemDetailDto.ExpiryDateText) },
            new() { Header = "تاریخ تولید",    PropertyName = nameof(ArrivalByItemDetailDto.ProductionDateText) },
            new() { Header = "انبار",          PropertyName = nameof(ArrivalByItemDetailDto.Store) },
            new() { Header = "نام تجاری",      PropertyName = nameof(ArrivalByItemDetailDto.CommercialName) },
            new() { Header = "نوع VVM",        PropertyName = nameof(ArrivalByItemDetailDto.VvmType) },
            new() { Header = "مرحله VVM",      PropertyName = nameof(ArrivalByItemDetailDto.VvmStage) },
            new() { Header = "ملاحظات",        PropertyName = nameof(ArrivalByItemDetailDto.Note) },
        }
        };

        return View("ArrivalByItemReport", model);
    }

    private async Task PopulateHeaderAsync(
        EntryExitReportQuery input,
        CancellationToken cancellationToken)
    {
        var storeIds = ParseIds(input.StoreId);
        var stores = await storeService.GetAllWithRecipientId(
            CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

        ViewBag.Stores = storeIds.Count == 0 || !stores.Select(s => s.StoreId).Except(storeIds).Any()
            ? "کلیه انبارها"
            : string.Join("، ", stores.Where(s => storeIds.Contains(s.StoreId)).Select(s => s.FullName));

        var itemIds = ParseIds(input.ItemId);
        var items = await itemService.GetActives(cancellationToken);

        ViewBag.Items = itemIds.Count == 0 || !items.Select(i => i.ItemId).Except(itemIds).Any()
            ? "کلیه کالاها"
            : string.Join("، ", items.Where(i => itemIds.Contains(i.ItemId)).Select(i => i.FullName));

        if (input.RecipientId is null or 0)
        {
            ViewBag.RecipientTitle = "همه مراکز";
        }
        else
        {
            var recipients = await recipientService.GetActiveRecipientChildren(
                CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

            ViewBag.RecipientTitle = recipients
                .FirstOrDefault(r => r.RecipientId == input.RecipientId)?.FullName
                ?? "همه مراکز";
        }
    }

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

    private async Task<IActionResult> DispatchByRecipientAsync(
        EntryExitReportQuery input,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool toExcel,
        string? reportId,
        CancellationToken cancellationToken)
    {
        PrepareCachedReport(ref input, ref reportId, "dispatchByRecipient",
            pageSize, sort, direction);

        if (toExcel)
        {
            var (excelData, _) = await reportService.GetDispatchByRecipientReportAsync(
                input, CurrentUserRecipientId.GetValueOrDefault(),
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            var stream = excelGenerator.DispatchByRecipientReportExcel(excelData);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ارسالات بر اساس گیرندگان.xlsx");
        }

        var (data, count) = await reportService.GetDispatchByRecipientReportAsync(
            input, CurrentUserRecipientId.GetValueOrDefault(),
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
                new() { Header = "نوع کالا",       PropertyName = nameof(DispatchReportDto.ItemClass) },
                new() { Header = "کالا",           PropertyName = nameof(DispatchReportDto.Item) },
                new() { Header = "کد کالا",        PropertyName = nameof(DispatchReportDto.Code) },
                new() { Header = "گیرنده",         PropertyName = nameof(DispatchReportDto.Recipient) },
                new() { Header = "تاریخ ارسال",    PropertyName = nameof(DispatchReportDto.DispatchDate) },
                new() { Header = "شماره حواله",    PropertyName = nameof(DispatchReportDto.VoucherNumber) },
                new() { Header = "شماره درخواست",  PropertyName = nameof(DispatchReportDto.RequestNumber) },
                new() { Header = "سری ساخت",       PropertyName = nameof(DispatchReportDto.BatchNumber) },
                new() { Header = "تعداد",          PropertyName = nameof(DispatchReportDto.Quantity) },
                new() { Header = "دوز بر ویال",    PropertyName = nameof(DispatchReportDto.DosePerVial) },
                new() { Header = "تعداد ویال",     PropertyName = nameof(DispatchReportDto.NumVials) },
                new() { Header = "تاریخ انقضا",    PropertyName = nameof(DispatchReportDto.ExpiryDate) },
                new() { Header = "انبار",          PropertyName = nameof(DispatchReportDto.Store) },
            }
        };

        return View("DispatchByRecipientReport", model);
    }

    private async Task<IActionResult> ArrivalSummaryAsync(
        EntryExitReportQuery input,
        int pageNumber,
        int pageSize,
        string sort,
        string direction,
        bool toExcel,
        string? reportId,
        CancellationToken cancellationToken)
    {
        PrepareCachedReport(ref input, ref reportId, "arrivalSummary",
            pageSize, sort, direction);

        if (toExcel)
        {
            var (excelData, _) = await reportService.GetArrivalSummaryReportAsync(
                input, CurrentUserRecipientId.GetValueOrDefault(),
                pageNumber, pageSize, sort, direction, true, cancellationToken);

            var stream = excelGenerator.ArrivalSummaryReportExcel(excelData);
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "خلاصه موارد رسیده.xlsx");
        }

        var (data, count) = await reportService.GetArrivalSummaryReportAsync(
            input, CurrentUserRecipientId.GetValueOrDefault(),
            pageNumber, pageSize, sort, direction, false, cancellationToken);

        await PopulateHeaderAsync(input, cancellationToken);

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
            new() { Header = "نوع کالا",     PropertyName = nameof(ArrivalSummaryDto.ItemClass) },
            new() { Header = "کالا",         PropertyName = nameof(ArrivalSummaryDto.Item) },
            new() { Header = "کد کالا",      PropertyName = nameof(ArrivalSummaryDto.ItemCode) },
            new() { Header = "سازنده",       PropertyName = nameof(ArrivalSummaryDto.Manufacturer) },
            new() { Header = "منبع",         PropertyName = nameof(ArrivalSummaryDto.Source) },
            new() { Header = "فعالیت",       PropertyName = nameof(ArrivalSummaryDto.Activity) },
            new() { Header = "تاریخ ورود",   PropertyName = nameof(ArrivalSummaryDto.ArrivalDateText) },
            new() { Header = "شماره گزارش",  PropertyName = nameof(ArrivalSummaryDto.ReportNumber) },
            new() { Header = "شماره سفارش",  PropertyName = nameof(ArrivalSummaryDto.OrderNumber) },
            new() { Header = "تعداد",         PropertyName = nameof(ArrivalSummaryDto.Quantity) },
            new() { Header = "سری ساخت",     PropertyName = nameof(ArrivalSummaryDto.BatchNumber) },
            new() { Header = "دوز بر ویال",  PropertyName = nameof(ArrivalSummaryDto.DosePerVial) },
            new() { Header = "تعداد ویال",   PropertyName = nameof(ArrivalSummaryDto.NumVials) },
            new() { Header = "قیمت واحد",    PropertyName = nameof(ArrivalSummaryDto.UnitPrice) },
            new() { Header = "مبلغ کل",      PropertyName = nameof(ArrivalSummaryDto.TotalPrice) },
            new() { Header = "تاریخ انقضا",  PropertyName = nameof(ArrivalSummaryDto.ExpiryDateText) },
        }
        };

        return View("ArrivalSummaryReport", model);
    }

    #endregion

    #region Helpers

    private void PrepareCachedReport(
        ref EntryExitReportQuery inputModel,
        ref string? reportId,
        string reportType,
        int pageSize,
        string sort,
        string direction)
    {
        if (!string.IsNullOrEmpty(reportId))
        {
            var cached = cache.Get<EntryExitReportQuery>(reportId);
            if (cached is not null)
            {
                inputModel = cached;
                cache.Remove(reportId);
            }
        }
        else
        {
            reportId = Guid.NewGuid().ToString("N");
        }

        ViewBag.ReportId = reportId;
        ViewBag.AdditionalParam =
            $"reportType={reportType}" +
            $"&reportId={reportId}" +
            $"&pageSize={pageSize}" +
            $"&sort={Uri.EscapeDataString(sort)}" +
            $"&direction={direction}";

        cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
    }


    private async Task PopulateFilterLookupsAsync(CancellationToken cancellationToken)
    {
        var recipientId = CurrentUserRecipientId.GetValueOrDefault();

        ViewBag.ItemClasses = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.ItemClassesId, cancellationToken);

        ViewBag.Items = new List<ItemListDto>();

        ViewBag.Stores = await storeService.GetAllWithRecipientId(
            recipientId, cancellationToken);

        var recipients = await recipientService.GetActiveRecipientChildren(
            recipientId, cancellationToken);

        ViewBag.Recipients = recipients;

        ViewBag.LowerReports = recipients
            .Select(r => new RecipientLookupDto
            {
                Id = r.RecipientId,
                Title = r.FullName ?? string.Empty,
            })
            .ToList();

        ViewBag.Levels = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.Levels, cancellationToken);

        ViewBag.Manufacturers = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.Manufacturers, cancellationToken);

        ViewBag.CommercialNames = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.BrandNames, cancellationToken);

        ViewBag.Sources = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.OwnershipTypes, cancellationToken);

        ViewBag.VvmTypes = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.VvmTypes, cancellationToken);

        ViewBag.VvmStages = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.VvmStages, cancellationToken);

        ViewBag.TransportationMethods = await baseInfoService.GetByParentIdAsync(
            BaseInfoIds.TransportModes, cancellationToken);

        var rawPeriods = await fiscalPeriodService.GetAllAsListAsync(cancellationToken);

        ViewBag.FiscalPeriods = rawPeriods
            .Select(x => new SelectOption
            {
                Value = x.FiscalPeriodId.ToString(),
                Text = x.FullName
            })
            .ToList();

        ViewBag.Users = await userService.GetAdminUsersAsBaseInfoAsync(cancellationToken);
    }



    [HttpGet]
    public async Task<IActionResult> GetItemsByClass(
        int classId,
        CancellationToken cancellationToken = default)
    {
        if (classId <= 0)
        {
            return Json(new
            {
                success = true,
                result = Array.Empty<object>()
            });
        }

        var items = await itemService.GetItemsByClassAsync(classId);

        return Json(new
        {
            success = true,
            result = items.Select(i => new
            {
                itemId = i.ItemId,
                fullName = i.FullName
            })
        });
    }
    #endregion
}