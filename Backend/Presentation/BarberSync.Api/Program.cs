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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "BarberSync API" }));

app.Run();
