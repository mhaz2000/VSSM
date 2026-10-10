using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public sealed class FiscalPeriodController(
    IFiscalPeriodService fiscalPeriodService) : BaseController
{

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search,
        string? status = "all",
        int page = 1,
        int pageSize = 10,
        string sort = "FullName",
        string direction = "asc")
    {
        var list = await fiscalPeriodService.GetPagedList(
            search,
            status,
            page,
            pageSize,
            sort,
            direction,
            cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = list.Items.Cast<object>().ToList(),
                PageNumber = list.Paging.Page,
                PageSize = pageSize,
                TotalCount = list.Paging.TotalItems
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "نام کامل", PropertyName = nameof(FiscalPeriodDto.FullName), Sortable = true },
            new() { Header = "تاریخ شروع", PropertyName = nameof(FiscalPeriodDto.StartDateShamsiDate), Sortable = true },
            new() { Header = "تاریخ پایان", PropertyName = nameof(FiscalPeriodDto.EndTimeShamsiDate), Sortable = true },
            new() { Header = "تاریخ پایان پیشنهادی", PropertyName = nameof(FiscalPeriodDto.SuggestedEndDate), Sortable = true },
            new()
            {
                Header = "وضعیت",
                PropertyName = nameof(FiscalPeriodDto.IsActive),
                Sortable = true,
                IsBadgeColumn = true,
                BadgeMap = new Dictionary<string, (string Text, string CssClass)>
                {
                    ["true"] = ("فعال", "badge-success"),
                    ["false"] = ("غیرفعال", "badge-danger")
                }
            },
            new() { Header = "عملیات", IsActionColumn = true }
        },
            Actions = new List<GridAction>
        {
            new()
            {
                Title = "ویرایش",
                Controller = "FiscalPeriod",
                Action = "Edit",
                RouteIdProperty = nameof(FiscalPeriodDto.FiscalPeriodId),
                IconClass = "fa fa-edit"
            },
            new()
            {
                Title = "حذف",
                Controller = "FiscalPeriod",
                Action = "Delete",
                Method = "Post",
                RouteIdProperty = nameof(FiscalPeriodDto.FiscalPeriodId),
                IconClass = "fa fa-trash",
                DynamicTitle = item =>
                {
                    var dto = item as FiscalPeriodDto;
                    return dto?.IsActive == true ? string.Empty : "حذف";
                }
            }
        }
        };

        return View(model);
    }
    // GET: /FiscalPeriod/New
    public IActionResult New()
    {
        var model = new FiscalPeriodDto();
        var today = DateTime.Today;
        var lastDayOfYear = new DateTime(today.Year, 12, 31);

        // تنظیم مقدار پیش‌فرض
        model.SuggestedEndDate = lastDayOfYear.ToShamsi();

        return View(model);
    }

    // POST: /FiscalPeriod/New
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(FiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await fiscalPeriodService.Create(dto, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    // GET: /FiscalPeriod/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var dto = await fiscalPeriodService.GetById(id, cancellationToken);
        if (dto == null)
            return NotFound();

        return View("New", dto);
    }

    // POST: /FiscalPeriod/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("New", dto);
        }

        var updated = await fiscalPeriodService.Update(dto, cancellationToken);
        if (!updated)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // POST: /FiscalPeriod/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await fiscalPeriodService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
