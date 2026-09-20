using Microsoft.Extensions.Options;
using VSSM.Application.Security;

namespace VSSM.Presentation.Web.Services;

/// <summary>
/// [SECURITY - A07]: HttpOnly refresh-token cookie helpers used by the login flow and the
/// <c>RefreshTokenSessionMiddleware</c>. The cookie carries only the raw token; its SHA-256
/// digest is stored server-side. Cookie flags mirror the auth cookie (HttpOnly, SameSite=Lax,
/// Secure when the request is HTTPS) so it stays out of reach of XSS/CSRF.
/// </summary>
public class RefreshTokenCookieService(IOptions<RefreshTokenOptions> options)
{
    public const string CookieName = "VSSM.RefreshToken";

    public string? Read(HttpContext context)
        => context.Request.Cookies[CookieName];

    public void Append(HttpContext context, string token)
    {
        context.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(options.Value.LifetimeDays)
        });
    }

    public void Delete(HttpContext context)
        => context.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
}