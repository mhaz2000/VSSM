using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.ManItemClass;

namespace VSSM.Presentation.Web.Controllers;

[Authorize]
public class ManItemClassController : BaseController
{
    private readonly IManItemClassService _service;

    public ManItemClassController(IManItemClassService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------
    // INDEX
    // -------------------------------------------------------------
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sortField = "ManItemClassId",
        string sortOrder = "asc")
    {
        var list = await _service.GetPagedList(
            search,
            page,
            pageSize,
            sortField,
            sortOrder,
            cancellationToken);

        return View(list);
    }


    // -------------------------------------------------------------
    // NEW
    // -------------------------------------------------------------
    public async Task<IActionResult> New()
    {
        var manufacturers = await _service.ManufacturerDropDown(CancellationToken.None);

        var viewModel = new ManItemClassViewModel
        {
            ItemClasses = manufacturers
                .Select(c => new SelectListItem
                {
                    Value = c.ItemsClassId.ToString(),
                    Text = c.Name
                })
                .ToList()
        };

        return View("New", viewModel);
    }


    // -------------------------------------------------------------
    // EDIT
    // -------------------------------------------------------------
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.GetById(id, CancellationToken.None);
        if (model == null)
            return NotFound();

        var manufacturers = await _service.ManufacturerDropDown(CancellationToken.None);

        var viewModel = new ManItemClassViewModel
        {
            ManItemClassId = model.ManItemClassId,
            ItemClassCode = model.ItemClassCode,
            ManufacturerIds = model.ManufacturerIds, // multi-select
            ItemClasses = manufacturers
                .Select(c => new SelectListItem
                {
                    Value = c.ItemsClassId.ToString(),
                    Text = c.Name,
                    Selected = model.ManufacturerIds.Any(m=>m == c.ItemsClassId)
                })
                .ToList()
        };

        return View("New", viewModel);
    }


    // -------------------------------------------------------------
    // CREATE / UPDATE (POST)
    // -------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManItemClassViewModel model, CancellationToken cancellationToken)
    {
        // Re-fill dropdown on failure
        if (!ModelState.IsValid)
        {
            var manufacturers = await _service.ManufacturerDropDown(CancellationToken.None);
            model.ItemClasses = manufacturers
                .Select(c => new SelectListItem
                {
                    Value = c.ItemsClassId.ToString(),
                    Text = c.Name,
                    Selected = model.ManufacturerIds.Any(m=>m == c.ItemsClassId)
                })
                .ToList();

            return View("New", model);
        }

        var dto = new ManItemClassDto
        {
            ManItemClassId = model.ManItemClassId,
            ItemClassCode = model.ItemClassCode,
            ManufacturerIds = model.ManufacturerIds  
        };

        if (model.ManItemClassId == null || model.ManItemClassId == 0)
        {
            await _service.Create(dto, cancellationToken);
        }
        else
        {
            await _service.Update(dto, cancellationToken);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _service.Delete(id, cancellationToken);

        TempData["ToastMessage"] = deleted
            ? "عملیات حذف با موفقیت انجام شد"
            : "رکورد مورد نظر یافت نشد";

        TempData["ToastType"] = deleted ? "success" : "error";

        return RedirectToAction("Index");
    }
}
