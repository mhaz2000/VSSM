using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Security.Authentication;
using VSSM.Application.Services.BackgroundServices;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web;
using VSSM.Presentation.Web.Filters;
using VSSM.Presentation.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddServiceCollectionExtension(builder.Configuration, builder.Environment);

// [SECURITY - A02/A05]: HSTS policy (applied via UseHsts in non-dev). 1-year max-age, covers subdomains.
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = false;
});

// [SECURITY - A05/FCS_ITC]: disable legacy TLS at the Kestrel/TLS endpoint (defense-in-depth).
// When hosted in-process on IIS, TLS is terminated by IIS — also disable TLS 1.0/1.1 in IIS/SChannel.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(https =>
    {
        https.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});

builder.Services.AddHostedService<OrdersInquiryBackgroundService>();
builder.Services.AddHostedService<AuditRetentionBackgroundService>();


builder.Services.AddMemoryCache();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews(options => options.Filters.Add<ForcePasswordChangeFilter>()).AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews(options => options.Filters.Add<ForcePasswordChangeFilter>());
}

var app = builder.Build();

foreach (var entityType in app.Services.GetRequiredService<IServiceScopeFactory>()
    .CreateScope().ServiceProvider
    .GetRequiredService<VSSMDbContext>() 
    .Model.GetEntityTypes())
{
    var shadows = entityType.GetProperties()
        .Where(p => p.IsShadowProperty())
        .ToList();

    if (shadows.Any())
        foreach (var shadow in shadows)
            Console.WriteLine($"❌ Entity: {entityType.ClrType.Name} | Shadow: {shadow.Name}");
}

// [SECURITY - A07]: هش کردن یک‌باره تمام رمزهای متنیِ مانده در راه‌اندازی
// (رفع ذخیره‌سازی plaintext و مقایسه غیر‌ثابت‌زمان). در صورت عدم دسترسی به پایگاه،
// فقط هشدار ثبت می‌شود و راه‌اندازی متوقف نمی‌گردد.
try
{
    using var seedScope = app.Services.CreateScope();
    var seedUsers = seedScope.ServiceProvider.GetRequiredService<VSSM.Application.Interfaces.BaseEntities.IUserService>();
    await seedUsers.UpgradeAllPlaintextPasswordsAsync(CancellationToken.None);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "خطا در ارتقای رمزهای متنی در راه‌اندازی (نادیده گرفته شد).");
}

app.UseGlobalException();

// [SECURITY - A05/A09]: trust the real client IP from the upstream proxy/IIS so audit records
// reflect the actual client (not the proxy). For a remote LB/ARR, list its IPs in
// "Security:KnownProxies" (semicolon-separated); loopback is trusted by default (IIS in-process).
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 1
};
var knownProxies = builder.Configuration["Security:KnownProxies"];
if (!string.IsNullOrWhiteSpace(knownProxies))
{
    foreach (var p in knownProxies.Split(';', StringSplitOptions.RemoveEmptyEntries))
        forwardedOptions.KnownProxies.Add(IPAddress.Parse(p.Trim()));
}
app.UseForwardedHeaders(forwardedOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // [SECURITY - A02/A05]: enforce HSTS in non-dev so the browser only ever talks to us over TLS.
    app.UseHsts();
}

app.UseHttpsRedirection();

// [SECURITY - A05]: baseline hardening response headers (CSP, nosniff, X-Frame-Options, Referrer-Policy, Permissions-Policy).
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
