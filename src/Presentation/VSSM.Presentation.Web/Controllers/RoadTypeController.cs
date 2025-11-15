using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.RoadType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.RoadType;

public class RoadTypeController : Controller
{
    private readonly IRoadTypeService _service;

    public RoadTypeController(IRoadTypeService service)
    {
        _service = service;
    }


    // -------------------------------------------------------
    // ثبت (CREATE PAGE)
    // -------------------------------------------------------
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RoadTypeCreateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _service.Create(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Management");
    }

    // -------------------------------------------------------
    // ویرایش (EDIT PAGE)
    // -------------------------------------------------------
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var dto = await _service.GetById(id, cancellationToken);

        var vm = new RoadTypeEditDto
        {
            RoadTypeId = dto.RoadTypeId,
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        };

        return View(vm);
    }

    public async Task<IActionResult> Management(string? search, int? id, CancellationToken token)
    {
        var vm = new RoadTypeManagementViewModel();

        // مقدار سرچ
        vm.Search = search;

        // دریافت همه رکوردها
        var data = await _service.GetAll(token);

        // اگر سرچ زده باشد
        if (!string.IsNullOrEmpty(search))
            data = data.Where(x => x.FullName.Contains(search) || x.Code.Contains(search)).ToList();

        vm.Items = data;

        // اگر id ارسال شده باشد → یعنی نمایش/ویرایش
        if (id != null)
            vm.EditItem = await _service.GetById(id.Value, token);

        return View(vm);
    }


    [HttpPost]
    public async Task<IActionResult> Update(RoadTypeEditDto dto, CancellationToken token)
    {
        await _service.Update(dto, token);

        TempData["ToastMessage"] = "ویرایش با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("Management");
    }

}
