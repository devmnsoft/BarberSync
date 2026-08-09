using BarberSync.Api.Middleware;
using BarberSync.Api.Swagger;
using BarberSync.Api.Services.Configuration;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Api.Validators;
using BarberSync.Application.DTOs;
using BarberSync.Application;
using BarberSync.Infrastructure;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    options.OperationFilter<FileUploadOperationFilter>();
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("DefaultCors", policy =>
{
    if (allowedOrigins.Length == 0 && !builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Cors:AllowedOrigins deve ser configurado em produção.");
    policy.WithOrigins(allowedOrigins.Length == 0 ? ["http://localhost:5081", "http://localhost:5082", "http://localhost:5083"] : allowedOrigins)
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IBarberSchemaInitializer, BarberSchemaInitializer>();
builder.Services.AddScoped<EnterpriseDataService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.GetRequiredService<IBarberSchemaInitializer>().InitializeAsync(app.Lifetime.ApplicationStopping);

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
