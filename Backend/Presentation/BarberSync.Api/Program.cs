using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.Services.Saas;
using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Infrastructure.Innovation;
using BarberSync.Application.Abstractions;
using BarberSync.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IInnovationOrchestrator, InMemoryInnovationOrchestrator>();
builder.Services.AddSingleton<InMemorySaasStore>();
builder.Services.AddScoped<ISaasService, SaasService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddSingleton<CommercialPlatformService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "BarberSync API" }));

app.Run();
