using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Source;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Controllers;
using VSSM.Presentation.Web.Models.Source;


public class SourceController : BaseController
{
    private readonly ISourceService _service;

    public SourceController(ISourceService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(
        CancellationToken token,
        string? search,
        int page = 1,
        int pageSize = 10,
        string sortField = "FullName",
        string sortOrder = "asc")
    {
        var result = await _service.GetPagedList(search, page, pageSize, sortField, sortOrder, token);
        return View(result);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        SourceViewModel model = new SourceViewModel();

        if (id.HasValue && id.Value > 0)
        {
            var dto = await _service.GetById(id.Value, token);
            model.SourceId = dto.SourceId;
            model.Code = dto.Code;
            model.FullName = dto.FullName;
            model.IsActive = dto.IsActive;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SourceViewModel model, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        if (model.SourceId == null || model.SourceId == 0)
        {
            var dto = new SourceCreateDto
            {
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive
            };

            await _service.Create(dto, token);
        }
        else
        {
            var dto = new SourceEditDto
            {
                SourceId = model.SourceId.Value,
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

        var vm = new SourceViewModel
        {
            SourceId = dto.SourceId,
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        };

        return View("New", vm);
    }
}
