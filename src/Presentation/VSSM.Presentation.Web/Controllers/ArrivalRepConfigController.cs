using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.ArrivalRepConfig;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

[Authorize]
public class ArrivalRepConfigController : Controller
{
    private readonly IArrivalRepConfigService _arrivalRepConfigService;

    public ArrivalRepConfigController(IArrivalRepConfigService arrivalRepConfigService)
    {
        _arrivalRepConfigService = arrivalRepConfigService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _arrivalRepConfigService.GetAsync() ?? new ArrivalRepConfigDto();
        return View("New", model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Save(ArrivalRepConfigDto model)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        await _arrivalRepConfigService.SaveAsync(model);

        TempData["Success"] = "تنظیمات با موفقیت ذخیره شد";
        return RedirectToAction(nameof(Index));
    }
}
