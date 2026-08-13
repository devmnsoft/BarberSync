using BarberSync.Api.Services.Enterprise;
using BarberSync.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;
using System.Reflection;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/health")]
[Route("api/system/health")]
public sealed class HealthController(
    IBarberSchemaInitializer schemaInitializer,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IHttpClientFactory httpClientFactory,
    ILogger<HealthController> logger) : ControllerBase
{
    private const string ConnectionStringName = "DefaultConnection";

    private static readonly IReadOnlyDictionary<string, string> RealDataChecks = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["clients"] = "select count(*) from barber.clients where deleted_at is null and is_active",
        ["professionals"] = "select count(*) from barber.professionals where deleted_at is null and is_active",
        ["services"] = "select count(*) from barber.services where deleted_at is null and is_active",
        ["products"] = "select count(*) from barber.products where deleted_at is null and is_active",
        ["appointments"] = "select count(*) from barber.appointments where deleted_at is null and is_active",
        ["service_orders"] = "select count(*) from barber.service_orders where deleted_at is null and is_active",
        ["payments"] = "select count(*) from barber.payments where deleted_at is null and is_active",
        ["reviews"] = "select count(*) from barber.reviews where deleted_at is null and is_active",
        ["kiosk_devices"] = "select count(*) from barber.kiosk_devices where deleted_at is null and is_active"
    };

    [HttpGet("database")]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        var result = await schemaInitializer.CheckHealthAsync(cancellationToken);
        return Ok(new
        {
            success = result.Success,
            databaseConnected = result.DatabaseConnected,
            schemaReady = result.SchemaReady,
            message = result.Message,
            database = result.Database,
            schema = result.Schema,
            environment = result.Environment,
            step = result.Step,
            databaseStatus = result.DatabaseStatus,
            schemaVersions = result.SchemaVersions
        });
    }

    [HttpGet("/api/system/version")]
    [AllowAnonymous]
    public IActionResult Version()
    {
        var assembly = typeof(HealthController).Assembly;
        return Ok(new
        {
            service = "BarberSync.Api",
            version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "unknown",
            environment = environment.EnvironmentName,
            startedAtUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            serverTimeUtc = DateTimeOffset.UtcNow
        });
    }

    [HttpGet("/api/system/dependencies")]
    [Authorize]
    [RequirePermission("SystemHealth.View")]
    public async Task<IActionResult> Dependencies(CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var database = await schemaInitializer.CheckHealthAsync(cancellationToken);
        var checks = new List<object>
        {
            Dependency("database", database.DatabaseConnected, database.DatabaseStatus, true),
            Dependency("schema:barber", database.SchemaReady, database.SchemaReady ? "Ready" : "Unavailable", true),
            Provider("whatsapp", "Providers:WhatsApp:Enabled"),
            Provider("email", "Providers:Email:Enabled")
        };

        foreach (var service in new[] { "AdminWeb", "PublicWeb", "Totem" })
            checks.Add(await CheckHttpDependency(service, cancellationToken));

        return Ok(new
        {
            status = database.DatabaseConnected && database.SchemaReady ? "Healthy" : "Degraded",
            schemaVersions = database.SchemaVersions,
            elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds,
            checkedAtUtc = DateTimeOffset.UtcNow,
            dependencies = checks
        });
    }

    [HttpGet("real-data")]
    public async Task<IActionResult> RealData(CancellationToken cancellationToken)
    {
        var schemaHealth = await schemaInitializer.CheckHealthAsync(cancellationToken);

        if (!schemaHealth.DatabaseConnected || !schemaHealth.SchemaReady)
        {
            return Ok(new
            {
                success = false,
                databaseConnected = schemaHealth.DatabaseConnected,
                schemaReady = schemaHealth.SchemaReady,
                realDataReady = false,
                message = "Banco ou schema BarberSync ainda não está pronto para validar dados reais.",
                resources = Array.Empty<object>(),
                environment = environment.EnvironmentName
            });
        }

        try
        {
            await using var connection = new NpgsqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            var resources = new List<object>();
            var readyTables = 0;

            foreach (var check in RealDataChecks)
            {
                var count = await CountRowsAsync(connection, check.Value, cancellationToken);
                var ready = count > 0;
                if (ready) readyTables++;

                resources.Add(new
                {
                    resource = check.Key,
                    count,
                    ready,
                    message = ready
                        ? $"barber.{check.Key} possui {count} registro(s) ativo(s)."
                        : $"barber.{check.Key} ainda não possui registros ativos."
                });
            }

            var realDataReady = readyTables >= 4;

            return Ok(new
            {
                success = true,
                databaseConnected = true,
                schemaReady = true,
                realDataReady,
                readyTables,
                totalTables = RealDataChecks.Count,
                message = realDataReady
                    ? "Dados reais/seeds mínimos encontrados no PostgreSQL."
                    : "Schema está pronto, mas os dados reais/seeds mínimos ainda precisam ser carregados.",
                resources,
                environment = environment.EnvironmentName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao validar dados reais do BarberSync.");
            return Ok(new
            {
                success = false,
                databaseConnected = false,
                schemaReady = schemaHealth.SchemaReady,
                realDataReady = false,
                message = $"Falha controlada ao validar dados reais: {ex.Message}",
                resources = Array.Empty<object>(),
                environment = environment.EnvironmentName
            });
        }
    }

    private string GetConnectionString()
        => BarberSync.Api.Services.Configuration.DatabaseConnectionResolver.Resolve(configuration)
           ?? throw new InvalidOperationException(BarberSync.Api.Services.Configuration.DatabaseConnectionResolver.MissingConfigurationMessage);

    private static async Task<long> CountRowsAsync(NpgsqlConnection connection, string commandText, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result ?? 0);
    }

    private object Provider(string name, string key)
    {
        var enabled = configuration.GetValue<bool>(key);
        return Dependency(name, enabled, enabled ? "Configured" : "NotConfigured", false);
    }

    private async Task<object> CheckHttpDependency(string name, CancellationToken cancellationToken)
    {
        var url = configuration[$"SystemHealth:Dependencies:{name}"];
        if (string.IsNullOrWhiteSpace(url)) return Dependency(name, false, "NotConfigured", false);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await httpClientFactory.CreateClient().SendAsync(request, cancellationToken);
            return Dependency(name, response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Reachable" : $"HTTP {(int)response.StatusCode}", false);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning("Dependência {Dependency} indisponível. TraceId={TraceId}", name, HttpContext.TraceIdentifier);
            return Dependency(name, false, "Unavailable", false);
        }
    }

    private static object Dependency(string name, bool healthy, string status, bool required)
        => new { name, healthy, status, required };
}
