using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.DestroyUid;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class DestroyUidController(
    IDestroyUidService destroyUidService) : BaseController
{
    public async Task<IActionResult> Index(CancellationToken token)
    {
        var reasons = await destroyUidService.GetDestroyReasonsAsync(token);

        ViewBag.ReasonList = reasons
            .Select(r => new { id = r.Id, title = r.Title })
            .ToList();

        ViewBag.TodayShamsi = DateTime.Now.ToShamsi(); 

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SearchByBatch(string batchNumber, CancellationToken token)
    {
        var (success, message, data) = await destroyUidService
            .SearchByBatchAsync(batchNumber, CurrentUserRecipientId ?? 0, token);

        return Json(new { success, message, data });
    }

    [HttpPost]
    public async Task<IActionResult> SearchByUid(string uid, CancellationToken token)
    {
        var (success, message, data) = await destroyUidService
            .SearchByUidAsync(uid, CurrentUserRecipientId ?? 0, token);

        return Json(new { success, message, data });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitDestroy(
        [FromBody] List<DestroyUidSubmitDto> items, CancellationToken token)
    {
        var (success, message) = await destroyUidService
            .SubmitDestroyAsync(items, CurrentUserId ?? 0, CurrentUserRecipientId ?? 0, token);

        return Json(new { success, message });
    }
}