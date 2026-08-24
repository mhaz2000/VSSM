using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.VoucherConfig;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class VoucherConfigController(
    IVoucherConfigService service,
    ILogger<VoucherConfigController> logger) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var model = await service.GetAsync() ?? new VoucherConfigDto();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VoucherConfigDto model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("ModelState is invalid for VoucherConfig");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                logger.LogWarning($"Validation error: {error.ErrorMessage}");
            }
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await service.AddOrUpdateAsync(model);
            TempData["SuccessMessage"] = "تنظیمات با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving VoucherConfig");
            ModelState.AddModelError("", "خطا در ذخیره‌سازی تنظیمات. لطفاً دوباره تلاش کنید.");
            return View(model);
        }
    }
}