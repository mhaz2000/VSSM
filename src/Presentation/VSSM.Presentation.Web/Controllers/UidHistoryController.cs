using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.UidHistory;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

public class UidHistoryController(IUidHistoryService historyService) : BaseController
{
    public IActionResult Index()
    {
        return View(new UidHistorySearchDto());
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] UidHistorySearchDto model, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(model.Uid))
            return Json(new { success = false, message = "UID اجباری است" });

        var (success, message, data) = await historyService
            .GetHistoryByUidAsync(model.Uid, CurrentUserRecipientId ?? 0, token);

        if (!success)
            return Json(new { success, message });

        var tableRows = data.Rows.Select(r => new
        {
            rowNumber = r.RowNumber,
            eventTitle = r.EventTitle,
            eventDate = r.EventDate,
            description = r.Description
        }).ToList();

        return Json(new { success = true, data = tableRows });
    }

    [HttpGet]
    public async Task<IActionResult> Print(
    string uid,
    CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(uid))
            return BadRequest("UID اجباری است.");

        var (success, message, data) = await historyService
            .GetHistoryByUidAsync(
                uid,
                CurrentUserRecipientId ?? 0,
                token);

        if (!success)
            return BadRequest(message);

        var model = new UidHistoryPrintViewModel
        {
            Uid = uid,
            PrintDate = DateTime.Now,
            Rows = data.Rows
        };

        return View(model);
    }
}