using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using Npgsql;

namespace BarberSync.Infrastructure.Saas;

public sealed class PostgresSaasService(IDbConnectionFactory connections) : ISaasService
{
    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id,code,name,coalesce(max_users,(limits->>'maxUsers')::int,0),
              coalesce(max_professionals,(limits->>'maxProfessionals')::int,0),coalesce(max_branches,(limits->>'maxBranches')::int,0),
              coalesce((limits->>'maxMonthlyAppointments')::int,0),coalesce((features->>'aiEnabled')::boolean,false),
              coalesce((features->>'totemEnabled')::boolean,false),coalesce((features->>'advancedReports')::boolean,false),
              coalesce((features->>'whatsappNotifications')::boolean,false),coalesce((features->>'biIntegration')::boolean,false),monthly_price
            FROM barber.saas_plans WHERE is_active AND status='Active' ORDER BY name
            """;
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<SubscriptionPlanDto>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.GetInt32(3),reader.GetInt32(4),reader.GetInt32(5),reader.GetInt32(6),reader.GetBoolean(7),reader.GetBoolean(8),reader.GetBoolean(9),reader.GetBoolean(10),reader.GetBoolean(11),reader.IsDBNull(12)?null:reader.GetDecimal(12)));
        return result;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> GetSubscriptionsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id,tenant_id,plan_id,status,coalesce(starts_at,period_start::timestamptz),coalesce(ends_at,period_end::timestamptz) FROM barber.tenant_subscriptions WHERE tenant_id=@tenant ORDER BY created_at DESC";
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<SubscriptionDto>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(new(reader.GetGuid(0),reader.GetGuid(1),reader.GetGuid(2),reader.GetString(3),reader.GetDateTime(4),reader.IsDBNull(5)?null:reader.GetDateTime(5)));
        return result;
    }

    public async Task<TenantUsageDto> GetUsageAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
              (SELECT count(*) FROM barber.users WHERE tenant_id=@tenant AND is_active AND deleted_at IS NULL),
              (SELECT count(*) FROM barber.professionals WHERE tenant_id=@tenant AND is_active AND deleted_at IS NULL),
              (SELECT count(*) FROM barber.branches WHERE tenant_id=@tenant AND is_active AND deleted_at IS NULL),
              (SELECT count(*) FROM barber.appointments WHERE tenant_id=@tenant AND created_at>=date_trunc('month',now()) AND deleted_at IS NULL),
              coalesce((SELECT sum(used) FROM barber.usage_counters WHERE tenant_id=@tenant AND metric='AiRequests' AND period_start=date_trunc('month',now())::date),0),
              (SELECT count(*) FROM barber.kiosk_sessions WHERE tenant_id=@tenant AND started_at>=date_trunc('month',now()) AND deleted_at IS NULL)
            """;
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new(tenantId,Convert.ToInt32(reader.GetInt64(0)),Convert.ToInt32(reader.GetInt64(1)),Convert.ToInt32(reader.GetInt64(2)),Convert.ToInt32(reader.GetInt64(3)),Convert.ToInt32(reader.GetInt64(4)),Convert.ToInt32(reader.GetInt64(5)));
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id,tenant_id,total,status,due_at FROM barber.billing_invoices WHERE tenant_id=@tenant ORDER BY created_at DESC LIMIT 200";
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<InvoiceDto>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(new(reader.GetGuid(0),reader.GetGuid(1),reader.GetDecimal(2),reader.GetString(3),reader.IsDBNull(4)?DateTime.MinValue:reader.GetDateTime(4)));
        return result;
    }
}
