using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.AdjustReason;
using VSSM.Application.DTOs.BaseEntities.CommercialName;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.Levels;
using VSSM.Application.DTOs.BaseEntities.Ownership;
using VSSM.Application.DTOs.BaseEntities.RoadType;
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
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IAdjustReasonService, AdjustReasonService>();
            services.AddScoped<ICommercialNameService, CommercialNameService>();
            services.AddScoped<IValidator<ItemClassCreateDto>, ItemClassCreateValidator>();
            services.AddScoped<IValidator<CommercialNameDto>, CommercialNameValidator>();
            services.AddScoped<IValidator<AdjustreasonDto>, AdjustReasonValidator>();
            services.AddScoped<IValidator<LevelDto>, LevelValidator>();
            services.AddScoped<IRoadTypeService, RoadTypeService>();
            services.AddScoped<IValidator<RoadTypeEditDto>, RoadTypeEditValidator>();
            services.AddScoped<IValidator<RoadTypeCreateDto>, RoadTypeCreateValidator>();
            services.AddScoped<IOwnershipService, OwnershipService>();
            services.AddScoped<IValidator<OwnershipEditDto>, OwnershipEditValidator>();
            services.AddScoped<IValidator<OwnershipCreateDto>, OwnershipCreateValidator>();
            services.AddScoped<IPowerSourceService, PowerSourceService>();
            services.AddScoped<IEduTypeService, EduTypeService>();
            services.AddScoped<IJobTypeService, JobTypeService>();
            services.AddScoped<ITransportModeService, TransportModeService>();
            services.AddScoped<IReceivingModeService, ReceivingModeService>();
        }
    }

}