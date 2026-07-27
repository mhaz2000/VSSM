using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Services.ExcelGenerators;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class OutlierReportsController(IReportService _reportService, IItemService _itemService,
    IReportExcelGenerator _reportExcelGenerator) : BaseController
{

    #region Total Loss
    public async Task<IActionResult> TotalLoss(CancellationToken cancellationToken = default)
    {
        // Load dropdowns
        ViewBag.Items = await _itemService.GetActives(cancellationToken);

        return View(null);
    }

    public async Task<IActionResult> TotalLossReport(
        long? itemId,
        string fromDate,
        string toDate,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "ItemName",
        string direction = "asc",
        bool toExcel = false,
        CancellationToken cancellationToken = default)
    {
        DateTime fromDateTime = fromDate.ParseWhetherGeorgianOrJalli();
        DateTime toDateTime = toDate.ParseWhetherGeorgianOrJalli();

        var (data, count) = await _reportService.GetTotalLossAsync(
            itemId,
            fromDateTime,
            toDateTime,
            pageNumber,
            pageSize,
            sort,
            direction,
            false,
            cancellationToken
        );

        if (toExcel)
        {

            MemoryStream stream = _reportExcelGenerator.GetTotalLossExcel(data);
            return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "گزارش اتلاف کل.xlsx");
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
                    new() { Header = "کالا", PropertyName = nameof(TotalLossReportDto.Item) },
                    new() { Header = "دلیل امحا", PropertyName = nameof(TotalLossReportDto.DestroyReason) },
                    new() { Header = "امحا شده توسط", PropertyName = nameof(TotalLossReportDto.DestroyBy) },
                    new() { Header = "تاریخ", PropertyName = nameof(TotalLossReportDto.Date) },
                }
        };

        return View(model);
    }

    #endregion

    #region ExpiredItems

    public async Task<IActionResult> ExpiredItemsReport(int pageNumber = 1,
        int pageSize = 20,
        string sort = "ItemName",
        string direction = "asc",
        bool toExcel = false,
        string? reportId = null,
        CancellationToken cancellationToken = default)
    {


        var (data, count) = await _reportService.GetExpiredItemAsync(CurrentUserRecipientId.Value,
            pageNumber,
            pageSize,
            sort,
            direction,
            false,
            cancellationToken);

        if (toExcel)
        {

            MemoryStream stream = _reportExcelGenerator.ExpiredItemsReportExcel(data);
            return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "گزارش کالاهای منقضی شده.xlsx");
        }



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
            Groups = data.Select(s => new GroupItem<object, object>() { Details = s.Details.Cast<object>().ToList(), Meta = s.Meta }).ToList(),
            Columns = new List<GridColumn>
                {
                    new() { Header = "سری ساخت", PropertyName = nameof(ExpiredItemDto.BatchNumber) },
                    new() { Header = "سازنده", PropertyName = nameof(ExpiredItemDto.ManufacturerName) },
                    new() { Header = "تاریخ انقضا", PropertyName = nameof(ExpiredItemDto.ExpiryDateText) },
                    new() { Header = "دوز بر ویال", PropertyName = nameof(ExpiredItemDto.DoesPerVial) },
                    new() { Header = "تعداد", PropertyName = nameof(ExpiredItemDto.CurrentStock) },
                    new() { Header = "تعداد ویال", PropertyName = nameof(ExpiredItemDto.VialNumber) },
                    new() { Header = "انبار", PropertyName = nameof(ExpiredItemDto.Stock) },
                    new() { Header = "مرحله VVM", PropertyName = nameof(ExpiredItemDto.Vvmstage) },
                }
        };

        return View(model);

    }

    #endregion
}
