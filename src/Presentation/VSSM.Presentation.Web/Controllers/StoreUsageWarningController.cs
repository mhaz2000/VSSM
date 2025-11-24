using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.StoreUsageWarning;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class StoreUsageWarningController : Controller
{
    private readonly IStoreUsageWarningService _service;

    public StoreUsageWarningController(IStoreUsageWarningService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _service.GetAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(StoreUsageWarningDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _service.CreateOrUpdateAsync(model);
        ViewBag.Message = "تغییرات با موفقیت ذخیره شد.";
        return View(model);
    }
}
