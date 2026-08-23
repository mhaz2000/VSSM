using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace VSSM.Presentation.Web.Filters;

/// <summary>
/// [SECURITY - A07/FIA_PMG]: forces any authenticated user carrying the "MustChangePassword"
/// claim to change their password before using the application. Applied globally; exempts the
/// forced-change endpoint itself plus login/logout so the flow can complete.
/// </summary>
public class ForcePasswordChangeFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true && user.HasClaim("MustChangePassword", "true"))
        {
            var controller = (string?)context.RouteData.Values["controller"];
            var action = (string?)context.RouteData.Values["action"];

            var isExempt = string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase) &&
                           (string.Equals(action, "ForceChangePassword", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase));

            if (!isExempt)
            {
                context.Result = new RedirectToActionResult("ForceChangePassword", "Account", null);
                return;
            }
        }

        await next();
    }
}
