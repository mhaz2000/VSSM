using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.DispatchRepConfig;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

[Authorize]
public class DispatchRepConfigController : BaseController
{
    private readonly IDispatchRepConfigService _service;

    public DispatchRepConfigController(IDispatchRepConfigService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _service.GetAsync();
        if (model == null)
            model = new DispatchRepConfigDto(); // No record exists yet

        return View("Edit", model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(DispatchRepConfigDto model)
    {
        if (!ModelState.IsValid)
            return View("Edit", model);

        await _service.CreateOrUpdateAsync(model);

        TempData["Success"] = "اطلاعات با موفقیت ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }
}