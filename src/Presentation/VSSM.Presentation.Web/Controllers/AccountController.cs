using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using System.Linq;
using VSSM.Presentation.Web.Models.Account;

namespace VSSM.Presentation.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRecipientService _recipientService;

        // TODO [SECURITY - افتا]: پیاده‌سازی Rate Limiting برای جلوگیری از Brute Force
        // از Microsoft.AspNetCore.RateLimiting یا ابزار سطح Reverse Proxy (مثل nginx) استفاده کنید.
        // حداکثر ۵ تلاش ناموفق در ۱۵ دقیقه توصیه می‌شود.

        public AccountController(IUserService userService, IRecipientService recipientService)
        {
            _userService = userService;
            _recipientService = recipientService;
        }

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
            // DEBUG TEMP: نمایش خطاهای ModelState
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                ViewBag.Error = "خطای اعتبارسنجی: " + errors;
                return View(model);
            }

            // [SECURITY - افتا]: پیام خطا عمداً مبهم است تا مهاجم نتواند
            // بین "کاربر وجود ندارد" و "رمز اشتباه است" تمایز قائل شود.
            const string genericError = "نام کاربری یا رمز عبور اشتباه است";

            var user = await _userService.GetByUsername(model.UserName.Trim(), CancellationToken.None);

            if (user == null)
            {
                // [SECURITY]: تأخیر مصنوعی برای جلوگیری از timing attack
                await Task.Delay(300);
                ViewBag.Error = genericError;
                return View(model);
            }

            // [SECURITY]: رمز عبور به صورت هش (PBKDF2) ذخیره و مقایسه می‌شود.
            // کاربرانی که هنوز رمزشان plain-text است، در اولین ورود موفق به‌صورت
            // شفاف (transparent) به هش ارتقا می‌یابند.
            var supplied = model.Password.Trim();
            bool passwordOk;

            if (PasswordHasher.LooksHashed(user.Password))
            {
                passwordOk = PasswordHasher.Verify(supplied, user.Password);
            }
            else
            {
                // Legacy plaintext value: accept on exact match, then upgrade to a hash.
                passwordOk = user.Password == supplied;
                if (passwordOk)
                {
                    await _userService.UpgradePasswordToHash(user.UserId, supplied, CancellationToken.None);
                }
            }

            if (!passwordOk)
            {
                await Task.Delay(300);
                ViewBag.Error = genericError;
                return View(model);
            }

            // recipient ممکن است null باشد — با null-safe handle می‌شود
            var recipient = user.RecipientId.HasValue && user.RecipientId.Value > 0
                ? await _recipientService.GetById(user.RecipientId.Value, CancellationToken.None)
                : null;

            var role = user.IsAdmin ? "Admin" : "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim("UserId", user.UserId.ToString()),
            
                new Claim("UserRole", role),
            
                new Claim(ClaimTypes.Role, role),
            
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