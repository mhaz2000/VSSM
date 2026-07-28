using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using VSSM.Application.DTOs.Dashboard;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services;
using VSSM.Presentation.Web.Models;

namespace VSSM.Presentation.Web.Controllers;


public class HomeController(
    ILogger<HomeController> logger,
    IDashboardService dashboardService) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var recipientId = CurrentUserRecipientId; 

        if (recipientId == 0)
        {
            logger.LogWarning("کاربر جاری فاقد RecipientId معتبر است.");
            return View(new DashboardViewModel()); 
        }

        try
        {
            var model = await dashboardService.GetDashboardDataAsync(recipientId!.Value);
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در بارگذاری داشبورد");
            return View(new DashboardViewModel()); 
        }
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult UnderConstruction()
    {
        return View();
    }

    public IActionResult ExpiredItemsReport(int id)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("~/Views/Shared/UnderConstructionModal.cshtml");
        }

        return View("~/Views/Shared/UnderConstruction.cshtml");
    }


}
