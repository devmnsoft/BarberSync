using Npgsql;

namespace BarberSync.Api.Services.Enterprise;

public sealed class BarberSchemaInitializer(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<BarberSchemaInitializer> logger) : IBarberSchemaInitializer
{
    private const string SchemaName = "barber";
    private static readonly SemaphoreSlim ProcessLock = new(1, 1);
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection não foi configurada. Use User Secrets ou BARBERSYNC_ConnectionStrings__DefaultConnection.");
    private bool _initialized;

    public DatabaseHealthResult LastResult { get; private set; } = new(false, false, false,
        "Schema BarberSync ainda não inicializado.", string.Empty, SchemaName, "Unknown");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;
        await ProcessLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;
            var scriptPath = ResolveScriptPath();
            var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            if (string.IsNullOrWhiteSpace(script))
                throw new InvalidOperationException($"O script oficial está vazio: {scriptPath}");

            var info = ConnectionInfo.Create(_connectionString);
            logger.LogInformation("Iniciando atualização do schema por {Script}. Database={Database}", scriptPath, info.Database);
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand(script, connection) { CommandTimeout = 300 };
            await command.ExecuteNonQueryAsync(cancellationToken);

            _initialized = true;
            LastResult = new(true, true, true, "Schema BarberSync atualizado pelo script oficial.",
                info.Database, SchemaName, environment.EnvironmentName, "Completed");
            logger.LogInformation("Schema BarberSync atualizado com sucesso. Database={Database}, Schema={Schema}", info.Database, SchemaName);
        }
        catch (Exception exception)
        {
            var info = ConnectionInfo.Create(_connectionString);
            LastResult = new(false, false, false, $"Falha crítica ao atualizar o schema: {exception.Message}",
                info.Database, SchemaName, environment.EnvironmentName, "script_completo.sql", exception.ToString());
            logger.LogCritical(exception, "Startup abortado: não foi possível aplicar ScriptsSQL/script_completo.sql em {Database}.", info.Database);
            throw new InvalidOperationException("Não foi possível inicializar o banco BarberSync. Consulte o log da atualização do schema.", exception);
        }
        finally
        {
            ProcessLock.Release();
        }
    }

    public async Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized) await InitializeAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            "select exists(select 1 from barber.schema_versions where version = '007')", connection);
        var ready = Convert.ToBoolean(await command.ExecuteScalarAsync(cancellationToken));
        LastResult = LastResult with { Success = ready, DatabaseConnected = true, SchemaReady = ready, Step = "HealthCheck" };
        return LastResult;
    }

    private static string ResolveScriptPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "ScriptsSQL", "script_completo.sql"),
            Path.Combine(Directory.GetCurrentDirectory(), "ScriptsSQL", "script_completo.sql")
        };
        return candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("ScriptsSQL/script_completo.sql não foi copiado para o output da API.");
    }

    private sealed record ConnectionInfo(string Database)
    {
        public static ConnectionInfo Create(string value) => new(new NpgsqlConnectionStringBuilder(value).Database ?? string.Empty);
    }
}
