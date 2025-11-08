using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Infrastructure.Data.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VSSMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VSSMDbConnectionStrings")));

// Add services to the container.
builder.Services.AddScoped<IItemClassService, ItemClassService>();
builder.Services.AddScoped<IValidator<ItemClassCreateDto>, ItemClassCreateValidator>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
