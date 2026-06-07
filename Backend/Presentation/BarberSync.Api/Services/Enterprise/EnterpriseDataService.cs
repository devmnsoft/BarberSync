using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Api.Services.Enterprise;

public sealed class EnterpriseDataService(IConfiguration configuration, ILogger<EnterpriseDataService> logger)
{
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);
    private static bool _schemaReady;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=barber;Username=barbersync;Password=barbersync_demo_10";

    private static readonly IReadOnlyDictionary<string, string> Tables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["clients"] = "clients",
        ["professionals"] = "professionals",
        ["services"] = "services",
        ["products"] = "products",
        ["appointments"] = "appointments",
        ["service-orders"] = "service_orders",
        ["service_order_items"] = "service_order_items",
        ["payments"] = "payments",
        ["campaigns"] = "campaigns",
        ["coupons"] = "coupons",
        ["loyalty"] = "loyalty_accounts",
        ["reviews"] = "reviews",
        ["public_leads"] = "public_leads",
        ["kiosk_devices"] = "kiosk_devices",
        ["kiosk_sessions"] = "kiosk_sessions",
        ["notifications"] = "notifications",
        ["audit_logs"] = "audit_logs",
        ["stock_movements"] = "stock_movements"
    };

    private Guid TenantId => Guid.Parse(configuration["BarberSync:DefaultTenantId"] ?? "11111111-1111-1111-1111-111111111111");
    private Guid BranchId => Guid.Parse(configuration["BarberSync:DefaultBranchId"] ?? "22222222-2222-2222-2222-222222222222");

    public async Task<IReadOnlyList<Dictionary<string, object?>>> ListAsync(string resource, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        var sql = $"select jsonb_strip_nulls(jsonb_build_object('id', id::text, 'tenantId', tenant_id::text, 'branchId', branch_id::text, 'status', status, 'isActive', is_active, 'createdAt', created_at, 'updatedAt', updated_at) || payload) from barber.{Table(resource)} where deleted_at is null order by created_at desc";
        await using var command = new NpgsqlCommand(sql, connection);
        var items = new List<Dictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(Deserialize(reader.GetString(0)));
        }
        return items;
    }

    public async Task<Dictionary<string, object?>?> GetAsync(string resource, Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        var sql = $"select jsonb_strip_nulls(jsonb_build_object('id', id::text, 'tenantId', tenant_id::text, 'branchId', branch_id::text, 'status', status, 'isActive', is_active, 'createdAt', created_at, 'updatedAt', updated_at) || payload) from barber.{Table(resource)} where id = @id and deleted_at is null";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is string json ? Deserialize(json) : null;
    }

    public async Task<Dictionary<string, object?>> CreateAsync(string resource, JsonElement payload, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        var validation = Validate(resource, payload, null);
        if (validation.Count > 0) throw new EnterpriseValidationException(validation);

        var id = Guid.NewGuid();
        var json = MergeId(payload, id);
        await using var connection = await OpenAsync(cancellationToken);
        var status = ExtractString(payload, "status") ?? "Active";
        var sql = $"insert into barber.{Table(resource)} (id, tenant_id, branch_id, payload, status) values (@id, @tenantId, @branchId, @payload::jsonb, @status) returning jsonb_strip_nulls(jsonb_build_object('id', id::text, 'tenantId', tenant_id::text, 'branchId', branch_id::text, 'status', status, 'isActive', is_active, 'createdAt', created_at) || payload)";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantId", TenantId);
        command.Parameters.AddWithValue("branchId", BranchId);
        command.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, json);
        command.Parameters.AddWithValue("status", status);
        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);
            await AuditAsync(connection, Module(resource), "Created", Table(resource), id, $"{Module(resource)} criado via API real.", json, cancellationToken);
            await NotifyOnCreateAsync(connection, resource, id, json, cancellationToken);
            return Deserialize(result?.ToString() ?? json);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new EnterpriseValidationException([new("document", "CPF/CNPJ já cadastrado para este tenant.")]);
        }
    }

    public async Task<Dictionary<string, object?>> UpdateAsync(string resource, Guid id, JsonElement payload, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        var validation = Validate(resource, payload, id);
        if (validation.Count > 0) throw new EnterpriseValidationException(validation);

        var json = MergeId(payload, id);
        await using var connection = await OpenAsync(cancellationToken);
        var status = ExtractString(payload, "status") ?? "Active";
        var sql = $"insert into barber.{Table(resource)} (id, tenant_id, branch_id, payload, status) values (@id, @tenantId, @branchId, @payload::jsonb, @status) on conflict (id) do update set payload = excluded.payload, status = excluded.status, updated_at = now() returning jsonb_strip_nulls(jsonb_build_object('id', id::text, 'tenantId', tenant_id::text, 'branchId', branch_id::text, 'status', status, 'isActive', is_active, 'createdAt', created_at, 'updatedAt', updated_at) || payload)";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantId", TenantId);
        command.Parameters.AddWithValue("branchId", BranchId);
        command.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, json);
        command.Parameters.AddWithValue("status", status);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        await AuditAsync(connection, Module(resource), "Updated", Table(resource), id, $"{Module(resource)} atualizado via API real.", json, cancellationToken);
        return Deserialize(result?.ToString() ?? json);
    }

    public async Task SoftDeleteAsync(string resource, Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        var sql = $"update barber.{Table(resource)} set deleted_at = now(), is_active = false, status = 'Inactive', updated_at = now() where id = @id and deleted_at is null";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
        await AuditAsync(connection, Module(resource), "SoftDeleted", Table(resource), id, $"{Module(resource)} inativado via soft delete.", "{}", cancellationToken);
    }

    public async Task<Dictionary<string, object?>> ChangeAppointmentStatusAsync(Guid id, string targetStatus, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        var appointment = await GetAsync("appointments", id, cancellationToken) ?? throw new KeyNotFoundException("Agendamento não encontrado.");
        var current = appointment.TryGetValue("status", out var value) ? value?.ToString() ?? "Scheduled" : "Scheduled";
        if (!CanTransition(current, targetStatus))
        {
            throw new EnterpriseValidationException([new("status", $"Transição inválida: {current} -> {targetStatus}.")]);
        }

        appointment["status"] = targetStatus;
        var json = JsonSerializer.Serialize(appointment, JsonOptions);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("update barber.appointments set payload = @payload::jsonb, status = @status, updated_at = now() where id = @id returning jsonb_strip_nulls(jsonb_build_object('id', id::text, 'tenantId', tenant_id::text, 'branchId', branch_id::text, 'status', status, 'isActive', is_active, 'createdAt', created_at, 'updatedAt', updated_at) || payload)", connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", targetStatus);
        command.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, json);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        await AuditAsync(connection, "Agenda", "StatusChanged", "appointments", id, $"Agendamento alterado para {targetStatus}.", json, cancellationToken);
        if (targetStatus is "CheckedIn" or "Cancelled" or "Finished")
        {
            await NotifyAsync(connection, targetStatus == "CheckedIn" ? "Cliente fez check-in." : $"Agendamento {targetStatus}.", "appointments", id, cancellationToken);
        }
        return Deserialize(result?.ToString() ?? json);
    }

    public async Task<Dictionary<string, object?>> DashboardAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        async Task<int> Count(string table, string where = "deleted_at is null")
        {
            await using var command = new NpgsqlCommand($"select count(*) from barber.{table} where {where}", connection);
            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        }
        async Task<decimal> SumPayments(string where)
        {
            await using var command = new NpgsqlCommand($"select coalesce(sum((payload->>'amount')::numeric), 0) from barber.payments where deleted_at is null and {where}", connection);
            return Convert.ToDecimal(await command.ExecuteScalarAsync(cancellationToken));
        }
        async Task<decimal> AverageRating()
        {
            await using var command = new NpgsqlCommand("select coalesce(round(avg((payload->>'rating')::numeric), 2), 0) from barber.reviews where deleted_at is null", connection);
            return Convert.ToDecimal(await command.ExecuteScalarAsync(cancellationToken));
        }

        var today = DateTime.UtcNow.Date;
        var month = new DateTime(today.Year, today.Month, 1);
        await using var todayCommand = new NpgsqlCommand("select count(*) from barber.appointments where deleted_at is null and coalesce((payload->>'scheduledAt')::timestamp, created_at)::date = current_date", connection);
        var appointmentsToday = Convert.ToInt32(await todayCommand.ExecuteScalarAsync(cancellationToken));
        return new Dictionary<string, object?>
        {
            ["revenueToday"] = await SumPayments("created_at::date = current_date"),
            ["revenueMonth"] = await SumPayments("created_at >= date_trunc('month', now())"),
            ["appointmentsToday"] = appointmentsToday,
            ["activeClients"] = await Count("clients", "deleted_at is null and is_active"),
            ["openServiceOrders"] = await Count("service_orders", "deleted_at is null and status in ('Open','Payment')"),
            ["averageTicket"] = await SumPayments("created_at >= date_trunc('month', now())") / Math.Max(await Count("payments"), 1),
            ["criticalStock"] = await Count("products", "deleted_at is null and coalesce((payload->>'currentStock')::numeric, 0) <= coalesce((payload->>'minStock')::numeric, 0)"),
            ["averageRating"] = await AverageRating(),
            ["activeCampaigns"] = await Count("campaigns", "deleted_at is null and is_active"),
            ["kioskOnline"] = await Count("kiosk_devices", "deleted_at is null and is_active"),
            ["publicWebLeads"] = await Count("public_leads"),
            ["availableProfessionals"] = await Count("professionals", "deleted_at is null and is_active"),
            ["isDemo"] = false
        };
    }

    public async Task<Dictionary<string, object?>> PublicAppointmentAsync(JsonElement payload, CancellationToken cancellationToken = default)
    {
        var protocol = $"PUB-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var enrichedPayload = EnrichPayload(payload, new Dictionary<string, object?>
        {
            ["protocol"] = protocol,
            ["origin"] = "PublicWeb",
            ["status"] = ExtractString(payload, "status") ?? "Scheduled"
        });
        var appointment = await CreateAsync("appointments", enrichedPayload, cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await NotifyAsync(connection, "Novo agendamento público.", "appointments", Guid.Parse(appointment["id"]!.ToString()!), cancellationToken);
        return appointment;
    }

    public async Task<Dictionary<string, object?>> StockMovementAsync(string type, JsonElement payload, CancellationToken cancellationToken = default)
    {
        var enrichedPayload = EnrichPayload(payload, new Dictionary<string, object?> { ["type"] = type });
        var movement = await CreateAsync("stock_movements", enrichedPayload, cancellationToken);
        if (ExtractString(payload, "productId") is { } productId && Guid.TryParse(productId, out var productGuid))
        {
            await ApplyStockToProductAsync(productGuid, type, payload, cancellationToken);
        }
        await using var connection = await OpenAsync(cancellationToken);
        await NotifyAsync(connection, type == "exit" ? "Saída de estoque registrada." : "Entrada de estoque registrada.", "stock_movements", Guid.Parse(movement["id"]!.ToString()!), cancellationToken);
        return movement;
    }

    public async Task<Dictionary<string, object?>> PayServiceOrderAsync(Guid orderId, JsonElement payload, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var payment = await InsertWithConnectionAsync(connection, transaction, "payments", EnrichPayload(payload, new Dictionary<string, object?> { ["serviceOrderId"] = orderId, ["receiptNumber"] = $"REC-{DateTime.UtcNow:yyyyMMddHHmmss}" }), "Paid", cancellationToken);
            await using (var command = new NpgsqlCommand("update barber.service_orders set status = 'Paid', payload = payload || @payment::jsonb, updated_at = now() where id = @id", connection, transaction))
            {
                command.Parameters.AddWithValue("id", orderId);
                command.Parameters.AddWithValue("payment", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(new { paidAt = DateTime.UtcNow, paymentId = payment["id"], receiptNumber = payment.GetValueOrDefault("receiptNumber") }, JsonOptions));
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
            await using var auditConnection = await OpenAsync(cancellationToken);
            await AuditAsync(auditConnection, "Comanda", "Paid", "service_orders", orderId, "Comanda paga via API real.", JsonSerializer.Serialize(payment, JsonOptions), cancellationToken);
            await NotifyAsync(auditConnection, "Comanda paga.", "service_orders", orderId, cancellationToken);
            return payment;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


    private async Task<Dictionary<string, object?>> InsertWithConnectionAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string resource, JsonElement payload, string status, CancellationToken cancellationToken)
    {
        var validation = Validate(resource, payload, null);
        if (validation.Count > 0) throw new EnterpriseValidationException(validation);
        var id = Guid.NewGuid();
        var json = MergeId(payload, id);
        var sql = $"insert into barber.{Table(resource)} (id, tenant_id, branch_id, payload, status) values (@id, @tenantId, @branchId, @payload::jsonb, @status) returning jsonb_strip_nulls(jsonb_build_object('id', id::text, 'tenantId', tenant_id::text, 'branchId', branch_id::text, 'status', status, 'isActive', is_active, 'createdAt', created_at) || payload)";
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenantId", TenantId);
        command.Parameters.AddWithValue("branchId", BranchId);
        command.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, json);
        command.Parameters.AddWithValue("status", status);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Deserialize(result?.ToString() ?? json);
    }

    private async Task ApplyStockToProductAsync(Guid productId, string movementType, JsonElement payload, CancellationToken cancellationToken)
    {
        var quantity = ExtractDecimal(payload, "quantity") ?? 0;
        if (quantity <= 0) throw new EnterpriseValidationException([new("quantity", "Quantidade deve ser maior que zero.")]);
        var delta = movementType is "exit" ? -quantity : quantity;
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(@"update barber.products
set payload = jsonb_set(payload, '{currentStock}', to_jsonb(greatest(coalesce((payload->>'currentStock')::numeric, 0) + @delta, 0)), true), updated_at = now()
where id = @productId and deleted_at is null
returning coalesce((payload->>'currentStock')::numeric, 0) <= coalesce((payload->>'minStock')::numeric, 0)", connection);
        command.Parameters.AddWithValue("productId", productId);
        command.Parameters.AddWithValue("delta", delta);
        var critical = await command.ExecuteScalarAsync(cancellationToken);
        await AuditAsync(connection, "Estoque", movementType, "products", productId, $"Movimento de estoque {movementType} aplicado.", payload.GetRawText(), cancellationToken);
        if (critical is bool true) await NotifyAsync(connection, "Produto abaixo do estoque mínimo.", "products", productId, cancellationToken);
    }

    private async Task NotifyOnCreateAsync(NpgsqlConnection connection, string resource, Guid id, string json, CancellationToken cancellationToken)
    {
        switch (resource)
        {
            case "reviews" when JsonDocument.Parse(json).RootElement.TryGetProperty("rating", out var rating) && rating.TryGetInt32(out var value) && value <= 3:
                await NotifyAsync(connection, "Avaliação baixa registrada.", "reviews", id, cancellationToken);
                break;
            case "campaigns":
                await NotifyAsync(connection, "Campanha criada.", "campaigns", id, cancellationToken);
                break;
            case "coupons":
                await NotifyAsync(connection, "Cupom criado.", "coupons", id, cancellationToken);
                break;
        }
    }

    private async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        if (_schemaReady) return;
        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            if (_schemaReady) return;
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "ScriptsSQL", "barber_full_database_postgresql.sql");
            var sql = File.Exists(scriptPath) ? await File.ReadAllTextAsync(scriptPath, cancellationToken) : EmbeddedSchema;
            if (!sql.Contains("CREATE SCHEMA IF NOT EXISTS barber", StringComparison.OrdinalIgnoreCase)) sql = EmbeddedSchema;
            await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync(cancellationToken);
            _schemaReady = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao preparar schema BarberSync.");
            throw;
        }
        finally
        {
            SchemaLock.Release();
        }
    }

    private async Task AuditAsync(NpgsqlConnection connection, string module, string action, string entityName, Guid entityId, string description, string metadata, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("insert into barber.audit_logs (tenant_id, branch_id, module, action, entity_name, entity_id, description, metadata) values (@tenantId, @branchId, @module, @action, @entityName, @entityId, @description, @metadata::jsonb)", connection);
        command.Parameters.AddWithValue("tenantId", TenantId);
        command.Parameters.AddWithValue("branchId", BranchId);
        command.Parameters.AddWithValue("module", module);
        command.Parameters.AddWithValue("action", action);
        command.Parameters.AddWithValue("entityName", entityName);
        command.Parameters.AddWithValue("entityId", entityId);
        command.Parameters.AddWithValue("description", description);
        command.Parameters.AddWithValue("metadata", NpgsqlDbType.Jsonb, metadata);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task NotifyAsync(NpgsqlConnection connection, string message, string entityName, Guid entityId, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("insert into barber.notifications (tenant_id, branch_id, title, message, entity_name, entity_id, payload) values (@tenantId, @branchId, @title, @message, @entityName, @entityId, @payload::jsonb)", connection);
        command.Parameters.AddWithValue("tenantId", TenantId);
        command.Parameters.AddWithValue("branchId", BranchId);
        command.Parameters.AddWithValue("title", "BarberSync");
        command.Parameters.AddWithValue("message", message);
        command.Parameters.AddWithValue("entityName", entityName);
        command.Parameters.AddWithValue("entityId", entityId);
        command.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(new { entityName, entityId, createdAt = DateTime.UtcNow }, JsonOptions));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }


    private static JsonElement EnrichPayload(JsonElement payload, IReadOnlyDictionary<string, object?> values)
    {
        var dictionary = Deserialize(payload.GetRawText());
        foreach (var item in values) dictionary[item.Key] = item.Value;
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dictionary, JsonOptions), JsonOptions);
    }

    private static string MergeId(JsonElement payload, Guid id)
    {
        var dictionary = Deserialize(payload.GetRawText());
        dictionary["id"] = id.ToString();
        dictionary["isDemo"] = false;
        return JsonSerializer.Serialize(dictionary, JsonOptions);
    }

    private static Dictionary<string, object?> Deserialize(string json) => JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions) ?? [];

    private static string? ExtractString(JsonElement payload, string name)
    {
        if (!payload.TryGetProperty(name, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static decimal? ExtractDecimal(JsonElement payload, string name)
    {
        if (!payload.TryGetProperty(name, out var value)) return null;
        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(value.GetString(), out var number) => number,
            _ => null
        };
    }

    private static List<FieldError> Validate(string resource, JsonElement payload, Guid? id)
    {
        var errors = new List<FieldError>();
        void Required(string field, string message)
        {
            if (string.IsNullOrWhiteSpace(ExtractString(payload, field))) errors.Add(new(field, message));
        }
        decimal? Number(string field) => ExtractDecimal(payload, field);

        switch (resource)
        {
            case "clients":
                Required("name", "Nome é obrigatório.");
                if (string.IsNullOrWhiteSpace(ExtractString(payload, "phone")) && string.IsNullOrWhiteSpace(ExtractString(payload, "whatsapp"))) errors.Add(new("phone", "Telefone ou WhatsApp é obrigatório."));
                var email = ExtractString(payload, "email");
                if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@')) errors.Add(new("email", "E-mail deve ser válido."));
                break;
            case "professionals":
                Required("name", "Nome é obrigatório.");
                Required("specialty", "Especialidade é obrigatória.");
                var commission = Number("commissionPercent");
                if (commission is < 0 or > 100) errors.Add(new("commissionPercent", "Comissão deve ficar entre 0 e 100."));
                break;
            case "services":
                Required("name", "Nome é obrigatório.");
                Required("category", "Categoria é obrigatória.");
                if (Number("price") is null or <= 0) errors.Add(new("price", "Preço deve ser maior que zero."));
                if (Number("durationMinutes") is null or <= 0) errors.Add(new("durationMinutes", "Duração deve ser maior que zero."));
                if (Number("commissionPercent") is < 0 or > 100) errors.Add(new("commissionPercent", "Comissão deve ficar entre 0 e 100."));
                break;
            case "products":
                Required("name", "Nome é obrigatório.");
                if (Number("salePrice") is null or <= 0) errors.Add(new("salePrice", "Preço de venda deve ser maior que zero."));
                if (Number("currentStock") is < 0) errors.Add(new("currentStock", "Estoque atual não pode ser negativo."));
                if (Number("minStock") is < 0) errors.Add(new("minStock", "Estoque mínimo não pode ser negativo."));
                break;
            case "appointments":
                if (string.IsNullOrWhiteSpace(ExtractString(payload, "clientName")) && string.IsNullOrWhiteSpace(ExtractString(payload, "client")) && string.IsNullOrWhiteSpace(ExtractString(payload, "name"))) errors.Add(new("clientName", "Cliente é obrigatório."));
                if (string.IsNullOrWhiteSpace(ExtractString(payload, "serviceName")) && string.IsNullOrWhiteSpace(ExtractString(payload, "service")) && string.IsNullOrWhiteSpace(ExtractString(payload, "serviceId"))) errors.Add(new("serviceName", "Serviço é obrigatório."));
                if (string.IsNullOrWhiteSpace(ExtractString(payload, "professionalName")) && string.IsNullOrWhiteSpace(ExtractString(payload, "professional")) && string.IsNullOrWhiteSpace(ExtractString(payload, "professionalId"))) errors.Add(new("professionalName", "Profissional é obrigatório."));
                if (ExtractString(payload, "scheduledAt") is not { Length: > 0 }) errors.Add(new("scheduledAt", "Data/hora é obrigatória."));
                if (ExtractString(payload, "scheduledAt") is { } scheduled && DateTime.TryParse(scheduled, out var when) && when < DateTime.Now) errors.Add(new("scheduledAt", "Não é permitido agendar em data/hora passada."));
                break;
            case "reviews":
                if (Number("rating") is null or < 1 or > 5) errors.Add(new("rating", "Nota deve ficar entre 1 e 5."));
                break;
            case "campaigns":
                Required("name", "Nome é obrigatório.");
                if (string.IsNullOrWhiteSpace(ExtractString(payload, "audience")) && string.IsNullOrWhiteSpace(ExtractString(payload, "targetAudience"))) errors.Add(new("audience", "Público-alvo é obrigatório."));
                break;
            case "coupons":
                Required("code", "Código é obrigatório.");
                break;
        }

        return errors;
    }

    private static bool CanTransition(string current, string target) => (NormalizeStatus(current), target) switch
    {
        ("Scheduled", "Confirmed") => true,
        ("Confirmed", "CheckedIn") => true,
        ("CheckedIn", "InService") => true,
        ("InService", "Finished") => true,
        ("Scheduled", "Cancelled") or ("Confirmed", "Cancelled") => true,
        ("Scheduled", "NoShow") or ("Confirmed", "NoShow") => true,
        var pair when pair.Item1 == target => true,
        _ => false
    };

    private static string NormalizeStatus(string value) => value switch
    {
        "Agendado" => "Scheduled",
        "Confirmado" => "Confirmed",
        "Check-in" => "CheckedIn",
        "Em atendimento" => "InService",
        "Finalizado" => "Finished",
        "Cancelado" => "Cancelled",
        _ => value
    };

    private static string Table(string resource) => Tables.TryGetValue(resource, out var table) ? table : throw new ArgumentOutOfRangeException(nameof(resource), resource, "Recurso não mapeado.");
    private static string Module(string resource) => resource.Replace('-', ' ').Replace('_', ' ');

    private const string EmbeddedSchema = "CREATE SCHEMA IF NOT EXISTS barber;";
}

public sealed class EnterpriseValidationException(IReadOnlyCollection<FieldError> errors) : Exception("Existem campos inválidos.")
{
    public IReadOnlyCollection<FieldError> Errors { get; } = errors;
}

public sealed record FieldError(string Field, string Message);
