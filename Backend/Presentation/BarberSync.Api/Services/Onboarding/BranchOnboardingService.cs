using System.Data.Common;
using System.Text.Json;
using BarberSync.Application.Abstractions;

namespace BarberSync.Api.Services.Onboarding;

public sealed record OnboardingProgressDto(Guid BranchId, int CurrentStep, bool IsCompleted, IReadOnlyDictionary<int, JsonElement> Steps, DateTime UpdatedAt);

public sealed class BranchOnboardingService(IDbConnectionFactory connections, ICurrentUserContext currentUser)
{
    public async Task<OnboardingProgressDto> GetAsync(CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT current_step, is_completed, steps, updated_at
              FROM barber.branch_onboarding
             WHERE tenant_id = @tenant AND branch_id = @branch
            """;
        Add(command, "tenant", currentUser.TenantId);
        Add(command, "branch", currentUser.BranchId);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return new(currentUser.BranchId, 1, false, new Dictionary<int, JsonElement>(), DateTime.UtcNow);
        return new(currentUser.BranchId, reader.GetInt32(0), reader.GetBoolean(1), ParseSteps(reader.GetString(2)), reader.GetDateTime(3));
    }

    public async Task<OnboardingProgressDto> SaveStepAsync(int step, JsonElement payload, CancellationToken ct)
    {
        Validate(step, payload);
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var json = payload.GetRawText();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO barber.branch_onboarding(tenant_id, branch_id, current_step, steps, updated_by)
                VALUES (@tenant, @branch, @next, jsonb_build_object(@step, @payload::jsonb), @user)
                ON CONFLICT (tenant_id, branch_id) DO UPDATE SET
                    current_step = GREATEST(barber.branch_onboarding.current_step, @next),
                    steps = barber.branch_onboarding.steps || jsonb_build_object(@step, @payload::jsonb),
                    updated_by = @user, updated_at = now()
                """;
            AddContext(command); Add(command, "step", step.ToString()); Add(command, "next", Math.Min(step + 1, 10)); Add(command, "payload", json);
            await command.ExecuteNonQueryAsync(ct);
        }
        await Audit(connection, transaction, "OnboardingStepSaved", step, json, ct);
        await transaction.CommitAsync(ct);
        return await GetAsync(ct);
    }

    public async Task<OnboardingProgressDto> CompleteAsync(CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE barber.branch_onboarding SET is_completed = true, completed_at = now(), updated_at = now(), updated_by = @user
             WHERE tenant_id = @tenant AND branch_id = @branch
               AND steps ?& ARRAY['1','2','3','4','5','6','7','8','9']
            """;
        AddContext(command);
        if (await command.ExecuteNonQueryAsync(ct) == 0)
            throw new InvalidOperationException("Conclua todas as etapas obrigatórias antes de finalizar.");
        await Audit(connection, transaction, "OnboardingCompleted", 10, "{}", ct);
        await transaction.CommitAsync(ct);
        return await GetAsync(ct);
    }

    private async Task Audit(DbConnection connection, DbTransaction transaction, string action, int step, string json, CancellationToken ct)
    {
        await using var audit = connection.CreateCommand();
        audit.Transaction = transaction;
        audit.CommandText = """
            INSERT INTO barber.audit_logs(id, tenant_id, branch_id, user_id, operation, entity_name, entity_id, after_data, module, action, description)
            VALUES (gen_random_uuid(), @tenant, @branch, @user, @action, 'branch_onboarding', @branch, @data::jsonb, 'Onboarding', @action, @description)
            """;
        AddContext(audit); Add(audit, "action", action); Add(audit, "data", json); Add(audit, "description", $"Etapa {step} da primeira configuração atualizada.");
        await audit.ExecuteNonQueryAsync(ct);
    }

    private void AddContext(DbCommand command)
    {
        Add(command, "tenant", currentUser.TenantId); Add(command, "branch", currentUser.BranchId); Add(command, "user", currentUser.UserId);
    }

    private static void Validate(int step, JsonElement payload)
    {
        if (step is < 1 or > 9 || payload.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Etapa ou conteúdo inválido.");
        string[][] required =
        [
            ["companyName", "document"], ["branchName", "timezone"], ["weekdays", "opensAt", "closesAt"],
            ["serviceName", "durationMinutes", "price"], ["professionalName", "email"], ["methods"],
            ["openingBalance"], ["profile", "permissions"], ["publicWebEnabled", "kioskEnabled"]
        ];
        var missing = required[step - 1].Where(name => !payload.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString())).ToArray();
        if (missing.Length > 0) throw new ArgumentException($"Campos obrigatórios: {string.Join(", ", missing)}.");
    }

    private static IReadOnlyDictionary<int, JsonElement> ParseSteps(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(x => int.Parse(x.Name), x => x.Value.Clone());
    }

    private static void Add(DbCommand command, string name, object value) { var p = command.CreateParameter(); p.ParameterName = name; p.Value = value; command.Parameters.Add(p); }
}
