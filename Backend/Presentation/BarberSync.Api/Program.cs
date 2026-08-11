using BarberSync.Api.Middleware;
using BarberSync.Api.Swagger;
using BarberSync.Api.Services.Configuration;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Api.Validators;
using BarberSync.Api.Security;
using BarberSync.Application.Abstractions;
using BarberSync.Infrastructure.Security;
using BarberSync.Application.DTOs;
using BarberSync.Application;
using BarberSync.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

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
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    options.OperationFilter<FileUploadOperationFilter>();
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>() });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("DefaultCors", policy =>
{
    if (allowedOrigins.Length == 0 && !builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Cors:AllowedOrigins deve ser configurado em produção.");
    policy.WithOrigins(allowedOrigins.Length == 0 ? ["http://localhost:5081", "http://localhost:5082", "http://localhost:5083"] : allowedOrigins)
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer,
        ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = "email", RoleClaimType = "roles"
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options => options.AddPolicy("login", context =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<RequirePermissionFilter>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IBarberSchemaInitializer, BarberSchemaInitializer>();
builder.Services.AddScoped<EnterpriseDataService>();
builder.Services.AddScoped<BarberSync.Api.Services.Onboarding.BranchOnboardingService>();
builder.Services.AddScoped<BarberSync.Api.Services.Growth.GrowthService>();
builder.Services.AddScoped<BarberSync.Api.Services.Growth.IAssistantInsightService>(sp => sp.GetRequiredService<BarberSync.Api.Services.Growth.GrowthService>());
builder.Services.AddScoped<BarberSync.Api.Services.Growth.IAssistantRepository>(sp => sp.GetRequiredService<BarberSync.Api.Services.Growth.GrowthService>());
builder.Services.AddScoped<BarberSync.Api.Services.Growth.IAssistantService>(sp => sp.GetRequiredService<BarberSync.Api.Services.Growth.GrowthService>());
builder.Services.AddSingleton<BarberSync.Api.Services.Growth.IWhatsAppProvider, BarberSync.Api.Services.Growth.UnconfiguredWhatsAppProvider>();
builder.Services.AddSingleton<BarberSync.Api.Services.Growth.IEmailProvider, BarberSync.Api.Services.Growth.UnconfiguredEmailProvider>();
builder.Services.AddSingleton<BarberSync.Api.Services.Growth.ISmsProvider, BarberSync.Api.Services.Growth.UnconfiguredSmsProvider>();
builder.Services.AddScoped<BarberSync.Api.Services.Growth.INotificationDispatcher, BarberSync.Api.Services.Growth.NotificationDispatcher>();
builder.Services.AddSingleton<BarberSync.Api.Services.Recognition.IDevRuleBasedRecognitionProvider, BarberSync.Api.Services.Recognition.DevRuleBasedRecognitionProvider>();
builder.Services.AddScoped<BarberSync.Api.Services.Recognition.IServiceRecognitionService, BarberSync.Api.Services.Recognition.ServiceRecognitionService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.GetRequiredService<IBarberSchemaInitializer>().InitializeAsync(app.Lifetime.ApplicationStopping);

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("DefaultCors");

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
