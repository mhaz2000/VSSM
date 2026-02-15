using Microsoft.AspNetCore.Mvc;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Views.Shared.Components;

public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IRoleService _roleService;
    public SidebarMenuViewComponent(IRoleService roleService)
    {
        _roleService = roleService;
    }
    public IViewComponentResult Invoke()
    {
        //var roleId = int.Parse(UserClaimsPrincipal.FindFirst("RoleId").Value);
        //var menus = _roleService.GetRoleMenus(roleId, CancellationToken.None).Result;
        var menu = MenuConfig.GetMenu();
        return View(menu);
    }
}
