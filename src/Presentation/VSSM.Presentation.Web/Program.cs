using Microsoft.AspNetCore.Authentication.Cookies;
using VSSM.Application.Services.BackgroundServices;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web;
using VSSM.Presentation.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddServiceCollectionExtension(builder.Configuration);

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
    .GetRequiredService<VSSMDbContext>() // نام DbContext خودت رو بذار
    .Model.GetEntityTypes())
{
    var shadows = entityType.GetProperties()
        .Where(p => p.IsShadowProperty())
        .ToList();

    if (shadows.Any())
        foreach (var shadow in shadows)
            Console.WriteLine($"❌ Entity: {entityType.ClrType.Name} | Shadow: {shadow.Name}");
}

app.UseGlobalException();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
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
