using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.ReceivingMode;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class ReceivingModeController : Controller
{
    private readonly IReceivingModeService _receivingModeService;

    public ReceivingModeController(IReceivingModeService receivingModeService)
    {
        _receivingModeService = receivingModeService;
    }

    public async Task<IActionResult> Index(
                                            CancellationToken cancellationToken,
                                            string? search,
                                            int page = 1,
                                            int pageSize = 10,
                                            string sortField = "Orders",
                                            string sortOrder = "desc")
    {
        var receivingModeas = await _receivingModeService.GetPagedListAsync(
                                                                search,
                                                                page,
                                                                pageSize,
                                                                sortField,
                                                                sortOrder,
                                                                cancellationToken);

        return View(receivingModeas);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReceivingModeDto dto, CancellationToken cancellationToken)
    {
        await _receivingModeService.CreateAsync(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var receivingMode = await _receivingModeService.GetByIdAsync(id, cancellationToken);

        if (receivingMode == null)
        {
            TempData["ToastMessage"] = "روش‌دریافتی یافت نشد.";
            TempData["ToastType"] = "error";

            return RedirectToAction(nameof(Index));
        }

        var updateReceivingModeDto = new UpdateReceivingModeDto()
        {
            ReceivingModeId = receivingMode.ReceivingModeId,
            Code = receivingMode.Code,
            FullName = receivingMode.FullName,
            //EntryDate = receivingMode.EntryDate,
            IsActive = receivingMode.IsActive
        };

        return View(updateReceivingModeDto);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateReceivingModeDto dto, CancellationToken cancellationToken)
    {
        await _receivingModeService.UpdateAsync(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }
}
