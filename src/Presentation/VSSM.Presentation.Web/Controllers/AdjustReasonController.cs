using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.AdjustReason;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.AdjustReason;

namespace VSSM.Presentation.Web.Controllers;

public class AdjustreasonController : Controller
{
    private readonly IAdjustReasonService _adjustReasonService;

    public AdjustreasonController(IAdjustReasonService adjustReasonService)
    {
        _adjustReasonService = adjustReasonService;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sortField = "AdjustreasonId",
        string sortOrder = "asc")
    {
        var result = await _adjustReasonService.GetPagedList(
            search,
            page,
            pageSize,
            sortField,
            sortOrder,
            cancellationToken);

        return View(result);
    }

    public IActionResult New()
    {
        return View(new AdjustReasonViewModel());
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _adjustReasonService.GetById(id, cancellationToken);
        if (entity == null)
            return NotFound();

        var model = new AdjustReasonViewModel
        {
            AdjustReasonId = entity.AdjustReasonId,
            Code = entity.Code,
            FullName = entity.FullName,
            EntryDate = entity.EntryDate,
            IsActive = entity.IsActive,
            UsedForDispatchAdjust = entity.UsedForDispatchAdjust,
            UserForArrivalAdjust = entity.UserForArrivalAdjust,
            UsedForStockAdjust = entity.UsedForStockAdjust,
            IsWaste = entity.IsWaste,
            ApplicationForStockSubtraction = entity.ApplicationForStockSubtraction,
            ApplicationForStockAddition = entity.ApplicationForStockAddition
        };

        return View("New", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdjustReasonViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("New", model);
        }

        var dto = new AdjustreasonDto
        {
            AdjustReasonId = model.AdjustReasonId??0,
            Code = model.Code,
            FullName = model.FullName,
            IsActive = model.IsActive,
            UsedForDispatchAdjust = model.UsedForDispatchAdjust,
            UserForArrivalAdjust = model.UserForArrivalAdjust,
            UsedForStockAdjust = model.UsedForStockAdjust,
            IsWaste = model.IsWaste,
            ApplicationForStockSubtraction = model.ApplicationForStockSubtraction,
            ApplicationForStockAddition = model.ApplicationForStockAddition
        };

        if (model.AdjustReasonId == null || model.AdjustReasonId == 0)
        {
            await _adjustReasonService.Create(dto, cancellationToken);
        }
        else
        {
            await _adjustReasonService.Update(dto, cancellationToken);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _adjustReasonService.Delete(id, cancellationToken);

        if (deleted)
        {
            TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] = "مورد مورد نظر یافت نشد";
            TempData["ToastType"] = "error";
        }

        return RedirectToAction("Index");
    }
}