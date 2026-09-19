using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces;
using VSSM.Presentation.Web.Models.Account;
using VSSM.Presentation.Web.Models.User;
using VSSM.Presentation.Web.Services;

namespace VSSM.Presentation.Web.Controllers;

public class AccountController(
    IUserService userService,
    IRecipientService recipientService,
    ICaptchaService captcha,
    IRateLimitService rateLimit,
    IAuditService audit,
    IRefreshTokenService refreshTokens,
    RefreshTokenCookieService refreshCookie) : Controller
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

        // [SECURITY - A07]: carry the forced-password-change flag as a claim so the
        // ForcePasswordChangeFilter can keep the user on the change screen until done.
        if (user.MustChangePassword)
            claims.Add(new Claim("MustChangePassword", "true"));

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

        // [SECURITY - A07]: صدور توکن تازه‌ساز (refresh token) به‌صورت کوکی HttpOnly.
        // فقط هشِ آن در پایگاه ذخیره می‌شود (IssueAsync) و نشست کوکیِ نزدیک به انقضا را
        // بی‌صدا و با چرخش توکن تمدید می‌کند (RefreshTokenSessionMiddleware).
        var refreshToken = await refreshTokens.IssueAsync(user.UserId, ip, CancellationToken.None);
        refreshCookie.Append(HttpContext, refreshToken);

        await audit.LogAsync("Information", "Login success", user.UserName, ip, "/Account/Login", "POST");

        // [SECURITY - A07]: force a password change on first login before anything else.
        if (user.MustChangePassword)
            return RedirectToAction("ForceChangePassword", "Account");

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

        // [SECURITY - A07]: خروج واقعی = باطل‌کردن تمام توکن‌های تازه‌سازِ کاربر و حذف کوکی،
        // تا نشست از طریق توکنِ سرقت‌شده قابل تمدید نباشد.
        var userId = GetCurrentUserId();
        if (userId > 0)
            await refreshTokens.RevokeAllForUserAsync(userId, CancellationToken.None);
        refreshCookie.Delete(HttpContext);

        await audit.LogAsync("Information", "Logout", User.Identity?.Name, ip,
            HttpContext.Request.Path, HttpContext.Request.Method);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ====================== FORCED PASSWORD CHANGE (first login) ======================
    // [SECURITY - A07/FIA_PMG]: a user flagged with MustChangePassword must set a new password
    // before using the app. The ForcePasswordChangeFilter redirects any authenticated request
    // here until the flag is cleared. This is strictly self-service (userId is taken from the
    // authenticated identity, never the posted form).
    [Authorize]
    [HttpGet]
    public IActionResult ForceChangePassword()
    {
        if (!User.HasClaim("MustChangePassword", "true"))
            return RedirectToAction("Index", "Home");

        var model = new ChangePasswordViewModel { UserId = GetCurrentUserId() };
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceChangePassword(ChangePasswordViewModel model)
    {
        var userId = GetCurrentUserId();
        model.UserId = userId; // ignore any client-supplied id

        if (!ModelState.IsValid)
            return View(model);

        if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "کلمه عبور و تکرار آن مطابقت ندارند.");
            return View(model);
        }

        var pwdErrors = PasswordHasher.ValidateStrength(model.NewPassword);
        if (pwdErrors.Count > 0)
        {
            foreach (var e in pwdErrors)
                ModelState.AddModelError("NewPassword", e);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await userService.ChangePassword(userId, model.NewPassword, CancellationToken.None);
        await userService.SetMustChangePasswordAsync(userId, false, CancellationToken.None);

        // Drop the forced-change claim by re-issuing the auth cookie without it.
        var identity = (ClaimsIdentity)User.Identity!;
        var mc = identity.FindFirst("MustChangePassword");
        if (mc != null) identity.RemoveClaim(mc);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) });

        // [SECURITY - A07]: تغییر رمز، تمام توکن‌های تازه‌سازِ قبلی را باطل و برای ادامه‌ی نشست
        // توکن جدید صادر می‌کند تا نشست‌های قدیمیِ دارای refresh token نامعتبر شوند.
        await refreshTokens.RevokeAllForUserAsync(userId, CancellationToken.None);
        refreshCookie.Append(HttpContext, await refreshTokens.IssueAsync(userId, ip, CancellationToken.None));

        await audit.LogAsync("Information", "Password changed (forced first login)", User.Identity?.Name, ip,
            HttpContext.Request.Path, HttpContext.Request.Method);

        TempData["ToastMessage"] = "رمز عبور با موفقیت تغییر یافت.";
        TempData["ToastType"] = "success";
        return RedirectToAction("Index", "Home");
    }

    private int GetCurrentUserId()
    {
        var v = User.FindFirst("UserId")?.Value;
        return int.TryParse(v, out var id) ? id : 0;
    }
}