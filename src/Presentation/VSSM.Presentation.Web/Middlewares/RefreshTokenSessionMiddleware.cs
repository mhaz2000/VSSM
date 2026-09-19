using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using VSSM.Application.Interfaces;
using VSSM.Application.Security;
using VSSM.Presentation.Web.Services;

namespace VSSM.Presentation.Web.Middlewares;

/// <summary>
/// [SECURITY - A07]: silently renews an authenticated cookie session when it is about to expire
/// (within the renewal window) and a valid HttpOnly refresh token is present. Effectively the
/// session becomes sliding for active users, but it stays bounded: an idle or token-less user
/// still hits the absolute 30-minute boundary, and the refresh token itself expires (default 7 days).
/// </summary>
public class RefreshTokenSessionMiddleware(
    RequestDelegate next,
    IOptions<RefreshTokenOptions> options)
{
    private readonly TimeSpan _renewalWindow =
        TimeSpan.FromMinutes(Math.Max(1, options.Value.RenewalWindowMinutes));

    public async Task InvokeAsync(
        HttpContext context,
        IRefreshTokenService refreshTokens,
        RefreshTokenCookieService refreshCookie,
        IOptionsMonitor<CookieAuthenticationOptions> cookieOptions)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var raw = refreshCookie.Read(context);
            if (!string.IsNullOrEmpty(raw))
                await TryRenewAsync(context, raw, refreshTokens, refreshCookie, cookieOptions);
        }

        await next(context);
    }

    private async Task TryRenewAsync(
        HttpContext context,
        string raw,
        IRefreshTokenService refreshTokens,
        RefreshTokenCookieService refreshCookie,
        IOptionsMonitor<CookieAuthenticationOptions> cookieOptions)
    {
        var props = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;
        if (props?.ExpiresUtc is not { } expiresUtc)
            return;

        // Only touch the session while it still has life left and is inside the renewal window.
        var remaining = expiresUtc - DateTimeOffset.UtcNow;
        if (remaining <= TimeSpan.Zero || remaining > _renewalWindow)
            return;

        var userIdValue = context.User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return;

        try
        {
            if (!await refreshTokens.TryValidateAsync(raw, userId, context.RequestAborted))
            {
                // Stale/foreign invalid token: drop it so it is not replayed until the session dies.
                refreshCookie.Delete(context);
                return;
            }

            // Same absolute session length the cookie auth middleware is configured with, so the
            // App-wide session policy (30 min, per افتا) is preserved on every renewal.
            var sessionLength = cookieOptions.Get(CookieAuthenticationDefaults.AuthenticationScheme)?.ExpireTimeSpan
                ?? TimeSpan.FromMinutes(30);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                context.User,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(sessionLength)
                });

            // Rotate the refresh token so each renewal issues a brand-new secret.
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var rotated = await refreshTokens.RotateAsync(raw, userId, ip, context.RequestAborted);
            refreshCookie.Append(context, rotated);
        }
        catch (Exception ex)
        {
            // [SECURITY - A07]: on any renewal failure the current request must not break and no
            // internal detail may leak; the session simply expires at its configured boundary.
            var logger = context.RequestServices.GetRequiredService<ILogger<RefreshTokenSessionMiddleware>>();
            logger.LogWarning(ex, "Refresh-token session renewal failed for user {UserId}", userId);
        }
    }
}