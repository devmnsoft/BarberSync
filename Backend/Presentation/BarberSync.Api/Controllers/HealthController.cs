using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(
    IBarberSchemaInitializer schemaInitializer,
    IConfiguration configuration,
    IWebHostEnvironment environment,
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
            step = result.Step
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
        => configuration.GetConnectionString(ConnectionStringName)
           ?? "Host=localhost;Port=5432;Database=barber;Username=barbersync;Password=barbersync_demo_10";

    private static async Task<long> CountRowsAsync(NpgsqlConnection connection, string commandText, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result ?? 0);
    }
}
