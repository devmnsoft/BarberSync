using BarberSync.Api.Middleware;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.Services.Saas;
using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Infrastructure.Innovation;
using BarberSync.Application.Abstractions;
using BarberSync.Infrastructure.Security;
using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.Services.Ai;
using BarberSync.Application.Abstractions.Hr;
using BarberSync.Application.Services.Hr;
using BarberSync.Application.Abstractions.Strategy;
using BarberSync.Application.Services.Strategy;
using BarberSync.Application.Abstractions.AutonomousGrowth;
using BarberSync.Application.Services.AutonomousGrowth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, cfg) => cfg
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BarberSync.Api")
    .WriteTo.Console());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IInnovationOrchestrator, InMemoryInnovationOrchestrator>();
builder.Services.AddSingleton<InMemorySaasStore>();
builder.Services.AddScoped<ISaasService, SaasService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddSingleton<CommercialPlatformService>();
builder.Services.AddSingleton<StrategicManagementService>();
builder.Services.AddSingleton<IAiProvider, MockAiProvider>();
builder.Services.AddSingleton<ICopilotService, CopilotService>();
builder.Services.AddSingleton<IHrProfessionalService, HrProfessionalService>();
builder.Services.AddSingleton<IStrategicGrowthService, StrategicGrowthService>();
builder.Services.AddSingleton<IAutonomousGrowthService, AutonomousGrowthService>();

var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "BarberSync API" }));
app.Run();
