using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Infrastructure.Cache;
using BarberSync.Infrastructure.Innovation;
using BarberSync.Infrastructure.Messaging;
using BarberSync.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BarberSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.Configure<MessagingOptions>(configuration.GetSection("Messaging"));

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IInnovationOrchestrator, InMemoryInnovationOrchestrator>();

        return services;
    }
}
