using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.ManItemClass;

namespace VSSM.Presentation.Web.Controllers;

public class ManItemClassController : Controller
{
    private readonly IManItemClassService _service;

    public ManItemClassController(IManItemClassService service)
    {
        _service = service;
    }

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
    
    public async Task<IActionResult> New()
    {
        var manufacturers = await _service.ManufacturerDropDown(CancellationToken.None);

        var viewModel = new ManItemClassViewModel();
        viewModel.Manufacturers = manufacturers
            .Select(c => new SelectListItem
            {
                Value = c.ManufacturerId.ToString(),
                Text = c.Name
            })
            .ToList();

        return View("New", viewModel);   // 👈 return the filled viewmodel
    }

    public async Task<IActionResult> ManufacturerDropDown(CancellationToken cancellationToken)
    {
        var manufacturers = await _service.ManufacturerDropDown(cancellationToken);
        return View("New", manufacturers);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.GetById(id, CancellationToken.None);
        var manufacturers = await _service.ManufacturerDropDown(CancellationToken.None);

        var viewModel = new ManItemClassViewModel
        {
            ManItemClassId = model.ManufacturerId,
            ItemClassCode = model.ItemClassCode,
            ManufacturerId = model.ManufacturerId
        };

        viewModel.Manufacturers = manufacturers
            .Select(c => new SelectListItem
            {
                Value = c.ManufacturerId.ToString(),
                Text = c.Name
            })
            .ToList();

        return View("New", viewModel);   // ✔ same view
    }


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
            ManufacturerId = model.ManufacturerId,
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