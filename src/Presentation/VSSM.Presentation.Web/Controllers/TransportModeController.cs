using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.ReceivingMode;
using VSSM.Application.DTOs.BaseEntities.TransportMode;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class TransportModeController : Controller
{
    private readonly ITransportModeService _transportModeService;

    public TransportModeController(ITransportModeService transportModeService)
    {
        _transportModeService = transportModeService;
    }

    public async Task<IActionResult> Index(
                                            CancellationToken cancellationToken,
                                            string? search,
                                            int page = 1,
                                            int pageSize = 10,
                                            string sortField = "Orders",
                                            string sortOrder = "desc")
    {
        var transportModes = await _transportModeService.GetPagedListAsync(
                                                                search,
                                                                page,
                                                                pageSize,
                                                                sortField,
                                                                sortOrder,
                                                                cancellationToken);

        return View(transportModes);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTransportModeDto dto, CancellationToken cancellationToken)
    {
        await _transportModeService.CreateAsync(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var receivingMode = await _transportModeService.GetByIdAsync(id, cancellationToken);

        if (receivingMode == null)
        {
            TempData["ToastMessage"] = "روش‌دریافتی یافت نشد.";
            TempData["ToastType"] = "error";

            return RedirectToAction(nameof(Index));
        }

        var updateTransportModeDto = new UpdateTransportModeDto()
        {
            TransportModeId = receivingMode.TransportModeId,
            Code = receivingMode.Code,
            FullName = receivingMode.FullName,
            EntryDate = receivingMode.EntryDate,
            IsActive = receivingMode.IsActive
        };

        return View(updateTransportModeDto);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTransportModeDto dto, CancellationToken cancellationToken)
    {
        await _transportModeService.UpdateAsync(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }
}
