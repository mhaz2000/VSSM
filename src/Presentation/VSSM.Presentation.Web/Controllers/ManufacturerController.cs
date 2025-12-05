using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        return View(manufacturers); 
    }

    public IActionResult New()
    {
        var itemClasses = _manufacturerService.ManufacturerDropDown(null, CancellationToken.None).Result;
        var model = new ManufacturerViewModel();
        
        model.Items.AddRange(itemClasses.Select(c=>new SelectListItem()
        {
            Value = c.ItemsClassId.ToString(),
            Text = c.Name
        }));

        return View(model);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        // گرفتن تولیدکننده از سرویس
        var manufacturer = await _manufacturerService.GetById(id, cancellationToken);
        if (manufacturer == null)
            return NotFound();

        // گرفتن لیست ItemClass‌ها برای نمایش در فرم
        var itemClasses = await _manufacturerService.ManufacturerDropDown(manufacturer.ManufacturerId,cancellationToken);

        // ساخت مدل ویو
        var model = new ManufacturerViewModel
        {
            ManufacturerId = manufacturer.ManufacturerId,
            Code = manufacturer.Code,
            FullName = manufacturer.FullName,
            ShowForVac = manufacturer.ShowForVac,
            ShowForOther = manufacturer.ShowForOther,
            IsActive = manufacturer.IsActive,
        };

        // افزودن ItemClass‌ها به مدل ویو
        model.Items.AddRange(itemClasses.Select(c => new SelectListItem
        {
            Value = c.ItemsClassId.ToString(),
            Text = c.Name
        }));

        // مقداردهی آیتم‌های انتخاب‌شده برای checkboxها
        model.SelectedItemClassIds = itemClasses.Where(c => c.Selected).Select(c => c.ItemsClassId).ToList();

        return View("New", model);
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
            ShowForVac = model.ShowForVac,
            ShowForOther = model.ShowForOther,
            IsActive = model.IsActive,
            ItemClasses = model.SelectedItemClassIds.ToList()
            
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

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد"; 
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
            TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد"; 
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] =
                "تولید کننده مورد نظر یافت نشد"; 
            TempData["ToastType"] = "error";
        }

        return RedirectToAction("Index");
    }
}