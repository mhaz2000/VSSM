using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.AdjustReason;
using VSSM.Application.DTOs.BaseEntities.CommercialName;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.Levels;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.DTOs.BaseEntities.Manufacturer;
using VSSM.Application.DTOs.BaseEntities.Ownership;
using VSSM.Application.DTOs.BaseEntities.RoadType;
using VSSM.Application.DTOs.BaseEntities.StoreType;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;
using VSSM.Application.DTOs.BaseEntities.Vvmtype;
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
            services.AddScoped<IManufacturerService, ManufacturerService>();
            services.AddScoped<IManItemClassService, ManItemClassService>();
            services.AddScoped<ICommercialNameService, CommercialNameService>();
            services.AddScoped<IVvmtypeService, VvmtypeService>();
            services.AddScoped<IVvmstageService, VvmstageService>();
            services.AddScoped<IStoreTypeService, StoreTypeService>();
            services.AddScoped<IValidator<VvmtypeDto>, VvmtypeValidator>();
            services.AddScoped<IValidator<ManufacturerDto>, ManufacturerDtoValidator>();
            services.AddScoped<IValidator<ManItemClassDto>, ManItemClassValidator>();
            services.AddScoped<IValidator<StoreTypeDto>, StoreTypeValidator>();
            services.AddScoped<IValidator<VvmstageDto>, VvmstageValidator>();
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
            services.AddScoped<IClimateService, ClimateService>();
        }
    }

}