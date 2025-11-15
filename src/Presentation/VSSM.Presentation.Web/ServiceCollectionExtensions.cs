using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Presentation.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceCollectionExtension(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<VSSMDbContext>(options =>
                                    options.UseSqlServer(configuration.GetConnectionString("VSSMDbConnectionStrings")));

            services.AddScoped<IItemClassService, ItemClassService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IValidator<ItemClassCreateDto>, ItemClassCreateValidator>();
            services.AddScoped<IReceivingModeService, ReceivingModeService>();
        }
    }

}