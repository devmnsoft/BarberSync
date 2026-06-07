namespace BarberSync.Api.Services.Enterprise;

public interface IBarberSchemaInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);
    DatabaseHealthResult LastResult { get; }
}

public sealed record DatabaseHealthResult(
    bool Success,
    bool DatabaseConnected,
    bool SchemaReady,
    string Message,
    string Database,
    string Schema,
    string Environment,
    string? Step = null,
    string? Error = null);
