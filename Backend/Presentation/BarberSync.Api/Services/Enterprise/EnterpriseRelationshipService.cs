using System.Text.Json;
using Npgsql;

namespace BarberSync.Api.Services.Enterprise;

public sealed partial class EnterpriseDataService
{
    public async Task<Dictionary<string, object?>> UpsertClientProfileAsync(Guid clientId, JsonElement payload, CancellationToken ct)
    {
        if (await GetAsync("clients", clientId, ct) is null)
            throw new EnterpriseValidationException([new("clientId", "Cliente não encontrado neste tenant e unidade.")]);
        await using var db = await OpenAsync(ct);
        const string sql = @"insert into barber.client_profiles(id,tenant_id,branch_id,client_id,birth_date,gender,phone,email,notes,preferences_json)
values(gen_random_uuid(),@tenantScope,@branchScope,@client,@birthDate,@gender,@phone,@email,@notes,@preferences::jsonb)
on conflict(tenant_id,branch_id,client_id) where deleted_at is null do update set birth_date=excluded.birth_date,gender=excluded.gender,phone=excluded.phone,email=excluded.email,notes=excluded.notes,preferences_json=excluded.preferences_json,updated_at=now()
returning jsonb_strip_nulls(to_jsonb(client_profiles))";
        string? Text(string name) => payload.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() : null;
        await using var command = new NpgsqlCommand(sql, db); AddScope(command); command.Parameters.AddWithValue("client", clientId);
        object birthDate = Text("birthDate") is { } birth && DateOnly.TryParse(birth, out var date) ? date : DBNull.Value;
        command.Parameters.AddWithValue("birthDate", birthDate);
        command.Parameters.AddWithValue("gender", (object?)Text("gender") ?? DBNull.Value); command.Parameters.AddWithValue("phone", (object?)Text("phone") ?? DBNull.Value);
        command.Parameters.AddWithValue("email", (object?)Text("email") ?? DBNull.Value); command.Parameters.AddWithValue("notes", (object?)Text("notes") ?? DBNull.Value);
        command.Parameters.AddWithValue("preferences", payload.TryGetProperty("preferences", out var preferences) ? preferences.GetRawText() : "{}");
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(Convert.ToString(await command.ExecuteScalarAsync(ct))!, JsonOptions)!;
    }

    public async Task<Dictionary<string, object?>> RelationshipDashboardAsync(CancellationToken ct)
    {
        await using var db = await OpenAsync(ct);
        async Task<decimal> Scalar(string sql)
        {
            await using var command = new NpgsqlCommand(sql, db);
            AddScope(command);
            return Convert.ToDecimal(await command.ExecuteScalarAsync(ct));
        }

        var revenue = await Scalar("select coalesce(sum(amount),0) from barber.payments where tenant_id=@tenantScope and branch_id=@branchScope and status='Paid'");
        var clients = await Scalar("select count(*) from barber.clients where tenant_id=@tenantScope and branch_id=@branchScope and deleted_at is null");
        return new Dictionary<string, object?>
        {
            ["activeClients"] = await Scalar("select count(distinct client_id) from barber.service_orders where tenant_id=@tenantScope and branch_id=@branchScope and created_at>=now()-interval '30 days'"),
            ["recurringClients"] = await Scalar("select count(*) from (select client_id from barber.service_orders where tenant_id=@tenantScope and branch_id=@branchScope group by client_id having count(*)>1) x"),
            ["inactiveClients"] = await Scalar("select count(*) from barber.clients c where c.tenant_id=@tenantScope and c.branch_id=@branchScope and c.deleted_at is null and not exists(select 1 from barber.service_orders o where o.tenant_id=c.tenant_id and o.branch_id=c.branch_id and o.client_id=c.id and o.created_at>=now()-interval '60 days')"),
            ["birthdaysThisMonth"] = await Scalar("select count(*) from barber.client_profiles where tenant_id=@tenantScope and branch_id=@branchScope and deleted_at is null and extract(month from birth_date)=extract(month from current_date)"),
            ["churnRiskClients"] = await Scalar("select count(*) from barber.client_profiles where tenant_id=@tenantScope and branch_id=@branchScope and deleted_at is null and last_visit_at<now()-interval '30 days'"),
            ["revenuePerClient"] = clients == 0 ? 0 : revenue / clients,
            ["averageTicket"] = await Scalar("select coalesce(avg(amount),0) from barber.payments where tenant_id=@tenantScope and branch_id=@branchScope and status='Paid'"),
            ["packagesSold"] = await Scalar("select count(*) from barber.client_packages where tenant_id=@tenantScope and branch_id=@branchScope and deleted_at is null"),
            ["couponsUsed"] = await Scalar("select count(*) from barber.coupon_redemptions where tenant_id=@tenantScope and branch_id=@branchScope"),
            ["pointsBalance"] = await Scalar("select coalesce(sum(points_balance),0) from barber.loyalty_accounts where tenant_id=@tenantScope and branch_id=@branchScope and deleted_at is null"),
            ["cashbackBalance"] = await Scalar("select coalesce(sum(cashback_balance),0) from barber.loyalty_accounts where tenant_id=@tenantScope and branch_id=@branchScope and deleted_at is null"),
            ["recentCampaigns"] = (await ListAsync("campaigns", ct)).Take(5).ToArray()
        };
    }

