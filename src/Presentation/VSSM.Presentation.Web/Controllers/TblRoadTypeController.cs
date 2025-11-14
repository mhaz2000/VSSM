using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.TblRoadType;
using VSSM.Application.Interfaces.BaseEntities;

public class TblRoadTypeController : Controller
{
    private readonly ITblRoadTypeService _service;

    public TblRoadTypeController(ITblRoadTypeService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _service.GetAll();
        return View(list);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(TblRoadTypeCreateDto dto)
    {
        await _service.Create(dto);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _service.GetById(id);
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TblRoadTypeEditDto dto)
    {
        await _service.Edit(dto);
        return RedirectToAction("Index");
    }
}
