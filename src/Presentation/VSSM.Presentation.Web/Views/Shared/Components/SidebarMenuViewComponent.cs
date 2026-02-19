using Microsoft.AspNetCore.Mvc;
using System.Threading;
using VSSM.Application.DTOs.BaseEntities.Menu;
using VSSM.Application.DTOs.BaseEntities.Role;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Views.Shared.Components;

public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IRoleService _roleService;
    private readonly IMenuService _menuService;
    public SidebarMenuViewComponent(IRoleService roleService, IMenuService menuService)
    {
        _roleService = roleService;
        _menuService = menuService;
    }
    public IViewComponentResult Invoke()
    {
        var allMenus = _menuService.GetMenus(CancellationToken.None).Result;
        var roleId = int.Parse(UserClaimsPrincipal.FindFirst("RoleId").Value);
        var roleMenus = _roleService.GetRoleMenus(roleId, CancellationToken.None).Result;
        var allowedIds = roleMenus.Where(rm => rm.RoleId == roleId).Select(rm => rm.MenuId).ToList();
        var finalMenus = new List<MenuListDto>();
        foreach (var menuId in allowedIds)
        {
            AddWithParents(menuId, allMenus, finalMenus);
        }

        finalMenus = finalMenus.DistinctBy(m => m.Id).OrderBy(r => r.MenuOrder).ToList();
        var menu = BuildTree(finalMenus, null);
        //var menu = MenuConfig.GetMenu();
        return View(menu);
    }

    private void AddWithParents(int menuId, List<MenuListDto> allMenus, List<MenuListDto> result)
    {
        var menu = allMenus.FirstOrDefault(m => m.Id == menuId);
        if (menu == null) return;
        if (!result.Any(r => r.Id == menu.Id))
        {
            result.Add(menu);
        }
        if (menu.ParentId.HasValue)
        {
            AddWithParents(menu.ParentId.Value, allMenus, result);
        }
    }

    private List<MenuItem> BuildTree(
    List<MenuListDto> menus,
    int? parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .Select(m => new MenuItem
            {
                Id = m.Id,
                Title = m.Title,
                Controller = m.Controller,
                Action = m.Action,
                Icon = m.Icon,
                Children = BuildTree(menus, m.Id)
            })
            .ToList();
    }
}
