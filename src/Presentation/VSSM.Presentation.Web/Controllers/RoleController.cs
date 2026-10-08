using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Role;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Role;

namespace VSSM.Presentation.Web.Controllers;

public class RoleController(
    IRoleService roleService,
    IMenuService menuService) : BaseController
{
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sort = "Name",
        string direction = "asc")
    {
        var paged = await roleService.GetPagedList(
            search, page, pageSize, sort, direction, cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = paged.Items.Cast<object>().ToList(),
                PageNumber = paged.Paging.Page,
                PageSize = pageSize,
                TotalCount = paged.Paging.PageSize
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "عنوان نقش", PropertyName = nameof(RoleListDto.Name), Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true }
            },
            Actions = new List<GridAction>
            {
                new()
                {
                    Title = "ویرایش",
                    Controller = "Role",
                    Action = "RolePermissions",
                    RouteIdProperty = nameof(RoleListDto.Id),
                    IconClass = "fa fa-edit"
                }
            }
        };

        return View(model);
    }

    public async Task<IActionResult> RolePermissions(int id, CancellationToken cancellationToken)
    {
        var model = new RolePermissionViewModel();
        if (id == null || id == 0)
        {
            var allMenus = await menuService.GetMenus(cancellationToken);
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
            var role = await roleService.GetById(id, cancellationToken);
            if (role == null)
                return NotFound();

            var allMenus = await menuService.GetMenus(cancellationToken);


            var rolemenus = await roleService.GetRoleMenus(id, cancellationToken);

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        RolePermissionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Forbid();

        if (!ModelState.IsValid)
            return View("RolePermissions", model);

        if (model.RoleId == null || model.RoleId == 0)
        {
            var roleDto = new RoleCreateDto
            {
                Name = model.RoleName
            };

            var roleId = await roleService.Create(roleDto, cancellationToken);

            if (roleId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.RoleName),
                    "نام نقش تکراری است.");

                return View("RolePermissions", model);
            }

            var selectedMenus = model.Menus
                .Where(m => m.IsSelected)
                .Select(m => new RoleMenuDto
                {
                    RoleId = roleId,
                    MenuId = m.Id
                })
                .ToList();

            await roleService.AddRoleMenu(selectedMenus, cancellationToken);
        }
        else
        {
            var roleDto = new RoleListDto
            {
                Name = model.RoleName,
                Id = model.RoleId.Value
            };

            var selectedMenus = model.Menus
                .Where(m => m.IsSelected)
                .Select(m => new RoleMenuDto
                {
                    RoleId = model.RoleId.Value,
                    MenuId = m.Id
                })
                .ToList();

            var result = await roleService.UpdateRoleMenus(
                roleDto,
                selectedMenus,
                cancellationToken);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.RoleName),
                    "نام نقش تکراری است یا نقش موردنظر یافت نشد.");

                return View("RolePermissions", model);
            }
        }

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    private bool IsAdmin() =>
        string.Equals(
        CurrentUserRole,
        "ادمین",
        StringComparison.OrdinalIgnoreCase);
}
