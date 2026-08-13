using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Api.Services.Enterprise;

public sealed partial class EnterpriseDataService
{
    public async Task<Dictionary<string, object?>> ClientCommercialBenefitsAsync(Guid clientId, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct);
        await RequireScopedEntity(connection, null, "clients", clientId, true, ct);
        const string sql = @"select jsonb_build_object(
'packages',coalesce((select jsonb_agg(jsonb_strip_nulls(jsonb_build_object('id',id::text,'status',case when status='Active' and payload->>'expiresAt' is not null and (payload->>'expiresAt')::timestamptz<=now() then 'Expired' else status end)||payload) order by created_at desc) from barber.client_packages where tenant_id=@tenant and branch_id=@branch and deleted_at is null and payload->>'clientId'=@client),'[]'::jsonb),
'memberships',coalesce((select jsonb_agg(jsonb_strip_nulls(jsonb_build_object('id',id::text,'status',case when status='Active' and payload->>'periodEnd' is not null and (payload->>'periodEnd')::timestamptz<=now() then 'Expired' else status end)||payload) order by created_at desc) from barber.client_memberships where tenant_id=@tenant and branch_id=@branch and deleted_at is null and payload->>'clientId'=@client),'[]'::jsonb))::text";
        await using var command = new NpgsqlCommand(sql, connection); command.Parameters.AddWithValue("tenant", TenantId); command.Parameters.AddWithValue("branch", BranchId); command.Parameters.AddWithValue("client", clientId.ToString());
        return Deserialize((await command.ExecuteScalarAsync(ct))?.ToString() ?? "{}");
    }
    public async Task<Dictionary<string, object?>> SellPackageAsync(Guid packageId, Guid clientId, bool paid, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var package = await LockedPayload(connection, transaction, "packages", packageId, ct);
        await RequireScopedEntity(connection, transaction, "clients", clientId, true, ct);
        var price = Decimal(package, "price");
        var services = package.GetProperty("services");
        if (price <= 0 || services.ValueKind != JsonValueKind.Array || services.GetArrayLength() == 0)
            throw new EnterpriseValidationException([new("packageId", "Pacote inválido para venda.")]);

        var balances = new Dictionary<string, decimal>();
        foreach (var service in services.EnumerateArray())
        {
            var serviceId = service.ValueKind == JsonValueKind.String ? service.GetString() : Text(service, "serviceId");
            var sessions = service.ValueKind == JsonValueKind.Object ? Decimal(service, "sessions", "quantity") : 1;
            if (!Guid.TryParse(serviceId, out _) || sessions <= 0)
                throw new EnterpriseValidationException([new("services", "Cada serviço precisa de ID e quantidade de sessões válida.")]);
            balances[serviceId!] = sessions;
        }

        var validityDays = (int)Math.Max(1, Decimal(package, "validityDays"));
        var now = DateTime.UtcNow;
        var sale = JsonSerializer.SerializeToElement(new { packageId, clientId, packageName = Text(package, "name"), price, purchasedAt = now, expiresAt = now.AddDays(validityDays), remainingByService = balances, totalSessions = balances.Values.Sum(), usedSessions = 0, paid });
        var clientPackage = await InsertWithConnectionAsync(connection, transaction, "client-packages", sale, "Active", ct);
        if (paid)
        {
            var entry = JsonSerializer.SerializeToElement(new { description = $"Venda de pacote {Text(package, "name")}", type = "Income", amount = price, category = "Pacotes", origin = "Package", clientPackageId = clientPackage["id"], clientId, confirmedAt = now });
            await InsertWithConnectionAsync(connection, transaction, "financial-entries", entry, "Paid", ct);
        }
        await transaction.CommitAsync(ct);
        await AuditAsync(connection, "Pacotes", "Sold", "client_packages", Guid.Parse(clientPackage["id"]!.ToString()!), "Pacote vendido ao cliente.", sale.GetRawText(), ct);
        return clientPackage;
    }

    public async Task<Dictionary<string, object?>> UsePackageSessionAsync(Guid clientPackageId, Guid serviceId, Guid serviceOrderId, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var sold = await LockedPayload(connection, transaction, "client_packages", clientPackageId, ct);
        var status = Text(sold, "status") ?? "Active";
        if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase)) throw Rule("clientPackageId", "Pacote cancelado ou inativo não pode ser usado.");
        if (DateTime.TryParse(Text(sold, "expiresAt"), out var expiry) && expiry <= DateTime.UtcNow) throw Rule("clientPackageId", "Pacote expirado não pode ser usado.");
        await RequireScopedEntity(connection, transaction, "service_orders", serviceOrderId, true, ct);
        var balance = sold.GetProperty("remainingByService");
        var key = serviceId.ToString();
        if (!balance.TryGetProperty(key, out var remaining) || remaining.GetDecimal() <= 0) throw Rule("serviceId", "Serviço não pertence ao pacote ou está sem saldo.");

        await using (var update = new NpgsqlCommand(@"update barber.client_packages set payload=jsonb_set(jsonb_set(payload,ARRAY['remainingByService',@service],to_jsonb((payload->'remainingByService'->>@service)::numeric-1),false),'{usedSessions}',to_jsonb(coalesce((payload->>'usedSessions')::numeric,0)+1),true),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch returning jsonb_strip_nulls(jsonb_build_object('id',id::text,'status',status)||payload)", connection, transaction))
        {
            update.Parameters.AddWithValue("service", key); update.Parameters.AddWithValue("id", clientPackageId); update.Parameters.AddWithValue("tenant", TenantId); update.Parameters.AddWithValue("branch", BranchId);
            var result = await update.ExecuteScalarAsync(ct) ?? throw new KeyNotFoundException("Pacote do cliente não encontrado.");
            await using var item = new NpgsqlCommand(@"insert into barber.service_order_items(id,tenant_id,branch_id,service_order_id,item_type,quantity,unit_price,total,status,is_active,payload) values(@item,@tenant,@branch,@order,'PackageBenefit',1,0,0,'Applied',true,@payload::jsonb)", connection, transaction);
            item.Parameters.AddWithValue("item", Guid.NewGuid()); item.Parameters.AddWithValue("tenant", TenantId); item.Parameters.AddWithValue("branch", BranchId); item.Parameters.AddWithValue("order", serviceOrderId); item.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(new { clientPackageId, serviceId, appliedAt = DateTime.UtcNow }));
            await item.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);
            await AuditAsync(connection, "Pacotes", "SessionUsed", "client_packages", clientPackageId, "Sessão aplicada na comanda.", JsonSerializer.Serialize(new { serviceId, serviceOrderId }), ct);
            return Deserialize(result.ToString()!);
        }
    }

    public Task<Dictionary<string, object?>> CancelPackageAsync(Guid id, string reason, CancellationToken ct)
        => ChangeCommercialStatus(id, "client_packages", "Pacotes", "Cancelled", reason, ct);

    public async Task<Dictionary<string, object?>> ActivateMembershipAsync(Guid membershipId, Guid clientId, bool paid, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct); await using var transaction = await connection.BeginTransactionAsync(ct);
        var plan = await LockedPayload(connection, transaction, "memberships", membershipId, ct); await RequireScopedEntity(connection, transaction, "clients", clientId, true, ct);
        var price = Decimal(plan, "monthlyPrice"); var limit = Decimal(plan, "usageLimit");
        if (price <= 0 || limit <= 0) throw Rule("membershipId", "Plano inválido para ativação.");
        var now = DateTime.UtcNow; var payload = JsonSerializer.SerializeToElement(new { membershipId, clientId, planName = Text(plan, "name"), monthlyPrice = price, usageLimit = limit, usedThisPeriod = 0, periodStart = now.Date, periodEnd = now.Date.AddMonths(1), paid });
        var membership = await InsertWithConnectionAsync(connection, transaction, "client-memberships", payload, "Active", ct);
        if (paid) await InsertWithConnectionAsync(connection, transaction, "financial-entries", JsonSerializer.SerializeToElement(new { description = $"Assinatura {Text(plan, "name")}", type = "Income", amount = price, category = "Receita recorrente", origin = "Membership", clientMembershipId = membership["id"], clientId }), "Paid", ct);
        await transaction.CommitAsync(ct); await AuditAsync(connection, "Assinaturas", "Activated", "client_memberships", Guid.Parse(membership["id"]!.ToString()!), "Assinatura ativada.", payload.GetRawText(), ct); return membership;
    }

    public async Task<Dictionary<string, object?>> UseMembershipAsync(Guid id, Guid serviceOrderId, CancellationToken ct)
    {
        await using var connection = await OpenAsync(ct); await using var transaction = await connection.BeginTransactionAsync(ct);
        var current = await LockedPayload(connection, transaction, "client_memberships", id, ct);
        if (!string.Equals(Text(current, "status") ?? "Active", "Active", StringComparison.OrdinalIgnoreCase) || DateTime.TryParse(Text(current, "periodEnd"), out var end) && end <= DateTime.UtcNow) throw Rule("membershipId", "Assinatura vencida, pausada ou cancelada.");
        if (Decimal(current, "usedThisPeriod") >= Decimal(current, "usageLimit")) throw Rule("usageLimit", "Limite mensal da assinatura atingido.");
        await RequireScopedEntity(connection, transaction, "service_orders", serviceOrderId, true, ct);
        await using var command = new NpgsqlCommand("update barber.client_memberships set payload=jsonb_set(payload,'{usedThisPeriod}',to_jsonb(coalesce((payload->>'usedThisPeriod')::numeric,0)+1),true),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch returning jsonb_strip_nulls(jsonb_build_object('id',id::text,'status',status)||payload)", connection, transaction);
        command.Parameters.AddWithValue("id", id); command.Parameters.AddWithValue("tenant", TenantId); command.Parameters.AddWithValue("branch", BranchId);
        var result = await command.ExecuteScalarAsync(ct) ?? throw new KeyNotFoundException(); await transaction.CommitAsync(ct); await AuditAsync(connection, "Assinaturas", "BenefitUsed", "client_memberships", id, "Benefício aplicado na comanda.", JsonSerializer.Serialize(new { serviceOrderId }), ct); return Deserialize(result.ToString()!);
    }

    public Task<Dictionary<string, object?>> CancelMembershipAsync(Guid id, string reason, CancellationToken ct) => ChangeCommercialStatus(id, "client_memberships", "Assinaturas", "Cancelled", reason, ct);
    public Task<Dictionary<string, object?>> PauseMembershipAsync(Guid id, string reason, CancellationToken ct) => ChangeCommercialStatus(id, "client_memberships", "Assinaturas", "Paused", reason, ct);

    public async Task<Dictionary<string, object?>> ChangePurchaseStatusAsync(Guid id, string target, string? reason, CancellationToken ct)
    {
        var allowed = target is "Approved" or "Cancelled"; if (!allowed || target == "Cancelled" && string.IsNullOrWhiteSpace(reason)) throw Rule("status", "Transição inválida ou motivo ausente.");
        return await ChangeCommercialStatus(id, "purchases", "Compras", target, reason, ct);
    }

    private async Task<Dictionary<string, object?>> ChangeCommercialStatus(Guid id, string table, string module, string status, string? reason, CancellationToken ct)
    {
        if ((status is "Cancelled" or "Paused") && string.IsNullOrWhiteSpace(reason)) throw Rule("reason", "Motivo é obrigatório.");
        await using var connection = await OpenAsync(ct); await using var command = new NpgsqlCommand($"update barber.{table} set status=@status,is_active=@active,payload=payload||@change::jsonb,updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and deleted_at is null returning jsonb_strip_nulls(jsonb_build_object('id',id::text,'status',status)||payload)", connection);
        command.Parameters.AddWithValue("status", status); command.Parameters.AddWithValue("active", status is "Active" or "Approved"); command.Parameters.AddWithValue("change", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(new { status, reason, changedAt = DateTime.UtcNow })); command.Parameters.AddWithValue("id", id); command.Parameters.AddWithValue("tenant", TenantId); command.Parameters.AddWithValue("branch", BranchId);
        var result = await command.ExecuteScalarAsync(ct) ?? throw new KeyNotFoundException("Registro não encontrado nesta unidade."); await AuditAsync(connection, module, status, table, id, $"Status alterado para {status}.", JsonSerializer.Serialize(new { reason }), ct); return Deserialize(result.ToString()!);
    }

    private async Task<JsonElement> LockedPayload(NpgsqlConnection connection, NpgsqlTransaction transaction, string table, Guid id, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand($"select (payload||jsonb_build_object('status',status))::text from barber.{table} where id=@id and tenant_id=@tenant and branch_id=@branch and deleted_at is null for update", connection, transaction);
        command.Parameters.AddWithValue("id", id); command.Parameters.AddWithValue("tenant", TenantId); command.Parameters.AddWithValue("branch", BranchId);
        var raw = await command.ExecuteScalarAsync(ct) as string ?? throw new KeyNotFoundException("Registro não encontrado nesta unidade."); return JsonSerializer.Deserialize<JsonElement>(raw);
    }
    private async Task RequireScopedEntity(NpgsqlConnection c, NpgsqlTransaction? t, string table, Guid id, bool active, CancellationToken ct) { await using var cmd = new NpgsqlCommand($"select exists(select 1 from barber.{table} where id=@id and tenant_id=@tenant and branch_id=@branch and deleted_at is null{(active ? " and is_active" : "")})", c, t); cmd.Parameters.AddWithValue("id", id); cmd.Parameters.AddWithValue("tenant", TenantId); cmd.Parameters.AddWithValue("branch", BranchId); if (await cmd.ExecuteScalarAsync(ct) is not true) throw new KeyNotFoundException("Registro relacionado não encontrado nesta unidade."); }
    private static string? Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : null;
    private static decimal Decimal(JsonElement value, params string[] names) { foreach (var name in names) if (value.TryGetProperty(name, out var item) && (item.ValueKind == JsonValueKind.Number && item.TryGetDecimal(out var number) || decimal.TryParse(item.ToString(), out number))) return number; return 0; }
    private static EnterpriseValidationException Rule(string field, string message) => new([new(field, message)]);
}
