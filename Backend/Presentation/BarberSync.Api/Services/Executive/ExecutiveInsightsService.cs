using System.Security.Claims;
using Npgsql;

namespace BarberSync.Api.Services.Executive;

/// <summary>Read-only executive projections. Every statement is scoped by the authenticated JWT.</summary>
public sealed class ExecutiveInsightsService(IConfiguration configuration, IHttpContextAccessor accessor)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada.");

    private (Guid Tenant, Guid Branch) Scope()
    {
        var user = accessor.HttpContext?.User ?? throw new UnauthorizedAccessException();
        if (!Guid.TryParse(user.FindFirstValue("tenant_id"), out var tenant) ||
            !Guid.TryParse(user.FindFirstValue("branch_id"), out var branch))
            throw new UnauthorizedAccessException("Claims tenant_id e branch_id são obrigatórias.");
        return (tenant, branch);
    }

    private async Task<NpgsqlConnection> Open(CancellationToken ct)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }

    private static void AddScope(NpgsqlCommand command, (Guid Tenant, Guid Branch) scope)
    {
        command.Parameters.AddWithValue("tenant", scope.Tenant);
        command.Parameters.AddWithValue("branch", scope.Branch);
    }

    private static async Task<decimal> Scalar(NpgsqlConnection connection, (Guid Tenant, Guid Branch) scope, string sql, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(sql, connection); AddScope(command, scope);
        return Convert.ToDecimal(await command.ExecuteScalarAsync(ct) ?? 0);
    }

    public Task<object> OwnerAsync(CancellationToken ct) => OwnerAsync(null, null, ct);

    public async Task<object> OwnerAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        if (from.HasValue != to.HasValue) throw new ArgumentException("Informe as datas inicial e final do período.");
        if (from > to) throw new ArgumentException("A data inicial não pode ser posterior à data final.");
        var scope = Scope(); await using var db = await Open(ct);
        const string paid = "tenant_id=@tenant and branch_id=@branch and deleted_at is null and status in ('Paid','Approved','Completed')";
        var start = from?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = to?.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? DateTime.UtcNow;
        async Task<decimal> PeriodScalar(string sql)
        {
            await using var command = new NpgsqlCommand(sql, db); AddScope(command, scope);
            command.Parameters.AddWithValue("from", start); command.Parameters.AddWithValue("to", end);
            return Convert.ToDecimal(await command.ExecuteScalarAsync(ct) ?? 0);
        }
        var today = await Scalar(db, scope, $"select coalesce(sum(amount),0) from barber.payments where {paid} and coalesce(paid_at,created_at)::date=current_date", ct);
        var month = await PeriodScalar($"select coalesce(sum(amount),0) from barber.payments where {paid} and coalesce(paid_at,created_at)>=@from and coalesce(paid_at,created_at)<@to");
        var sales = await PeriodScalar($"select count(*) from barber.payments where {paid} and coalesce(paid_at,created_at)>=@from and coalesce(paid_at,created_at)<@to");
        return new
        {
            period = new { from = start, to = end },
            metrics = new Dictionary<string, decimal>
            {
                ["revenueToday"] = today, ["revenueMonth"] = month, ["averageTicket"] = sales == 0 ? 0 : month / sales,
                ["newClients"] = await Scalar(db, scope, "select count(*) from barber.clients where tenant_id=@tenant and branch_id=@branch and deleted_at is null and created_at>=date_trunc('month',now())", ct),
                ["recurringClients"] = await Scalar(db, scope, "select count(*) from (select client_id from barber.appointments where tenant_id=@tenant and branch_id=@branch and deleted_at is null group by client_id having count(*)>1) x", ct),
                ["pendingCommissions"] = await Scalar(db, scope, "select coalesce(sum(amount),0) from barber.commissions where tenant_id=@tenant and branch_id=@branch and status='Pending'", ct),
                ["purchasesAwaitingReceipt"] = await Scalar(db, scope, "select count(*) from barber.purchases where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status in ('Open','Approved','PartiallyReceived')", ct),
                ["overduePayables"] = await Scalar(db, scope, "select count(*) from barber.financial_entries where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status='Pending' and payload->>'type'='Expense' and payload->>'dueAt' ~ '^\\d{4}-\\d{2}-\\d{2}' and (payload->>'dueAt')::timestamptz<now()", ct),
                ["packagesSold"] = await Scalar(db, scope, "select count(*) from barber.client_packages where tenant_id=@tenant and branch_id=@branch and deleted_at is null and created_at>=date_trunc('month',now())", ct),
                ["activeSubscriptions"] = await Scalar(db, scope, "select count(*) from barber.client_memberships where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status='Active'", ct),
                ["recurringRevenue"] = await Scalar(db, scope, "select coalesce(sum((payload->>'monthlyPrice')::numeric),0) from barber.client_memberships where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status='Active' and payload->>'monthlyPrice' ~ '^[0-9]+(\\.[0-9]+)?$'", ct),
                ["criticalStock"] = await Scalar(db, scope, "select count(*) from barber.products where tenant_id=@tenant and branch_id=@branch and deleted_at is null and is_active and current_stock<=minimum_stock", ct),
                ["noShow"] = await Scalar(db, scope, "select count(*) from barber.appointments where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status='NoShow' and created_at>=date_trunc('month',now())", ct),
                ["cashDifference"] = await Scalar(db, scope, "select coalesce(sum(abs(actual_balance-expected_balance)),0) from barber.cash_registers where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status='Closed' and closed_at>=date_trunc('month',now())", ct)
            },
            isDemo = false
        };
    }

    public async Task<object> ReceptionAsync(CancellationToken ct)
    {
        var scope = Scope(); await using var db = await Open(ct);
        var metrics = new Dictionary<string, decimal>
        {
            ["todayAppointments"] = await Scalar(db, scope, "select count(*) from barber.appointments where tenant_id=@tenant and branch_id=@branch and deleted_at is null and coalesce(scheduled_start,created_at)::date=current_date", ct),
            ["waiting"] = await Scalar(db, scope, "select count(*) from barber.appointments where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status in ('CheckedIn','Waiting')", ct),
            ["late"] = await Scalar(db, scope, "select count(*) from barber.appointments where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status in ('Scheduled','Confirmed') and scheduled_start<now()", ct),
            ["kioskCheckins"] = await Scalar(db, scope, "select count(*) from barber.kiosk_sessions where tenant_id=@tenant and branch_id=@branch and deleted_at is null and created_at::date=current_date", ct),
            ["openOrders"] = await Scalar(db, scope, "select count(*) from barber.service_orders where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status in ('Open','Payment')", ct),
            ["expiringPackages"] = await Scalar(db, scope, "select count(*) from barber.client_packages where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status='Active' and payload->>'expiresAt' ~ '^\\d{4}-\\d{2}-\\d{2}' and (payload->>'expiresAt')::timestamptz<now()+interval '30 days'", ct),
            ["expiredSubscriptions"] = await Scalar(db, scope, "select count(*) from barber.client_memberships where tenant_id=@tenant and branch_id=@branch and deleted_at is null and status in ('Expired','PastDue')", ct),
            ["inactiveClients"] = await Scalar(db, scope, "select count(*) from barber.clients c where c.tenant_id=@tenant and c.branch_id=@branch and c.deleted_at is null and not exists(select 1 from barber.appointments a where a.client_id=c.id and a.created_at>now()-interval '60 days')", ct)
        };
        return new { metrics, isDemo = false };
    }

    public async Task AuditAsync(string operation, string description, CancellationToken ct)
    {
        var scope = Scope(); await using var db = await Open(ct);
        await using var command = new NpgsqlCommand("insert into barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,module,action,description,correlation_id) values(gen_random_uuid(),@tenant,@branch,@user,@operation,'executive_report','Relatórios',@operation,@description,@trace)", db);
        AddScope(command, scope); command.Parameters.AddWithValue("operation", operation); command.Parameters.AddWithValue("description", description);
        command.Parameters.AddWithValue("user", NpgsqlTypes.NpgsqlDbType.Uuid, Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? accessor.HttpContext?.User.FindFirstValue("sub"), out var userId) ? userId : DBNull.Value);
        command.Parameters.AddWithValue("trace", accessor.HttpContext?.TraceIdentifier ?? ""); await command.ExecuteNonQueryAsync(ct);
    }
}
