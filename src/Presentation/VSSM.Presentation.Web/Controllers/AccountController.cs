using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces;
using VSSM.Presentation.Web.Models.Account;
using VSSM.Presentation.Web.Services;

namespace VSSM.Presentation.Web.Controllers;

[AllowAnonymous]
    public class AccountController(
        IUserService userService,
        IRecipientService recipientService,
        ICaptchaService captcha,
        IRateLimitService rateLimit,
        IAuditService audit) : Controller
    {
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        var model = new LoginViewModel();
        AttachCaptcha(model);
        return View(model);
    }

    // [SECURITY]: تولید کپچای جدید برای دکمه «تغییر کد» بدون از دست رفتن فرم.
    [HttpGet]
    public IActionResult Captcha()
    {
        var c = captcha.Generate();
        return Json(new { token = c.Token, svg = c.Svg });
    }

    private void AttachCaptcha(LoginViewModel model)
    {
        // [SECURITY/BUG]: ورودی‌های asp-for از ModelState اولویت می‌گیرند؛
        // در صورت شکست POST، مقدار CaptchaToken قدیمی دوباره رندر می‌شد در حالی که
        // تصویر کپچای جدید نمایش داده می‌شد. پاکسازی ModelState باعث می‌شود مقدارِ
        // تازه تولید‌شده در مدل (توکن جدید و فیلد خالی) اعمال شود.
        ModelState.Remove("Captcha");
        ModelState.Remove("CaptchaToken");

        var c = captcha.Generate();
        model.CaptchaToken = c.Token;
        model.CaptchaSvg = c.Svg;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userKey = "u_" + (model.UserName ?? string.Empty).Trim().ToLowerInvariant();

        // [SECURITY]: محدودیت نرخ (Rate Limit) بر اساس IP و نام کاربری با حافظه محلی (IMemoryCache)
        var (blocked, remaining) = rateLimit.Check(ip);
        if (!blocked) (blocked, remaining) = rateLimit.Check(userKey);
        if (blocked)
        {
            ViewBag.Error = $"تلاش‌های ناموفق زیاد بود. لطفاً {remaining} ثانیه صبر کنید.";
            await audit.LogAsync("Warning", "Login blocked: too many failed attempts", model.UserName, ip, "/Account/Login", "POST");
            AttachCaptcha(model);
            return View(model);
        }

        // [SECURITY]: اعتبارسنجی کد امنیتی (captcha)
        if (!captcha.Validate(model.CaptchaToken, model.Captcha))
        {
            rateLimit.RegisterFailure(ip);
            rateLimit.RegisterFailure(userKey);
            ViewBag.Error = "کد امنیتی نادرست است.";
            await audit.LogAsync("Warning", "Login failed: captcha mismatch", model.UserName, ip, "/Account/Login", "POST");
            AttachCaptcha(model);
            return View(model);
        }

        // DEBUG TEMP: نمایش خطاهای ModelState
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            ViewBag.Error = "خطای اعتبارسنجی: " + errors;
            AttachCaptcha(model);
            return View(model);
        }

        // [SECURITY - افتا]: پیام خطا عمداً مبهم است تا مهاجم نتواند
        // بین "کاربر وجود ندارد" و "رمز اشتباه است" تمایز قائل شود.
        const string genericError = "نام کاربری یا رمز عبور اشتباه است";

        var supplied = model.Password.Trim();

        var user = await userService.GetByUsername(model.UserName.Trim(), CancellationToken.None);

        if (user == null)
        {
            // [SECURITY - A07]: هشِ ساختگی برای یکسان‌سازی زمان پاسخ با حالت «رمز اشتباه»
            // تا شناسایی وجود/عدم وجود نام کاربری از طریق تفاوت زمان (enumeration) غیرممکن شود.
            PasswordHasher.Verify(supplied, PasswordHasher.DummyHash);
            await Task.Delay(300);
            rateLimit.RegisterFailure(ip);
            rateLimit.RegisterFailure(userKey);
            ViewBag.Error = genericError;
            await audit.LogAsync("Warning", "Login failed: unknown username", model.UserName, ip, "/Account/Login", "POST");
            AttachCaptcha(model);
            return View(model);
        }

        // [SECURITY - A07]: رمز فقط از طریق PBKDF2 (مقایسه constant-time) بررسی می‌شود.
        // تمام رمزهای متنی در راه‌اندازی هش شده‌اند (UpgradeAllPlaintextPasswordsAsync)،
        // بنابراین مسیر مقایسه‌ی plaintext حذف شده است.
        bool passwordOk = PasswordHasher.Verify(supplied, user.Password);

        if (!passwordOk)
        {
            await Task.Delay(300);
            rateLimit.RegisterFailure(ip);
            rateLimit.RegisterFailure(userKey);
            ViewBag.Error = genericError;
            await audit.LogAsync("Warning", "Login failed: wrong password", model.UserName, ip, "/Account/Login", "POST");
            AttachCaptcha(model);
            return View(model);
        }

        // موفقیت: پاکسازی شمارنده‌های خطا
        rateLimit.Reset(ip);
        rateLimit.Reset(userKey);

        // recipient ممکن است null باشد — با null-safe handle می‌شود
        var recipient = user.RecipientId.HasValue && user.RecipientId.Value > 0
            ? await recipientService.GetById(user.RecipientId.Value, CancellationToken.None)
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

        await audit.LogAsync("Information", "Login success", user.FullName, ip, "/Account/Login", "POST");

        if (user.RoleId == 3)
        {
            return RedirectToAction("Index", "InputFiles");
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [SECURITY - افتا]: خروج فقط از طریق POST با AntiForgeryToken انجام می‌شود
    // تا صفحه‌های مخرب نتوانند کاربر را بدون اطلاع او خارج کنند (logout CSRF).
    // لینک‌های GET قدیمی دیگر پاسخ نمی‌گیرند (405) و باید از دکمه/فرم خروج استفاده شود.
    public async Task<IActionResult> Logout()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await audit.LogAsync("Information", "Logout", User.Identity?.Name, ip,
            HttpContext.Request.Path, HttpContext.Request.Method);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}