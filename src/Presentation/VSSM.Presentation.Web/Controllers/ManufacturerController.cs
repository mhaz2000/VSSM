using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Manufacturer;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Manufacturer;

namespace VSSM.Presentation.Web.Controllers;

public class ManufacturerController : Controller
{
    private readonly IManufacturerService _manufacturerService;

    public ManufacturerController(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sortField = "ManufacturerId",
        string sortOrder = "asc")
    {
        var manufacturers = await _manufacturerService.GetPagedList(
            search,
            page,
            pageSize,
            sortField,
            sortOrder,
            cancellationToken);

        return View(manufacturers); // View file: /Views/Manufacturer/Index.cshtml
    }

    public IActionResult New()
    {
        return View(new ManufacturerViewModel()); // View file: /Views/Manufacturer/New.cshtml
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var manufacturer = await _manufacturerService.GetById(id, cancellationToken);
        if (manufacturer == null)
            return NotFound();

        var model = new ManufacturerViewModel
        {
            ManufacturerId = manufacturer.ManufacturerId,
            Code = manufacturer.Code,
            FullName = manufacturer.FullName,
            EntryDate = manufacturer.EntryDate,
            ShowForVac = manufacturer.ShowForVac,
            ShowForOther = manufacturer.ShowForOther,
            IsActive = manufacturer.IsActive
        };

        return View("New", model); // Reuse the New view for editing
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManufacturerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("New", model);
        }

        var dto = new ManufacturerDto
        {
            ManufacturerId = model.ManufacturerId.GetValueOrDefault(),
            Code = model.Code,
            FullName = model.FullName,
            EntryDate = model.EntryDate,
            ShowForVac = model.ShowForVac,
            ShowForOther = model.ShowForOther,
            IsActive = model.IsActive
        };

        if (model.ManufacturerId == null || model.ManufacturerId == 0)
        {
            // Create Operation
            await _manufacturerService.Create(dto, cancellationToken);
        }
        else
        {
            // Update Operation
            await _manufacturerService.Update(dto, cancellationToken);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد"; // Operation successfully completed (Persian)
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _manufacturerService.Delete(id, cancellationToken);

        if (deleted)
        {
            TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد"; // Deletion successfully completed (Persian)
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] =
                "تولید کننده مورد نظر یافت نشد"; // The requested manufacturer was not found (Persian)
            TempData["ToastType"] = "error";
        }

        return RedirectToAction("Index");
    }
}