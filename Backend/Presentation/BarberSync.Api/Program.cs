using BarberSync.Api.Middleware;
using BarberSync.Api.Services.Configuration;
using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.Abstractions.AutonomousGrowth;
using BarberSync.Application.Abstractions.Hr;
using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.Abstractions.Strategy;
using BarberSync.Application.Services.Ai;
using BarberSync.Application.Services.AutonomousGrowth;
using BarberSync.Application.Services.Hr;
using BarberSync.Application.Services.Saas;
using BarberSync.Application.Services.Strategy;
using BarberSync.Infrastructure.Innovation;
using BarberSync.Infrastructure.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BarberSync.Api")
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IInnovationOrchestrator, InMemoryInnovationOrchestrator>();
builder.Services.AddSingleton<InMemorySaasStore>();
builder.Services.AddScoped<ISaasService, SaasService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddSingleton<CommercialPlatformService>();
builder.Services.AddSingleton<BarberSync.Application.Services.Commercial.CrmLeadService>();
builder.Services.AddSingleton<StrategicManagementService>();
builder.Services.AddSingleton<IAiProvider, MockAiProvider>();
builder.Services.AddSingleton<ICopilotService, CopilotService>();
builder.Services.AddSingleton<IHrProfessionalService, HrProfessionalService>();
builder.Services.AddSingleton<IStrategicGrowthService, StrategicGrowthService>();
builder.Services.AddSingleton<IAutonomousGrowthService, AutonomousGrowthService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
