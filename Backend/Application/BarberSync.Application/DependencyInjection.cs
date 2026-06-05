using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.Abstractions.AutonomousGrowth;
using BarberSync.Application.Abstractions.Hr;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.Abstractions.Strategy;
using BarberSync.Application.Services.Ai;
using BarberSync.Application.Services.AutonomousGrowth;
using BarberSync.Application.Services.Commercial;
using BarberSync.Application.Services.Hr;
using BarberSync.Application.Services.Saas;
using BarberSync.Application.Services.Strategy;
using Microsoft.Extensions.DependencyInjection;

namespace BarberSync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<InMemorySaasStore>();

        services.AddScoped<ISaasService, SaasService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<CommercialPlatformService>();
        services.AddScoped<StrategicManagementService>();
        services.AddScoped<CrmLeadService>();

        services.AddScoped<ICopilotService, CopilotService>();
        services.AddScoped<IAiProvider, MockAiProvider>();
        services.AddScoped<IHrProfessionalService, HrProfessionalService>();
        services.AddScoped<IAutonomousGrowthService, AutonomousGrowthService>();
        services.AddScoped<IStrategicGrowthService, StrategicGrowthService>();

        return services;
    }
}
