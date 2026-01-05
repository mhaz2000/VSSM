using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.Adjustments;
using VSSM.Application.DTOs.BaseEntities.AdjustReason;
using VSSM.Application.DTOs.BaseEntities.CommercialName;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment;
using VSSM.Application.DTOs.BaseEntities.Levels;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.DTOs.BaseEntities.Manufacturer;
using VSSM.Application.DTOs.BaseEntities.Ownership;
using VSSM.Application.DTOs.BaseEntities.RecipientMaxItem;
using VSSM.Application.DTOs.BaseEntities.RoadType;
using VSSM.Application.DTOs.BaseEntities.StoreType;
using VSSM.Application.DTOs.BaseEntities.StoreUsageWarning;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;
using VSSM.Application.DTOs.BaseEntities.Vvmtype;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Application.Services;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Services.RequestEntities;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web.Controllers;

namespace VSSM.Presentation.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceCollectionExtension(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<VSSMDbContext>(options =>
                                    options.UseSqlServer(configuration.GetConnectionString("VSSMDbConnectionStrings")));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });

            services.AddScoped<IItemClassService, ItemClassService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IAdjustReasonService, AdjustReasonService>();
            services.AddScoped<IManufacturerService, ManufacturerService>();
            services.AddScoped<IManItemClassService, ManItemClassService>();
            services.AddScoped<IUserHomeService, UserHomeService>();
            services.AddScoped<IArrivalRepConfigService, ArrivalRepConfigService>();
            services.AddScoped<IVoucherConfigService, VoucherConfigService>();
            services.AddScoped<ICommercialNameService, CommercialNameService>();
            services.AddScoped<IVvmtypeService, VvmtypeService>();
            services.AddScoped<IVvmstageService, VvmstageService>();
            services.AddScoped<IDispatchRepConfigService, DispatchRepConfigService>();
            services.AddScoped<IStoreUsageWarningService, StoreUsageWarningService>();
            services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
            services.AddScoped<IRecipientMaxItemService, RecipientMaxItemService>();
            services.AddScoped<IStoreTypeService, StoreTypeService>();
            services.AddScoped<IValidator<RecipientMaxItemDto>, RecipientMaxItemValidator>();
            services.AddScoped<IValidator<VvmtypeDto>, VvmtypeValidator>();
            services.AddScoped<IValidator<StoreUsageWarningDto>, StoreUsageWarningValidator>();
            services.AddScoped<IValidator<ManufacturerDto>, ManufacturerDtoValidator>();
            services.AddScoped<IValidator<FiscalPeriodDto>, FiscalPeriodDtoValidator>();
            services.AddScoped<IValidator<ManItemClassDto>, ManItemClassValidator>();
            services.AddScoped<IValidator<StoreTypeDto>, StoreTypeValidator>();
            services.AddScoped<IValidator<VvmstageDto>, VvmstageValidator>();
            services.AddScoped<IValidator<ItemClassCreateDto>, ItemClassCreateValidator>();
            services.AddScoped<IValidator<ItemCreateDto>, ItemCreateValidator>();
            services.AddScoped<IValidator<CommercialNameDto>, CommercialNameValidator>();
            services.AddScoped<IValidator<AdjustreasonDto>, AdjustReasonValidator>();
            services.AddScoped<IValidator<LevelDto>, LevelValidator>();
            services.AddScoped<IRoadTypeService, RoadTypeService>();
            services.AddScoped<IValidator<RoadTypeEditDto>, RoadTypeEditValidator>();
            services.AddScoped<IValidator<RoadTypeCreateDto>, RoadTypeCreateValidator>();
            services.AddScoped<IOwnershipService, OwnershipService>();
            services.AddScoped<IValidator<OwnershipEditDto>, OwnershipEditValidator>();
            services.AddScoped<IValidator<OwnershipCreateDto>, OwnershipCreateValidator>();
            services.AddScoped<IValidator<DosePerVialCreateDto>, DosePerVialCreateValidator>();
            services.AddScoped<IValidator<ItemDoseAssignmentCreateDto>, ItemDoseAssignmentCreateValidator>();
            services.AddScoped<IPowerSourceService, PowerSourceService>();
            services.AddScoped<IEduTypeService, EduTypeService>();
            services.AddScoped<IJobTypeService, JobTypeService>();
            services.AddScoped<ITransportModeService, TransportModeService>();
            services.AddScoped<IReceivingModeService, ReceivingModeService>();
            services.AddScoped<IClimateService, ClimateService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISourceService, SourceService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IPackService, PackService>();
            services.AddScoped<IRecipientService, RecipientService>();
            services.AddScoped<IRecipientConfigurationService, RecipientConfigurationService>();
            services.AddScoped<IDosePerVialService, DosePerVialService>();
            services.AddScoped<IItemDoseAssignmentService, ItemDoseAssignmentService>();
            services.AddScoped<IMinMaxItemService, MinMaxItemService>();
            services.AddScoped<IArrivalService, ArrivalService>();
            services.AddScoped<IUseService, UseService>();
            services.AddScoped<IItemDetailsService, ItemDetailService>();
            services.AddScoped<IPackingTypeService, PackingTypeService>();
            services.AddScoped<IAdjustmentService, AdjustmentService>();
            services.AddScoped<IValidator<AdjustmentDto>, AdjustmentDtoValidator>();
            services.AddScoped<IDispatchService, DispatchService>();
            services.AddScoped<IRequestService, RequestService>();
        }
    }

}