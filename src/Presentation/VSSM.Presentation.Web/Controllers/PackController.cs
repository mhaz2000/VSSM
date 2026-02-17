using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Pack;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Controllers;
using VSSM.Presentation.Web.Models.Pack;


public class PackController : BaseController
{
    private readonly IPackService _packService;

    public PackController(IPackService packService)
    {
        _packService = packService;
    }

    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        int pageSize = 10,
        string sortField = "Code",
        string sortOrder = "asc",
        CancellationToken token = default)
    {
        var list = await _packService.GetPagedList(
            search, page, pageSize, sortField, sortOrder, token);

        return View(list);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        if (id == null)
            return View(new PackViewModel());

        var dto = await _packService.GetById(id.Value, token);
        if (dto == null) return NotFound();

        return View(new PackViewModel
        {
            PackId = dto.PackId,
            Code = dto.Code,
            FullName = dto.FullName,
            NetVolume = dto.NetVolume,
            ExternalSize = dto.ExternalSize,
            PackWeight = dto.PackWeight,
            IsActive = dto.IsActive,
            UseForVacDL = dto.UseForVacDL,
            UseForOS = dto.UseForOS
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(PackViewModel model, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        if (model.PackId == null)
        {
            var dto = new PackCreateDto
            {
                Code = model.Code,
                FullName = model.FullName,
                NetVolume = model.NetVolume,
                ExternalSize = model.ExternalSize,
                PackWeight = model.PackWeight,
                IsActive = model.IsActive,
                UseForVacDL = model.UseForVacDL,
                UseForOS = model.UseForOS
            };

            await _packService.Create(dto, token);
        }
        else
        {
            var dto = new PackEditDto
            {
                PackId = model.PackId.Value,
                Code = model.Code,
                FullName = model.FullName,
                NetVolume = model.NetVolume,
                ExternalSize = model.ExternalSize,
                PackWeight = model.PackWeight,
                IsActive = model.IsActive,
                UseForVacDL = model.UseForVacDL,
                UseForOS = model.UseForOS
            };

            await _packService.Update(dto, token);
        }

        TempData["ToastMessage"] = "با موفقیت ذخیره شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }
}
