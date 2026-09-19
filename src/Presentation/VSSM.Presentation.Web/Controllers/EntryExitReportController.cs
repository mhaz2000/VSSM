using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.Recipient;
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
                new() { Header = "نوع کالا",     PropertyName = nameof(ArrivalReportDto.ItemClass) },
                new() { Header = "کالا",         PropertyName = nameof(ArrivalReportDto.Item) },
                new() { Header = "کد کالا",      PropertyName = nameof(ArrivalReportDto.Code) },
                new() { Header = "سازنده",       PropertyName = nameof(ArrivalReportDto.Manufacturer) },
                new() { Header = "تاریخ ورود",   PropertyName = nameof(ArrivalReportDto.ArrivalDate) },
                new() { Header = "شماره گزارش", PropertyName = nameof(ArrivalReportDto.ReportNumber) },
                new() { Header = "سری ساخت",    PropertyName = nameof(ArrivalReportDto.BatchNumber) },
                new() { Header = "تعداد",        PropertyName = nameof(ArrivalReportDto.Quantity) },
                new() { Header = "دوز بر ویال", PropertyName = nameof(ArrivalReportDto.DosePerVial) },
                new() { Header = "تعداد ویال",  PropertyName = nameof(ArrivalReportDto.NumVials) },
                new() { Header = "تاریخ انقضا", PropertyName = nameof(ArrivalReportDto.ExpiryDate) },
                new() { Header = "انبار",        PropertyName = nameof(ArrivalReportDto.Store) },
                new() { Header = "گیرنده",       PropertyName = nameof(ArrivalReportDto.Recipient) },
            }
        };

        return View("ArrivalByItemReport", model);
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
                new() { Header = "نوع کالا",  PropertyName = nameof(ArrivalSummaryDto.ItemClass) },
                new() { Header = "کالا",      PropertyName = nameof(ArrivalSummaryDto.Item) },
                new() { Header = "کد کالا",   PropertyName = nameof(ArrivalSummaryDto.Code) },
                new() { Header = "جمع تعداد", PropertyName = nameof(ArrivalSummaryDto.TotalQuantity) },
                new() { Header = "جمع ویال",  PropertyName = nameof(ArrivalSummaryDto.TotalVials) },
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