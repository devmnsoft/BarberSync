using System.Data.Common;
using BarberSync.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BarberSync.Infrastructure.Persistence;

public sealed class PostgresConnectionFactory(IConfiguration configuration, ILogger<PostgresConnectionFactory> logger) : IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada.");

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            logger.LogDebug("Conexão PostgreSQL aberta. Database={Database}", connection.Database);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
