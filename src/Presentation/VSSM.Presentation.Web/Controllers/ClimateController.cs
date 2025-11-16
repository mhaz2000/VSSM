using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Climate;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class ClimateController : Controller
{
    private readonly IClimateService _climateService;

    public ClimateController(IClimateService climateService)
    {
        _climateService = climateService;
    }

    public async Task<IActionResult> Index(
                                            CancellationToken cancellationToken,
                                            string? search,
                                            int page = 1,
                                            int pageSize = 10,
                                            string sortField = "Orders",
                                            string sortOrder = "desc")
    {
        var climates = await _climateService.GetPagedListAsync(
                                                                search,
                                                                page,
                                                                pageSize,
                                                                sortField,
                                                                sortOrder,
                                                                cancellationToken);

        return View(climates);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClimateDto dto, CancellationToken cancellationToken)
    {
        await _climateService.CreateAsync(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var climate = await _climateService.GetByIdAsync(id, cancellationToken);

        if (climate == null)
        {
            TempData["ToastMessage"] = "روش‌دریافتی یافت نشد.";
            TempData["ToastType"] = "error";

            return RedirectToAction(nameof(Index));
        }

        var updateClimateDto = new UpdateClimateDto()
        {
            ClimateId = climate.ClimateId,
            Code = climate.Code,
            FullName = climate.FullName,
            IsActive = climate.IsActive
        };

        return View(updateClimateDto);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateClimateDto dto, CancellationToken cancellationToken)
    {
        await _climateService.UpdateAsync(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }
}
