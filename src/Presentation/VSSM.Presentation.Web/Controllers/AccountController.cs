using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Account;

namespace VSSM.Presentation.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRecipientService _recipientService;
        public AccountController(IUserService userService, IRecipientService recipientService)
        {
            _userService = userService;
            _recipientService = recipientService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByUsername(model.UserName.Trim(), CancellationToken.None);

            if (user == null)
            {
                ViewBag.Error = "کاربر یافت نشد";
                return View(model);
            }

            if (user.Password != model.Password.Trim())
            {
                ViewBag.Error = "رمز عبور اشتباه است";
                return View(model);
            }

            //var chainLevel = await _recipientService.GetChainLevelById(user.RecipientId.GetValueOrDefault(), CancellationToken.None);

            var recipient = await _recipientService.GetById(user.RecipientId.GetValueOrDefault(), CancellationToken.None);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("UserRole", user.IsAdmin ? "Admin" : "User"),
                    new Claim("RecipientId", user.RecipientId.GetValueOrDefault().ToString()),
                    new Claim("RoleId", user.RoleId.GetValueOrDefault().ToString()),
                    new Claim("Level", recipient.ChainLevel.GetValueOrDefault().ToString()),
                    new Claim("RecipientParentId", recipient.ParentId.GetValueOrDefault().ToString()),

            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

    }

    
}
