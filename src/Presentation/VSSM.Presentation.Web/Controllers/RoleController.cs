using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.Role;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web.Models.ItemClass;
using VSSM.Presentation.Web.Models.Role;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IMenuService _menuService;
        public RoleController(IRoleService roleService, IMenuService menuService)
        {
            _roleService = roleService;
            _menuService = menuService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationTOken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _roleService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    cancellationTOken);

            return View(listModel);
        }

        public async Task<IActionResult> RolePermissions(int id, CancellationToken cancellationToken)
        {
            var model = new RolePermissionViewModel();
            if (id == null || id == 0)
            {
                var allMenus = await _menuService.GetMenus(cancellationToken);
                model.RoleId = 0;
                model.RoleName = "";
                model.Menus = allMenus.Select(m => new MenuCheckboxViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ParentId = m.ParentId,
                    IsSelected = false
                }).ToList();

            }
            else
            {
                var role = await _roleService.GetById(id, cancellationToken);
                if (role == null)
                    return NotFound();

                var allMenus = await _menuService.GetMenus(cancellationToken);

                var rolemenus = await _roleService.GetRoleMenus(id, cancellationToken);

                model.RoleId = id;
                model.RoleName = role.Name;
                model.Menus = allMenus.Select(m => new MenuCheckboxViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ParentId = m.ParentId,
                    IsSelected = rolemenus.Any(r => r.MenuId == m.Id)
                }).ToList();
            }

            return View(model);
        }

        public async Task<IActionResult> Create(RolePermissionViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("RolePermissions", model);
            }

            
            if (model.RoleId == null || model.RoleId == 0)
            {
                // ---------- Create ----------
                var roleDto = new RoleCreateDto { Name = model.RoleName };
                var roleId = await _roleService.Create(roleDto, cancellationToken);
                if (roleId > 0)
                {
                    var selectedMenus = model.Menus.Where(m => m.IsSelected)
                                                    .Select(m => new RoleMenuDto
                                                    {
                                                        RoleId = roleId,
                                                        MenuId = m.Id
                                                    }).ToList();
                    var result = await _roleService.AddRoleMenu(selectedMenus, cancellationToken);
                }
            }
            else
            {
                // ---------- Update ----------
                var roleDto = new RoleListDto { Name = model.RoleName, Id = model.RoleId };
                var selectedMenus = model.Menus.Where(m => m.IsSelected)
                                                    .Select(m => new RoleMenuDto
                                                    {
                                                        RoleId = model.RoleId,
                                                        MenuId = m.Id
                                                    }).ToList();
                var result = await _roleService.UpdateRoleMenus(roleDto, selectedMenus, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }


    }
}
