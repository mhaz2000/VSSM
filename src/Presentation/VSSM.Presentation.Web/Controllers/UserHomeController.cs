using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.UserHome;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class UserHomeController : Controller
{
    private readonly IUserHomeService _service;

    public UserHomeController(IUserHomeService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _service.GetAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(UserHomeDto dto)
    {
        if (!ModelState.IsValid)
            return View("Index", dto);

        await _service.CreateOrUpdateAsync(dto);

        TempData["Success"] = "تنظیمات با موفقیت ذخیره شد";
        return RedirectToAction("Index");
    }
}
