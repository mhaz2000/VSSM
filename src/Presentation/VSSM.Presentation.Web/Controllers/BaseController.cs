using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VSSM.Presentation.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int? CurrentUserId { get; private set; }
        protected string? CurrentUserType { get; private set; }
        protected string? CurrentUserFullName { get; private set; }

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                CurrentUserId = int.TryParse(
                    User.FindFirst("UserId")?.Value, out var id) ? id : null;

                CurrentUserType = User.FindFirst("UserType")?.Value;
                CurrentUserFullName = User.Identity?.Name;
            }

            base.OnActionExecuting(context);
        }
    }
}
