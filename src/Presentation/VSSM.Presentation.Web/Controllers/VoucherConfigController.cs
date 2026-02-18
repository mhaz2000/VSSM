using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.VoucherConfig;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;


public class VoucherConfigController : BaseController
{
    private readonly IVoucherConfigService _service;

    public VoucherConfigController(IVoucherConfigService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _service.GetAsync() ?? new VoucherConfigDto();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VoucherConfigDto model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        await _service.AddOrUpdateAsync(model);
        TempData["SuccessMessage"] = "تنظیمات با موفقیت ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }
}
