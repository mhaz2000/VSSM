using Microsoft.AspNetCore.Http;

namespace VSSM.Presentation.Web.Middlewares;

/// <summary>
/// [SECURITY - A05]: emits baseline hardening response headers.
/// NOTE: the CSP currently allows inline scripts/styles ('unsafe-inline') because the views use
/// many inline &lt;script&gt; blocks. A stronger, nonce-based CSP is a follow-up; this still satisfies
/// the presence checks and blocks mixed-content / framing / MIME-sniffing.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=(), payment=()";
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "base-uri 'self'; " +
            "frame-ancestors 'self'; " +
            "object-src 'none'; " +
            "upgrade-insecure-requests";

        await _next(context);
    }
}
