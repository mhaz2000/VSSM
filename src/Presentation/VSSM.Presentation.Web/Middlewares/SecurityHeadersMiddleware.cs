using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace VSSM.Presentation.Web.Middlewares;

/// <summary>
/// [SECURITY - A05]: emits baseline hardening response headers.
/// The CSP is nonce-based: every &lt;script&gt;/&lt;style&gt; BLOCK rendered by the app carries a
/// per-request nonce (see CspNonceTagHelper), so 'unsafe-inline' is removed from script-src/style-src.
/// Inline event-handler attributes (onclick=...) and inline style="" attributes are still permitted via
/// script-src-attr/style-src-attr ('unsafe-inline') because the views use many of them and there is no
/// user-controlled injection sink; fully removing those would require converting handlers to addEventListener.
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

        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        context.Items["CspNonce"] = nonce;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=(), payment=()";
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            "script-src-attr 'unsafe-inline'; " +
            $"style-src 'self' 'nonce-{nonce}'; " +
            "style-src-attr 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "base-uri 'self'; " +
            "frame-ancestors 'self'; " +
            "object-src 'none'; " +
            "upgrade-insecure-requests";

        await _next(context);
    }
}
