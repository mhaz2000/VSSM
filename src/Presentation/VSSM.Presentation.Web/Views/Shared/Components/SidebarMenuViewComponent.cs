using Microsoft.AspNetCore.Mvc;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Views.Shared.Components;

public class SidebarMenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var menu = MenuConfig.GetMenu();
        return View(menu);
    }
}
