using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.ManItemClass;
using VSSM.Presentation.Web.Models.Manufacturer;

namespace VSSM.Presentation.Web.Controllers;

public class ManItemClassController : Controller
{
    private readonly IManItemClassService _service;

    public ManItemClassController(IManItemClassService service)
    {
        _service = service;
    }

    // ---------------------- INDEX (LIST) ----------------------
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

        return View(list); // Views/ManItemClass/Index.cshtml
    }

    // ---------------------- NEW ----------------------
    public IActionResult New()
    {
        return View("New", new ManItemClassViewModel()); // Views/ManItemClass/New.cshtml
    }

    public async Task<IActionResult> ManufacturerDropDown(CancellationToken cancellationToken)
    {
        var manufacturers = await _service.ManufacturerDropDown(cancellationToken);
        return View("New", manufacturers);
    }

    // ---------------------- EDIT ----------------------
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var entity = await _service.GetById(id, cancellationToken);
        if (entity == null)
            return NotFound();

        var model = new ManItemClassViewModel
        {
            ManItemClassId = entity.ManItemClassId,
            ItemClassCode = entity.ItemClassCode,
            ManufacturerId = entity.ManufacturerId
        };

        return View("New", model); // reuse the New form
    }

    // ---------------------- CREATE / UPDATE ----------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManItemClassViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        var dto = new ManItemClassDto
        {
            ManItemClassId = model.ManItemClassId.GetValueOrDefault(),
            ItemClassCode = model.ItemClassCode,
            ManufacturerId = model.ManufacturerId
        };

        if (model.ManItemClassId == null || model.ManItemClassId == 0)
        {
            // Create
            await _service.Create(dto, cancellationToken);
        }
        else
        {
            // Update
            await _service.Update(dto, cancellationToken);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    // ---------------------- DELETE ----------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _service.Delete(id, cancellationToken);

        if (deleted)
        {
            TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] = "رکورد مورد نظر یافت نشد";
            TempData["ToastType"] = "error";
        }

        return RedirectToAction("Index");
    }
}