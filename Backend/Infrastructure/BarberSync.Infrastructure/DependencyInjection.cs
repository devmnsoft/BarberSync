using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Infrastructure.Cache;
using BarberSync.Infrastructure.Innovation;
using BarberSync.Infrastructure.Messaging;
using BarberSync.Infrastructure.Persistence;
using BarberSync.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using BarberSync.Application.Operations;
using BarberSync.Infrastructure.Repositories;

namespace BarberSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.Configure<MessagingOptions>(configuration.GetSection("Messaging"));

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, PostgresAuthService>();
        services.AddScoped<IFirstAdminSetupService, PostgresFirstAdminSetupService>();
        services.AddScoped<IPasswordHasher<AuthUser>, PasswordHasher<AuthUser>>();
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer é obrigatório.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience é obrigatório.")
            .Validate(o => o.SigningKey.Length >= 32, "Jwt:SigningKey deve possuir pelo menos 32 caracteres.")
            .Validate(o => o.AccessTokenMinutes > 0 && o.RefreshTokenDays > 0, "Expiração JWT inválida.")
            .ValidateOnStart();
        services.AddScoped<IDbConnectionFactory, PostgresConnectionFactory>();
        services.AddScoped<IAppointmentRepository, PostgresAppointmentRepository>();
        services.AddScoped<IServiceOrderRepository, PostgresServiceOrderRepository>();
        services.AddScoped<IPaymentRepository, PostgresServiceOrderRepository>();
        services.AddScoped<ICashRegisterRepository, PostgresCashRegisterRepository>();
        services.AddSingleton<IInnovationOrchestrator, InMemoryInnovationOrchestrator>();

        return services;
    }
}
