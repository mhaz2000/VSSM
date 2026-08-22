using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.UserManagement;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController(IUserService userService, IRecipientService recipientService) : BaseController
{
    public IActionResult ManageRecipient()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var result = await userService.SearchUsersWithRecipientAsync(searchTerm, pageNumber, pageSize);
            return Json(new { success = true, data = result.Items, totalCount = result.TotalCount });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchRecipients(string searchTerm)
    {
        try
        {
            var recipients = await recipientService.SearchActiveRecipientsAsync(searchTerm);
            return Json(new { success = true, data = recipients });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRecipient([FromBody] ChangeRecipientDto model)
    {
        try
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات نامعتبر" });

            if (!User.IsInRole("Admin"))
                return Json(new { success = false, message = "شما دسترسی لازم را ندارید" });

            var result = await userService.ChangeRecipientAsync(model.UserId, model.NewRecipientId);
            return Json(new { success = true, message = "محل خدمت با موفقیت تغییر یافت" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
