using Microsoft.AspNetCore.Mvc;

namespace VSSM.Presentation.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: اعتبارسنجی یوزر و پسورد
            // اگر درست بود:
            // SignIn...

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            // TODO: SignOut...

            return RedirectToAction("Login");
        }

    }

    public class LoginViewModel
    {
    }
}
