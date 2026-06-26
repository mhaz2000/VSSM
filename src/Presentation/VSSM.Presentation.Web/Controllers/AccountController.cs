using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Account;

namespace VSSM.Presentation.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController(IUserService userService, IRecipientService recipientService) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // [SECURITY]: محافظت در برابر CSRF
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // [SECURITY - افتا]: پیام خطا عمداً مبهم است تا مهاجم نتواند
            // بین "کاربر وجود ندارد" و "رمز اشتباه است" تمایز قائل شود.
            const string genericError = "نام کاربری یا رمز عبور اشتباه است";

            var user = await userService.GetByUsername(model.UserName.Trim(), CancellationToken.None);

            if (user == null)
            {
                // [SECURITY]: تأخیر مصنوعی برای جلوگیری از timing attack
                await Task.Delay(300);
                ViewBag.Error = genericError;
                return View(model);
            }

            // TODO [SECURITY - افتا - اولویت بالا]: رمز عبور باید با الگوریتم امن
            // (bcrypt / Argon2 / PBKDF2) هش شده در پایگاه داده ذخیره شود.
            // مقایسه plain-text فعلی کاملاً ناامن است و باید در اسرع وقت برطرف شود.
            if (user.Password != model.Password.Trim())
            {
                await Task.Delay(300);
                ViewBag.Error = genericError;
                return View(model);
            }

            var recipient = await recipientService.GetById(
                user.RecipientId.GetValueOrDefault(), CancellationToken.None);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("UserRole", user.IsAdmin ? "Admin" : "User"),
                new Claim("RecipientId", user.RecipientId.GetValueOrDefault().ToString()),
                new Claim("RoleId", user.RoleId.GetValueOrDefault().ToString()),
                new Claim("Level", recipient?.ChainLevel.GetValueOrDefault().ToString() ?? "0"),
                new Claim("RecipientParentId", recipient?.ParentId.GetValueOrDefault().ToString() ?? "0"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                // [SECURITY]: کوکی session-based (بسته شدن مرورگر = خروج)
                IsPersistent = false,
                // [SECURITY - افتا]: حداکثر مدت session طبق الزامات افتا (معمولاً ۳۰ دقیقه)
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        [HttpPost]
        // [SECURITY - یادداشت]: ایده‌آل POST + AntiForgeryToken است.
        // اگر layout اصلی از _LogoutButton.cshtml (partial view) استفاده می‌کند،
        // HttpGet را حذف کنید و [ValidateAntiForgeryToken] را اضافه کنید.
        // HttpGet فعلاً برای سازگاری با لینک‌های موجود در سایت نگه داشته شده.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}