    public async Task<Dictionary<string, object?>?> ClientRelationshipAsync(Guid clientId, CancellationToken ct)
    {
        var client = await GetAsync("clients", clientId, ct);
        if (client is null) return null;
        await using var db = await OpenAsync(ct);
        const string sql = @"select jsonb_build_object(
'profile',coalesce((select to_jsonb(p) from barber.client_profiles p where p.tenant_id=@tenantScope and p.branch_id=@branchScope and p.client_id=@client and p.deleted_at is null),'{}'::jsonb),
'packages',coalesce((select jsonb_agg(to_jsonb(p)) from barber.client_packages p where p.tenant_id=@tenantScope and p.branch_id=@branchScope and p.payload->>'clientId'=@clientText and p.deleted_at is null),'[]'::jsonb),
'loyalty',coalesce((select to_jsonb(l) from barber.loyalty_accounts l where l.tenant_id=@tenantScope and l.branch_id=@branchScope and l.client_id=@client and l.deleted_at is null),'{}'::jsonb),
'couponRedemptions',coalesce((select jsonb_agg(to_jsonb(r)) from barber.coupon_redemptions r where r.tenant_id=@tenantScope and r.branch_id=@branchScope and r.client_id=@client),'[]'::jsonb))";
        await using var command = new NpgsqlCommand(sql, db);
        AddScope(command); command.Parameters.AddWithValue("client", clientId); command.Parameters.AddWithValue("clientText", clientId.ToString());
        var relationship = JsonSerializer.Deserialize<Dictionary<string, object?>>(Convert.ToString(await command.ExecuteScalarAsync(ct))!, JsonOptions)!;
        relationship["client"] = client;
        return relationship;
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> ClientTimelineAsync(Guid clientId, CancellationToken ct)
    {
        await using var db = await OpenAsync(ct);
        const string sql = @"select jsonb_build_object('type',type,'occurredAt',occurred_at,'title',title,'amount',amount,'status',status) from (
select 'ServiceOrder' type,created_at occurred_at,number title,total amount,status from barber.service_orders where tenant_id=@tenantScope and branch_id=@branchScope and client_id=@client
union all select 'Appointment',created_at,coalesce(payload->>'serviceName','Agendamento'),null,status from barber.appointments where tenant_id=@tenantScope and branch_id=@branchScope and client_id=@client
union all select 'Loyalty',created_at,description,cashback_delta,type from barber.loyalty_transactions where tenant_id=@tenantScope and branch_id=@branchScope and client_id=@client) timeline order by occurred_at desc limit 100";
        await using var command = new NpgsqlCommand(sql, db); AddScope(command); command.Parameters.AddWithValue("client", clientId); command.Parameters.AddWithValue("clientText", clientId.ToString());
        var rows = new List<Dictionary<string, object?>>(); await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) rows.Add(JsonSerializer.Deserialize<Dictionary<string, object?>>(reader.GetString(0), JsonOptions)!);
        return rows;
    }

    public static IReadOnlyList<object> RelationshipSegments() =>
    [
        new { key="new", name="Clientes novos" }, new { key="recurring", name="Clientes recorrentes" },
        new { key="inactive-30", name="Inativos há 30 dias" }, new { key="inactive-60", name="Inativos há 60 dias" },
        new { key="birthdays", name="Aniversariantes do mês" }, new { key="no-show", name="No-show recorrente" },
        new { key="vip", name="VIP por gasto" }, new { key="active-package", name="Pacote ativo" },
        new { key="cashback", name="Saldo de cashback" }
    ];

    public async Task<IReadOnlyList<Dictionary<string, object?>>> SegmentClientsAsync(string key, CancellationToken ct)
    {
        var predicate = key switch
        {
            "new" => "p.created_at>=now()-interval '30 days'", "recurring" => "p.visit_count>1",
            "inactive-30" => "p.last_visit_at<now()-interval '30 days'", "inactive-60" => "p.last_visit_at<now()-interval '60 days'",
            "birthdays" => "extract(month from p.birth_date)=extract(month from current_date)", "no-show" => "p.no_show_count>=2",
            "vip" => "p.total_spent>=1000", "active-package" => "exists(select 1 from barber.client_packages cp where cp.tenant_id=p.tenant_id and cp.branch_id=p.branch_id and cp.payload->>'clientId'=p.client_id::text and cp.status='Active' and cp.deleted_at is null)",
            "cashback" => "exists(select 1 from barber.loyalty_accounts l where l.tenant_id=p.tenant_id and l.branch_id=p.branch_id and l.client_id=p.client_id and l.cashback_balance>0 and l.deleted_at is null)",
            _ => throw new EnterpriseValidationException([new("key", "Segmento desconhecido.")])
        };
        await using var db = await OpenAsync(ct);
        var sql = $"select jsonb_build_object('id',c.id,'name',c.name,'email',p.email,'phone',p.phone,'lastVisitAt',p.last_visit_at,'totalSpent',p.total_spent) from barber.client_profiles p join barber.clients c on c.id=p.client_id and c.tenant_id=p.tenant_id where p.tenant_id=@tenantScope and p.branch_id=@branchScope and p.deleted_at is null and {predicate} order by p.total_spent desc";
        await using var command = new NpgsqlCommand(sql, db); AddScope(command); var rows = new List<Dictionary<string, object?>>(); await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) rows.Add(JsonSerializer.Deserialize<Dictionary<string, object?>>(reader.GetString(0), JsonOptions)!); return rows;
    }
}
