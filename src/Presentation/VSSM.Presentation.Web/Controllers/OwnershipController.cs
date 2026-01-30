using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Presentation.Web.Controllers;

[Authorize]
public class OwnershipController : BaseController
{
    private readonly IOwnershipService _service;

    public OwnershipController(IOwnershipService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string? search,
        int page = 1, int pageSize = 10,
        string sortField = "Code",
        string sortOrder = "asc",
        CancellationToken token = default)
    {
        var model = await _service.GetPagedList(search, page, pageSize, sortField, sortOrder, token);
        return View(model);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        if (id == null)
            return View(new OwnershipViewModel());

        var dto = await _service.GetById(id.Value, token);

        return View(new OwnershipViewModel
        {
            OwnershipId = dto.OwnershipId,
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(OwnershipViewModel model, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return View("New", model);

        if (model.OwnershipId == null || model.OwnershipId == 0)
        {
            var dto = new OwnershipCreateDto
            {
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive
            };
            await _service.Create(dto, token);
        }
        else
        {
            var dto = new OwnershipEditDto
            {
                OwnershipId = model.OwnershipId.Value,
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

        return View("New", new OwnershipViewModel
        {
            OwnershipId = dto.OwnershipId,
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        });
    }
}
