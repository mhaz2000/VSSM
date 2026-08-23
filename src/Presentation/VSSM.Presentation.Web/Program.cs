using Microsoft.AspNetCore.Authentication.Cookies;
using VSSM.Application.Services.BackgroundServices;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web;
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

builder.Services.AddHostedService<OrdersInquiryBackgroundService>();


builder.Services.AddMemoryCache();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // [SECURITY - A02/A05]: enforce HSTS in non-dev so the browser only ever talks to us over TLS.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
