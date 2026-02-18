using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Adjustments;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Presentation.Web.Models.Adjustment;

namespace VSSM.Presentation.Web.Controllers;


public class AdjustmentController : BaseController
{
    private readonly IAdjustmentService _adjustmentService;
    private readonly IItemClassService _itemClassService;
    private readonly IItemService _itemService;
    private readonly IArrivalService _arrivalService;
    private readonly IAdjustReasonService _adjustReasonService;
    private readonly IItemDetailStoreService _detailStoreService;

    public AdjustmentController(IAdjustmentService adjustmentService, IItemClassService itemClassService,
        IItemService itemService, IArrivalService arrivalService, IAdjustReasonService adjustReasonService, IItemDetailStoreService detailStoreService)
    {
        _adjustmentService = adjustmentService;
        _itemClassService = itemClassService;
        _itemService = itemService;
        _arrivalService = arrivalService;
        _adjustReasonService = adjustReasonService;
        _detailStoreService = detailStoreService;
    }

    // ---------- Index ----------
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sortField = "AdjustmentId",
        string sortOrder = "asc")
    {
        var adjustments = await _adjustmentService.GetPagedList(
            search,
            page,
            pageSize,
            sortField,
            sortOrder,
            CurrentUserId.Value,
            cancellationToken);

        return View(adjustments);
    }

    // ---------- New ----------
    public async Task<IActionResult> New(CancellationToken cancellationToken)
    {
        await PrepareDropdown(cancellationToken);

        var model = new AdjustmentViewModel
        {
            OldQuantity = 0, // مقدار اولیه برای Create
            NewQuantity = 0
        };

        return View(model);
    }

    // ---------- Edit ----------
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var adjustment = await _adjustmentService.GetById(id, cancellationToken);
        if (adjustment == null)
            return NotFound();

        await PrepareDropdown(cancellationToken);

        var itemDetailStores = await _detailStoreService.GetById(adjustment.ItemDetailStoreId ?? -1);
        var arrivalLine =
            await _arrivalService.GetArrivalLineByStoreIdAndItemDetailId((long)itemDetailStores.StoreId.Value,
                (long)itemDetailStores.ItemDetailId);
        var item = await _itemService.GetById(arrivalLine.ItemId.Value, cancellationToken);
        var model = new AdjustmentViewModel
        {
            AdjustmentId = adjustment.AdjustmentId,
            Date = adjustment.Date,
            Note = adjustment.Note,
            OldQuantity = adjustment.NewQuantity,
            NewQuantity = 0,
            AdjustmentReasonId = adjustment.AdjustmentreasonId,
            ItemDetailStoreId = adjustment.ItemDetailStoreId,
            ItemId = arrivalLine.ItemId,
            ArrivalLineId = arrivalLine.ArrivalId,
            ItemClassId = item.ItemClassId
        };

        return View("New", model);
    }

    private async Task PrepareDropdown(CancellationToken cancellationToken)
    {
        var list = await _itemClassService.GetActives(cancellationToken);
        ViewBag.ItemClass = list;
        var itemList = await _itemService.GetActives(cancellationToken);
        ViewBag.ItemGroups = itemList;
        var itemDetailStores = await _detailStoreService.GetActives(CurrentUserRecipientId.Value);
        ViewBag.ItemDetailStores = itemDetailStores;
        var adjustReasons = await _adjustReasonService.GetActives(cancellationToken);
        ViewBag.AdjustReasons = adjustReasons;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdjustmentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("New", model);
        }

        var dto = new AdjustmentDto
        {
            Date = DateTime.Now,
            Note = model.Note,
            AdjustmentreasonId = model.AdjustmentReasonId,
            NewQuantity = model.NewQuantity,
            OldQuantity = model.OldQuantity,
            ArrivalLineId = model.ArrivalLineId,
            DispatchLineId = model.DispatchLineId,
            EnteredById = CurrentUserId,
            FiscalPeriodId = model.FiscalPeriodId,
            ItemDetailStoreId = model.ItemDetailStoreId
        };

        if (model.AdjustmentId == null || model.AdjustmentId == 0)
        {
            // Create
            await _adjustmentService.Create(dto, cancellationToken);
        }
        else
        {
            // Update
            await _adjustmentService.Update(dto, cancellationToken);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    // ---------- Delete ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _adjustmentService.Delete(id, cancellationToken);

        if (deleted)
        {
            TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] = "رکورد مورد نظر یافت نشد";
            TempData["ToastType"] = "error";
        }

        return RedirectToAction("Index");
    }
}