using Npgsql;

namespace BarberSync.Api.Services.Growth;

public sealed class GrowthService(IConfiguration configuration) : IAssistantInsightService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada.");

    public async Task<Dictionary<string, object?>> Client360Async(Guid tenantId, Guid clientId, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct);
        const string sql = "select barber.client_360(@tenant,@client)::text";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant", tenantId); command.Parameters.AddWithValue("client", clientId);
        var result = await command.ExecuteScalarAsync(ct) as string;
        if (result is null) throw new KeyNotFoundException("Cliente não encontrado.");
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(result)!;
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> ReactivationAsync(Guid tenantId, Guid branchId, int days, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct);
        const string sql = "select row_to_json(r)::text from barber.vw_clients_to_reactivate r where tenant_id=@tenant and branch_id=@branch and days_without_visit>=@days order by historical_spend desc";
        return await ReadJsonAsync(connection, sql, ct, ("tenant", tenantId), ("branch", branchId), ("days", Math.Clamp(days, 30, 365)));
    }

    public async Task<Dictionary<string, object?>> PreviewAudienceAsync(Guid tenantId, Guid branchId, AudienceFilter filter, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct);
        const string sql = @"select row_to_json(x)::text from (select c.id,c.payload->>'name' name,c.payload->>'phone' phone,
coalesce(sum(p.amount),0) historical_spend,coalesce(avg(p.amount),0) average_ticket
from barber.clients c left join barber.service_orders so on so.client_id=c.id left join barber.payments p on p.service_order_id=so.id and p.status='Paid'
where c.tenant_id=@tenant and c.branch_id=@branch and c.deleted_at is null
and (@status='' or c.status=@status)
and (@inactive=0 or not exists(select 1 from barber.appointments a where a.client_id=c.id and a.status='Finished' and a.scheduled_start>=now()-(@inactive||' days')::interval))
group by c.id order by historical_spend desc limit 500) x";
        var clients = await ReadJsonAsync(connection, sql, ct, ("tenant", tenantId), ("branch", branchId), ("status", filter.ClientStatus ?? ""), ("inactive", Math.Max(filter.InactiveDays ?? 0, 0)));
        var potential = clients.Sum(x => x.TryGetValue("average_ticket", out var v) && decimal.TryParse(v?.ToString(), out var n) ? n : 0);
        return new() { ["eligibleClients"] = clients, ["count"] = clients.Count, ["estimatedPotentialRevenue"] = potential, ["currency"] = "BRL" };
    }

    public async Task<IReadOnlyList<AssistantInsight>> GetDashboardAsync(Guid tenantId, Guid branchId, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct); var insights = new List<AssistantInsight>();
        async Task<int> Count(string sql) { await using var q = new NpgsqlCommand(sql,c); q.Parameters.AddWithValue("tenant",tenantId); q.Parameters.AddWithValue("branch",branchId); return Convert.ToInt32(await q.ExecuteScalarAsync(ct)); }
        var inactive = await Count("select count(*) from barber.vw_clients_to_reactivate where tenant_id=@tenant and branch_id=@branch and days_without_visit>=45");
        var stock = await Count("select count(*) from barber.products where tenant_id=@tenant and branch_id=@branch and deleted_at is null and current_stock<=minimum_stock");
        var cashback = await Count("select count(*) from barber.loyalty_accounts where tenant_id=@tenant and branch_id=@branch and points>=30 and deleted_at is null");
        if (inactive > 0) insights.Add(new("Retention","High",$"{inactive} clientes não retornam há mais de 45 dias.","/Admin/Reactivation"));
        if (stock > 0) insights.Add(new("Stock","Critical",$"{stock} produtos estão abaixo do estoque mínimo.","/Admin/Stock"));
        if (cashback > 0) insights.Add(new("Loyalty","Medium",$"{cashback} clientes têm saldo de fidelidade acima de R$ 30.","/Admin/Loyalty"));
        return insights;
    }

    public async Task<IReadOnlyList<AssistantInsight>> GetClientAsync(Guid tenantId, Guid clientId, CancellationToken ct)
    {
        var client = await Client360Async(tenantId, clientId, ct); var result = new List<AssistantInsight>();
        var days = Convert.ToInt32(client.GetValueOrDefault("daysWithoutVisit")?.ToString() ?? "0");
        if (days >= 30) result.Add(new("Retention", days >= 60 ? "High" : "Medium", $"Cliente está há {days} dias sem retornar.", $"/Admin/Appointments?clientId={clientId}"));
        if (client.GetValueOrDefault("recommendedReturnAt") is { } date) result.Add(new("Return","Medium",$"Retorno recomendado para {date}.", $"/Admin/Appointments?clientId={clientId}"));
        return result;
    }

    private async Task<NpgsqlConnection> OpenAsync(CancellationToken ct) { var c=new NpgsqlConnection(_connectionString); await c.OpenAsync(ct); return c; }
    private static async Task<IReadOnlyList<Dictionary<string, object?>>> ReadJsonAsync(NpgsqlConnection c,string sql,CancellationToken ct,params (string,object)[] args)
    { await using var q=new NpgsqlCommand(sql,c); foreach(var (n,v) in args) q.Parameters.AddWithValue(n,v); var list=new List<Dictionary<string,object?>>(); await using var r=await q.ExecuteReaderAsync(ct); while(await r.ReadAsync(ct)) list.Add(System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,object?>>(r.GetString(0))!); return list; }
}

public sealed record AudienceFilter(string? ClientStatus, int? InactiveDays, Guid? ServiceId, Guid? ProfessionalId, DateTimeOffset? From, DateTimeOffset? To);
