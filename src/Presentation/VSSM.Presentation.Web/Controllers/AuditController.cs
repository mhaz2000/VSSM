using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using VSSM.Application.Interfaces;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

// [SECURITY - A01/FAU_SAR]: audit review is restricted to authenticated Admins (not the
// menu-permission policy, since no "Audit" menu exists). Reachable at /Audit.
[Authorize]
public class AuditController : Controller
{
    private readonly IAuditService _audit;

    public AuditController(IAuditService audit) => _audit = audit;

    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpGet]
    public async Task<IActionResult> Index(
        string? userName, string? level, string? message, DateTime? from, DateTime? to,
        string? sort = "TimeStamp", string? direction = "desc",
        int pageNumber = 1, int pageSize = 10)
    {
        if (!IsAdmin()) return Forbid();

        // [FAU_SAR]: load the filtered records, then page in-memory to feed the shared
        // _DynamicGrid component (mirrors the /User grid contract).
        var rows = (await _audit.QueryAsync(userName, level, message, from, to, 5000, HttpContext.RequestAborted))
            .ToList();

        var total = rows.Count;
        var items = rows
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Cast<object>()
            .ToList();

        ViewBag.UserName = userName;
        ViewBag.Level = level;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");

        var model = new GridViewModel
        {
            SearchTerm = message,
            CurrentSort = sort ?? "TimeStamp",
            CurrentDirection = direction ?? "desc",
            Data = new PagedResult<object>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "زمان",   PropertyName = "TimeStamp", Sortable = false },
                new() { Header = "سطح",    PropertyName = "Level",      Sortable = false },
                new() { Header = "کاربر",  PropertyName = "UserName",   Sortable = false },
                new() { Header = "IP",     PropertyName = "IpAddress", Sortable = false },
                new() { Header = "مسیر",   PropertyName = "Path",       Sortable = false },
                new() { Header = "متد",   PropertyName = "Method",     Sortable = false },
                new() { Header = "پیام",   PropertyName = "Message",    Sortable = false },
            },
            Actions = new List<GridAction>()
        };

        return View(model);
    }
}
