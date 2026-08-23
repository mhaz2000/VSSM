using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.Common.Authorization;
using VSSM.Application.Common.Interfaces;
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
using VSSM.Application.Services.Barcode;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Services.ExcelGenerators;
using VSSM.Application.Services.Reports;
using VSSM.Application.Services.RequestEntities;
using VSSM.Application.Services.UidHistory;
using VSSM.Infrastructure.Data.Dapper;
using VSSM.Infrastructure.Data.Models;
using VSSM.Infrastructure.Services.BaseEntities;
using VSSM.Presentation.Web.Authentication;
using VSSM.Presentation.Web.Authorization;

namespace VSSM.Presentation.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceCollectionExtension(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<VSSMDbContext>(options =>
                                    options.UseSqlServer(configuration.GetConnectionString("VSSMDbConnectionStrings")));

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddCookie(options =>
            //{
            //    options.LoginPath = "/Account/Login";
            //});
            services.AddScoped<IUidHistoryService, UidHistoryService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IArrivalRepConfigService, ArrivalRepConfigService>();

            services.AddScoped<IDestroyUidService, DestroyUidService>();
            services.AddScoped<IUidInfoService, UidInfoService>();
            services.AddScoped<IConsumerService, ConsumerService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IManItemClassService, ManItemClassService>();
            services.AddScoped<IVoucherConfigService, VoucherConfigService>();
            services.AddScoped<IDispatchRepConfigService, DispatchRepConfigService>();
            services.AddScoped<IOrderInfoService, OrderInfoService>();
            services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
            services.AddScoped<IRecipientMaxItemService, RecipientMaxItemService>();
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
            services.AddScoped<IValidator<RoadTypeEditDto>, RoadTypeEditValidator>();
            services.AddScoped<IValidator<RoadTypeCreateDto>, RoadTypeCreateValidator>();
            services.AddScoped<IValidator<OwnershipEditDto>, OwnershipEditValidator>();
            services.AddScoped<IValidator<OwnershipCreateDto>, OwnershipCreateValidator>();
            services.AddScoped<IValidator<DosePerVialCreateDto>, DosePerVialCreateValidator>();
            services.AddScoped<IValidator<ItemDoseAssignmentCreateDto>, ItemDoseAssignmentCreateValidator>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IRecipientService, RecipientService>();
            services.AddScoped<IBaseInfoService, BaseInfoService>();
            services.AddScoped<IRecipientConfigurationService, RecipientConfigurationService>();
            services.AddScoped<IItemDoseAssignmentService, ItemDoseAssignmentService>();
            services.AddScoped<IMinMaxItemService, MinMaxItemService>();
            services.AddScoped<IArrivalService, ArrivalService>();
            services.AddScoped<IItemDetailsService, ItemDetailService>();
            services.AddScoped<IWarningService, WarningService>();
            services.AddScoped<IAdjustmentService, AdjustmentService>();
            services.AddScoped<IValidator<AdjustmentDto>, AdjustmentDtoValidator>();
            services.AddScoped<IDispatchService, DispatchService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IItemDetailStoreService, ItemDetailStoreService>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IStoreUsageWarningService, StoreUsageWarningService>();

            services.AddScoped<IReportExcelGenerator, ReportExcelGenerator>();
            services.AddScoped<IGs1BarcodeParser, Gs1BarcodeParser>();

            services.AddSingleton<DispatchScanSessionService>();

            /*reports services DI*/
            #region reports services DI

            services.AddScoped<DapperContext>();
            services.AddScoped<IndicatorReportService>();

            #endregion




            services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                })
                .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("Basic", null);
            services.AddScoped<IMenuPermissionService, MenuPermissionService>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MenuPermission",
                    policy => policy.Requirements.Add(new MenuPermissionRequirement()));
            });
            services.AddScoped<IAuthorizationHandler, MenuPermissionHandler>();
        }
    }
}