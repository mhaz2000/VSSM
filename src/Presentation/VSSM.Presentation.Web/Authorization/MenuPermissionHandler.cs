using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using VSSM.Application.Common.Authorization;
using VSSM.Application.Common.Interfaces;
using VSSM.Application.Interfaces;

namespace VSSM.Presentation.Web.Authorization
{
    public class MenuPermissionHandler : AuthorizationHandler<MenuPermissionRequirement>
    {
        private readonly IMenuPermissionService _permissionService;
        private readonly IAuditService _audit;

        public MenuPermissionHandler(
            IMenuPermissionService permissionService,
            IAuditService audit)
        {
            _permissionService = permissionService;
            _audit = audit;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MenuPermissionRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
                return;

            var roleIdClaim = context.User.FindFirst("RoleId");
            if (roleIdClaim == null)
                return;

            var roleId = int.Parse(roleIdClaim.Value);

            var httpContext = context.Resource switch
            {
                HttpContext c => c,
                AuthorizationFilterContext mvc => mvc.HttpContext,
                _ => null
            };

            if (httpContext == null)
                return;

            var routeData = httpContext.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            //var action = routeData.Values["action"]?.ToString();

            var hasAccess = await _permissionService
                .HasAccessAsync(roleId, controller);

            if (hasAccess)
                context.Succeed(requirement);
            else
            {
                // [AUDIT - FAU_GEN]: record authorization failures (tamper/enumeration signal).
                try
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                    await _audit.LogAsync("Warning", $"Access denied: controller={controller}",
                        context.User.Identity?.Name, ip, httpContext.Request.Path, httpContext.Request.Method);
                }
                catch
                {
                    // audit logging must never break the authorization decision
                }
            }
        }
    }

}
