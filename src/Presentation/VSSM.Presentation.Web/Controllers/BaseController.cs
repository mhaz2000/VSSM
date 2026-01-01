using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VSSM.Presentation.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int? CurrentUserId { get; private set; }
        protected string? CurrentUserRole { get; private set; }
        protected string? CurrentUserFullName { get; private set; }
        protected int? CurrentUserRecipientId { get; private set; }
        protected int? CurrentUserLevel { get; private set; }

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                CurrentUserId = int.TryParse(
                    User.FindFirst("UserId")?.Value, out var id) ? id : null;

                CurrentUserRole = User.FindFirst("UserRole")?.Value;
                CurrentUserFullName = User.Identity?.Name;
                CurrentUserRecipientId = int.TryParse(
                    User.FindFirst("RecipientId")?.Value, out var rid) ? rid : null;
                CurrentUserLevel = int.TryParse(
                    User.FindFirst("Level")?.Value, out var l) ? l : null;

                ViewBag.CurrentUserName = CurrentUserFullName;
            }

            base.OnActionExecuting(context);
        }
    }
}
