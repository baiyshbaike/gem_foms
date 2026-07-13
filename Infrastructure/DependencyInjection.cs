using Application.Admin;
using Application.Audit;
using Application.Auth;
using Application.MedCards;
using Application.MedCenterMachines;
using Application.Patients;
using Application.Regions;
using Application.Sessions;
using Application.Tenants;
using Infrastructure.Admin;
using Infrastructure.Audit;
using Infrastructure.Auth;
using Infrastructure.Data;
using Infrastructure.MedCards;
using Infrastructure.MedCenterMachines;
using Infrastructure.Patients;
using Infrastructure.Regions;
using Infrastructure.Sessions;
using Infrastructure.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conncetionsString = configuration.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conncetionsString));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IActionLogService, ActionLogService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantAccessService, TenantAccessService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IMedCardService, MedCardService>();
        services.AddScoped<IHdSessionService, HdSessionService>();
        services.AddScoped<ISessionWorkflowSettingsService, SessionWorkflowSettingsService>();
        services.AddHostedService<HdSessionWorkflowWorker>();
        services.AddScoped<IMedCenterMachineService, MedCenterMachineService>();
        return services;
    }
}
