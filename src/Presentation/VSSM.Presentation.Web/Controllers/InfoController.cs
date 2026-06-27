
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VSSM.Presentation.Web.Controllers;

[AllowAnonymous] 
public class InfoController : BaseController
{
    [HttpGet]
    public IActionResult About() => View();

    [HttpGet]
    public IActionResult Privacy() => View();
}

