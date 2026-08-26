using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/500")]
    public IActionResult Error500(string message)
    {
        ViewBag.ErrorMessage = message;
        return View("~/Views/Shared/Error.cshtml");
    }

    [HttpGet]
    public IActionResult Forbidden()
    {
        return View("_Forbidden");
    }
}