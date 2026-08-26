using System.Text.Json;
using BarberSync.Application.Abstractions;
using Npgsql;

namespace BarberSync.Api.Services.Governance;

public interface IPlanLimitService
{
    Task<PlanLimitResult> CheckAsync(string resource, CancellationToken cancellationToken);
}

public sealed record PlanLimitResult(bool Allowed, int Current, int? Limit, string Message)
{
    public static PlanLimitResult Unlimited(int current) => new(true, current, null, string.Empty);
}

public sealed class PlanLimitService(IConfiguration configuration, ICurrentUserContext user) : IPlanLimitService
{
    public async Task<PlanLimitResult> CheckAsync(string resource, CancellationToken ct)
    {
        var (table, column) = resource switch
        {
            "branches" => ("branches", "max_branches"),
            "users" => ("users", "max_users"),
            "professionals" => ("professionals", "max_professionals"),
            "clients" => ("clients", "max_clients"),
            _ => throw new ArgumentOutOfRangeException(nameof(resource))
        };
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync(ct);
        var sql = $"""select (select count(*)::int from barber.{table} where tenant_id=@tenant and deleted_at is null), p.{column} from barber.tenant_subscriptions s join barber.saas_plans p on p.id=s.plan_id where s.tenant_id=@tenant and s.status in ('Trial','Active') order by s.created_at desc limit 1""";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", user.TenantId);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) throw new InvalidOperationException("Assinatura ativa não configurada para o tenant.");
        var current = reader.GetInt32(0);
        if (reader.IsDBNull(1)) return PlanLimitResult.Unlimited(current);
        var limit = reader.GetInt32(1);
        return new(current < limit, current, limit, $"O plano atual permite até {limit} {resource}.");
    }
}

public sealed class GovernanceStore(IConfiguration configuration, ICurrentUserContext user)
{
    private string ConnectionString => configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");
    public async Task<List<Dictionary<string, object?>>> QueryAsync(string sql, Action<NpgsqlParameterCollection>? parameters, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString); await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection); command.Parameters.AddWithValue("tenant", user.TenantId); command.Parameters.AddWithValue("branch", user.BranchId); parameters?.Invoke(command.Parameters);
        var result = new List<Dictionary<string, object?>>(); await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) { var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase); for (var i=0;i<reader.FieldCount;i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i); result.Add(row); }
        return result;
    }
    public async Task<Guid> ExecuteInsertAsync(string sql, Action<NpgsqlParameterCollection> parameters, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString); await connection.OpenAsync(ct); await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", user.TenantId); command.Parameters.AddWithValue("branch", user.BranchId); command.Parameters.AddWithValue("user", user.UserId); parameters(command.Parameters);
        return (Guid)(await command.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("Operação de governança não persistida."));
    }
    public async Task<int> ExecuteAsync(string sql, Action<NpgsqlParameterCollection> parameters, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString); await connection.OpenAsync(ct); await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", user.TenantId); command.Parameters.AddWithValue("branch", user.BranchId); command.Parameters.AddWithValue("user", user.UserId); parameters(command.Parameters); return await command.ExecuteNonQueryAsync(ct);
    }
    public static string Json(JsonElement value) => value.ValueKind == JsonValueKind.Undefined ? "{}" : value.GetRawText();
}
