using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Message = "خطای غیرمنتظره‌ای رخ داده است.",
                    // [SECURITY - A05/A09]: never leak raw exception text to the client in production.
                    Error = _env.IsDevelopment() ? ex.Message : (string?)null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            // [SECURITY - A05/A09]: redirect to the generic error page WITHOUT echoing the
            // exception message (avoids leaking internals via URL / logs / Referer).
            context.Response.Redirect("/Error/500");
        }
    }
}