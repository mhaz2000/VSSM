using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.ItemDoseAssignment;

namespace VSSM.Presentation.Web.Controllers;

public class ItemDoseAssignmentController(
    IItemDoseAssignmentService itemDoseAssignmentService,
    IBaseInfoService baseInfoService) : BaseController
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                            int page = 1,
                                            int pageSize = 10,
                                            string sortField = "ItemDoseAssignmentId",
                                            string sortOrder = "desc")
    {
        var pagedData = await itemDoseAssignmentService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    cancellationToken);

        await PrepareDropdown(cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sortField,
            CurrentDirection = sortOrder,
            Data = new PagedResult<object>
            {
                Items = pagedData.Items.Cast<object>().ToList(),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = pagedData.Paging.TotalItems
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "کالا", PropertyName = nameof(ItemDoseAssignmentListDto.ItemName), Sortable = true },
                new() { Header = "حجم بر دوز (cm3)", PropertyName = nameof(ItemDoseAssignmentListDto.VialPerDoseCm3), Sortable = false },
                new() { Header = "دوز بر ویال", PropertyName = nameof(ItemDoseAssignmentListDto.DoseNum), Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true}
            },
            Actions = new List<GridAction>
            {
                new()
                {
                    Title = "ویرایش",
                    Controller = "ItemDoseAssignment",
                    Action = "Edit",
                    RouteIdProperty = nameof(ItemDoseAssignmentListDto.ItemDoseAssignmentId),
                    LinkClass = "",
                    IconClass = "fa fa-edit",
                    OpenModal = false
                },
                new()
                {
                    Title = "حذف",
                    Controller = "ItemDoseAssignment",
                    Action = "Delete",
                    RouteIdProperty = nameof(ItemDoseAssignmentListDto.ItemDoseAssignmentId),
                    LinkClass = "delete-item",
                    IconClass = "fa fa-trash",
                    Method = "Post",
                }
            }
        };

        return View(model);
    }
    public async Task<IActionResult> New()
    {
        await PrepareDropdown(CancellationToken.None);
        return View();
    }

    public async Task<IActionResult> Create(ItemDoseAssignmentViewModel model, CancellationToken cancellationTOken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "تمام فیلدهای اجباری باید پر شوند";
            TempData["ToastType"] = "error";
            await PrepareDropdown(cancellationTOken);
            return View("New", model);
        }

        var dto = new ItemDoseAssignmentCreateDto
        {
            ItemDoseAssignmentId = model.ItemDoseAssignmentId.GetValueOrDefault(),
            ItemId = model.ItemId,
            DoseId = model.DoseId,
            VialPerDoseCm3 = model.VialPerDoseCm3
        };

        if (model.ItemDoseAssignmentId == null || model.ItemDoseAssignmentId == 0)
        {
            // ---------- Create ----------
            var res = await itemDoseAssignmentService.Create(dto, cancellationTOken);
        }
        else
        {
            // ---------- Update ----------
            var res = await itemDoseAssignmentService.Update(dto, cancellationTOken);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }


    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        // داده را از دیتابیس می‌خوانیم
        var item = await itemDoseAssignmentService.GetById(id, cancellationToken);
        if (item == null)
            return NotFound();

        var model = new ItemDoseAssignmentViewModel
        {
            ItemClassId = item.ItemClassId,
            ItemDoseAssignmentId = item.ItemDoseAssignmentId,
            ItemId = item.ItemId.GetValueOrDefault(),
            DoseId = item.DoseId.GetValueOrDefault(),
            VialPerDoseCm3 = item.VialPerDoseCm3.GetValueOrDefault()
        };

        await PrepareDropdown(cancellationToken);
        // نمایش فرم ویرایش با داده‌ها
        return View("New", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        await itemDoseAssignmentService.Delete(id, token);

        TempData["ToastMessage"] = "با موفقیت حذف شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    private async Task PrepareDropdown(CancellationToken cancellationTOken)
    {
        ViewBag.ItemClass = (await baseInfoService.GetByParentIdAsync(53, cancellationTOken))
         .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
         .ToList();

        var dosePerVial = await baseInfoService.GetByParentIdAsync(84, cancellationTOken);
        ViewBag.DosePerVial = dosePerVial;
    }

}
