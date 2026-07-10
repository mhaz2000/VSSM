using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.DestroyUid;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

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
            .SearchByBatchAsync(batchNumber, CurrentUserRecipientId.GetValueOrDefault(), token);

        return Json(new { success, message, data });
    }

    [HttpPost]
    public async Task<IActionResult> SearchByUid(string uid, CancellationToken token)
    {
        var (success, message, data) = await destroyUidService
            .SearchByUidAsync(uid, CurrentUserRecipientId.GetValueOrDefault(), token);

        return Json(new { success, message, data });
    }
    [HttpPost]
    public async Task SubmitDestroyProgress(
        [FromBody] List<DestroyUidSubmitDto> items, CancellationToken token)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        async Task Send(string eventName, object data)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
            await Response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", token);
            await Response.Body.FlushAsync(token);
        }

        // شمارش کل UIDs قبل از شروع — تا progress bar بدونه total چقدره
        var total = await destroyUidService
            .CountDestroyableUidsAsync(items, CurrentUserRecipientId ?? 0, token);

        await Send("start", new { total });

        var (success, message) = await destroyUidService.SubmitDestroyAsync(
            items,
            CurrentUserId ?? 0,
            CurrentUserRecipientId ?? 0,
            token,
            onProgress: async (done) =>
                await Send("progress", new { done, total })
        );

        await Send("done", new { success, message });
    }
}