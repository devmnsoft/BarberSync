using BarberSync.Api.Services.Configuration;
using Npgsql;

namespace BarberSync.Api.Services.Enterprise;

/// <summary>
/// Performs a lightweight readiness check. Database changes are deliberately owned by
/// ScriptsSQL/script_completo.sql and are never applied during application startup.
/// </summary>
public sealed class BarberSchemaInitializer(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<BarberSchemaInitializer> logger) : IBarberSchemaInitializer
{
    private const string SchemaName = "barber";
    private const string ConfigurationHelp = """
        ConnectionStrings:DefaultConnection não foi configurada.

        Configure uma das opções:
        - User Secrets: ConnectionStrings:DefaultConnection
        - Variável de ambiente: ConnectionStrings__DefaultConnection
        - Variável com prefixo: BARBERSYNC_ConnectionStrings__DefaultConnection
        - appsettings.Development.json
        - docker-compose.yml

        Execute Scripts/check-api-config.ps1 para diagnosticar o ambiente local.
        """;

    public DatabaseHealthResult LastResult { get; private set; } = NotConfigured("Unknown");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Validando configuração do PostgreSQL. Environment={Environment}", environment.EnvironmentName);
        LastResult = await ProbeAsync(cancellationToken);

        if (LastResult.DatabaseStatus == "NotConfigured")
            logger.LogError("{ConfigurationHelp} Environment={Environment}", ConfigurationHelp, environment.EnvironmentName);
        else if (!LastResult.Success)
            logger.LogError("PostgreSQL indisponível ou schema inválido. Environment={Environment}, Step={Step}, Message={Message}",
                environment.EnvironmentName, LastResult.Step, LastResult.Message);
        else
            logger.LogInformation("PostgreSQL pronto. Database={Database}, Schema={Schema}, SchemaVersions={SchemaVersions}",
                LastResult.Database, SchemaName, LastResult.SchemaVersions);
    }

    public async Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        LastResult = await ProbeAsync(cancellationToken);
        return LastResult;
    }

    private async Task<DatabaseHealthResult> ProbeAsync(CancellationToken cancellationToken)
    {
        var connectionString = DatabaseConnectionResolver.Resolve(configuration);
        if (string.IsNullOrWhiteSpace(connectionString)) return NotConfigured(environment.EnvironmentName);

        string database;
        try
        {
            database = new NpgsqlConnectionStringBuilder(connectionString).Database ?? string.Empty;
        }
        catch (ArgumentException)
        {
            return new(false, false, false, "ConnectionStrings:DefaultConnection possui formato inválido.", string.Empty,
                SchemaName, environment.EnvironmentName, "Configuration", null, 0, "InvalidConfiguration");
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                select
                    exists(select 1 from information_schema.schemata where schema_name = 'barber'),
                    exists(select 1 from information_schema.tables where table_schema = 'barber' and table_name = 'schema_versions')
                """, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var schemaExists = reader.GetBoolean(0);
            var versionsTableExists = reader.GetBoolean(1);
            await reader.CloseAsync();

            var versions = 0;
            if (versionsTableExists)
            {
                await using var versionsCommand = new NpgsqlCommand("select count(*) from barber.schema_versions", connection);
                versions = Convert.ToInt32(await versionsCommand.ExecuteScalarAsync(cancellationToken));
            }

            var ready = schemaExists && versionsTableExists && versions > 0;
            return new(ready, true, ready,
                ready ? "Banco de dados e schema BarberSync prontos." : "Execute ScriptsSQL/script_completo.sql para preparar o schema barber.",
                database, SchemaName, environment.EnvironmentName, "Validation", null, versions,
                ready ? "Healthy" : "SchemaNotReady");
        }
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException)
        {
            logger.LogWarning("Falha ao conectar ao PostgreSQL. Environment={Environment}, Database={Database}", environment.EnvironmentName, database);
            return new(false, false, false, "Não foi possível conectar ao banco de dados configurado.", database,
                SchemaName, environment.EnvironmentName, "Connection", null, 0, "Unhealthy");
        }
    }

    private static DatabaseHealthResult NotConfigured(string environmentName) =>
        new(false, false, false, DatabaseConnectionResolver.MissingConfigurationMessage, string.Empty,
            SchemaName, environmentName, "Configuration", null, 0, "NotConfigured");
}
