using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSSM.Presentation.Web.Models.Account;

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // نمونه تستی
            if (model.UserName == "admin" && model.Password == "123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin User"),
                    new Claim("UserId", "1"),
                    new Claim("UserType", "Admin")
                };


                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);


                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                return RedirectToAction("Index", "Home");
            }


            ViewBag.Error = "نام کاربری یا رمز عبور اشتباه است";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

    }

    
}
