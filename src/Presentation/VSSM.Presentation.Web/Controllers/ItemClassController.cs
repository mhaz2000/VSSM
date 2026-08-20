using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers.BaseEntities;

public class ItemClassController(
    IBaseInfoService baseInfoService) : BaseController
{
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search,
        int page = 1,
        int pageSize = 10,
        string sortField = "Order",
        string sortOrder = "desc")
    {
        var result = await baseInfoService.GetPagedBaseInfoAsync(
            search,
            page,
            pageSize,
            cancellationToken);

        return View(result);
    }

    public IActionResult New()
    {
        return View();
    }

    public async Task<IActionResult> Create(
        BaseInfoInputDto model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        await baseInfoService.AddAsync(
            model,
            CurrentUserId.GetValueOrDefault());

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var itemClass = await baseInfoService.GetByIdAsync(id);

        if (itemClass is null)
            return NotFound();

        var model = new BaseInfoInputDto
        {
            BaseInfoId = itemClass.Id,
            Code = itemClass.Code,
            Title = itemClass.Title,
            ParentId = itemClass.ParentId,
            Order = itemClass.Order
        };

        return View("New", model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(
        BaseInfoInputDto model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        await baseInfoService.UpdateAsync(
            model,
            CurrentUserId.GetValueOrDefault());

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await baseInfoService.RemoveAsync(id);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }
}