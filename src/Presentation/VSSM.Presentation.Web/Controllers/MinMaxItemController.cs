using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.MinMaxItem;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.MinMaxItem;

namespace VSSM.Presentation.Web.Controllers;

public class MinMaxItemController(
    IMinMaxItemService service,
    IItemService itemService,
    IRecipientService recipientService) : BaseController
{
    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        int pageSize = 10,
        string sortField = "Code",
        string sortOrder = "asc",
        CancellationToken token = default)
    {
        var list = await service.GetPagedList(
            search, page, pageSize, sortField, sortOrder, token);

        return View(list);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        await LoadDropDowns(token);

        if (id == null)
            return View(new MinMaxItemViewModel());

        var dto = await service.GetById(id.Value, token);
        if (dto == null) return NotFound();

        var model = new MinMaxItemViewModel
        {
            MinMaxItemId = dto.MinMaxItemId,
            ItemId = dto.ItemId,
            RecipientId = dto.RecipientId,
            StockMin = dto.StockMin,
            StockMax = dto.StockMax,
            Min = dto.Min,
            Max = dto.Max,
            AnnualNeeds = dto.AnnualNeeds,

            NumberShipment = dto.NumberShipment,
            PercentSafety = dto.PercentSafety,
            LeadConsumption = dto.LeadConsumption,
            TotalPop = dto.TotalPop,
            PercentUnder = dto.PercentUnder,
            NumDose = dto.NumDose,
            Wastage = dto.Wastage,
            PercentCoverage = dto.PercentCoverage,
            OrderLevel = dto.OrderLevel
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MinMaxItemViewModel model, CancellationToken token)
    {
        await LoadDropDowns(token);

        if (!ModelState.IsValid)
            return View("New", model);

        if (model.MinMaxItemId == null || model.MinMaxItemId == 0)
        {
            var dto = new MinMaxItemCreateDto
            {
                ItemId = model.ItemId,
                RecipientId = model.RecipientId,
                StockMin = model.StockMin,
                StockMax = model.StockMax,
                Min = model.Min,
                Max = model.Max,
                AnnualNeeds = model.AnnualNeeds,

                NumberShipment = model.NumberShipment,
                PercentSafety = model.PercentSafety,
                LeadConsumption = model.LeadConsumption,
                TotalPop = model.TotalPop,
                PercentUnder = model.PercentUnder,
                NumDose = model.NumDose,
                Wastage = model.Wastage,
                PercentCoverage = model.PercentCoverage,
                OrderLevel = model.OrderLevel
            };

            await service.Create(dto, token);
        }
        else
        {
            var dto = new MinMaxItemEditDto
            {
                MinMaxItemId = model.MinMaxItemId.Value,
                ItemId = model.ItemId,
                RecipientId = model.RecipientId,
                StockMin = model.StockMin,
                StockMax = model.StockMax,
                Min = model.Min,
                Max = model.Max,
                AnnualNeeds = model.AnnualNeeds,

                NumberShipment = model.NumberShipment,
                PercentSafety = model.PercentSafety,
                LeadConsumption = model.LeadConsumption,
                TotalPop = model.TotalPop,
                PercentUnder = model.PercentUnder,
                NumDose = model.NumDose,
                Wastage = model.Wastage,
                PercentCoverage = model.PercentCoverage,
                OrderLevel = model.OrderLevel
            };

            await service.Update(dto, token);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken token)
    {
        return await New(id, token);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken token)
    {
        var deleted = await service.DeleteAsync(id, token);

        if (!deleted)
        {
            TempData["ToastMessage"] = "رکورد موردنظر یافت نشد";
            TempData["ToastType"] = "error";

            return RedirectToAction(nameof(Index));
        }

        TempData["ToastMessage"] = "با موفقیت حذف شد";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropDowns(CancellationToken token)
    {
        ViewBag.Items = (await itemService.GetActives(token))
            .Select(x => new SelectListItem
            {
                Value = x.ItemId.ToString(),
                Text = x.FullName
            })
            .ToList();

        ViewBag.RecipientList = (await recipientService.GetAll(token))
            .Select(x => new SelectListItem
            {
                Value = x.RecipientId.ToString(),
                Text = x.FullName
            })
            .ToList();
    }
}
