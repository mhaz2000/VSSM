using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.PowerSource;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Controllers;
using VSSM.Presentation.Web.Models.PowerSource;


public class PowerSourceController : BaseController
{
    private readonly IPowerSourceService _service;

    public PowerSourceController(IPowerSourceService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(
        CancellationToken token,
        string? search,
        int page = 1,
        int pageSize = 10,
        string sortField = "Code",
        string sortOrder = "asc")
    {
        var model = await _service.GetPagedList(search, page, pageSize, sortField, sortOrder, token);
        return View(model);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        PowerSourceViewModel model = new PowerSourceViewModel();

        if (id.HasValue)
        {
            var dto = await _service.GetById(id.Value, token);
            model = new PowerSourceViewModel
            {
                PowerSourceId = dto.PowerSourceId,
                Code = dto.Code,
                FullName = dto.FullName,
                IsActive = dto.IsActive
            };
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PowerSourceViewModel model, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        if (model.PowerSourceId == null || model.PowerSourceId == 0)
        {
            var dto = new PowerSourceCreateDto
            {
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive
            };

            await _service.Create(dto, token);
        }
        else
        {
            var dto = new PowerSourceEditDto
            {
                PowerSourceId = model.PowerSourceId.Value,
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive
            };

            await _service.Update(dto, token);
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken token)
    {
        var dto = await _service.GetById(id, token);
        var model = new PowerSourceViewModel
        {
            PowerSourceId = dto.PowerSourceId,
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        };

        return View("New", model);
    }
}
