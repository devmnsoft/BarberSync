using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using BarberSync.Domain.Saas;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Infrastructure.Saas;

public sealed class PostgresModuleEntitlementService(
    IDbConnectionFactory connections,
    IEffectiveAccessCache cache) : IModuleEntitlementService
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(3);

    public async Task<ModuleAccessDecision> DecideAsync(ModuleAccessRequest request, CancellationToken cancellationToken = default)
    {
        var key = $"module|{request.TenantId:N}|{request.UserId:N}|{request.BranchId:N}|{request.ModuleKey.ToUpperInvariant()}|{request.Permission}|{request.FeatureKey}|{request.QuotaKey}";
        if (cache.TryGet(key, out var cached)) return cached;

        const string sql = """
            WITH usr AS (
              SELECT u.id,u.tenant_id,u.branch_id,u.is_active,u.status,u.deleted_at
              FROM barber.users u WHERE u.id=@user
              UNION ALL
              SELECT s.actor_user_id,s.tenant_id,s.branch_id,true,'Active',NULL::timestamptz
              FROM barber.platform_scope_sessions s WHERE s.actor_user_id=@user AND s.tenant_id=@tenant AND s.branch_id=@branch
                AND s.status='Active' AND s.expires_at>now()
            ), selected_subscription AS (
              SELECT s.* FROM barber.tenant_subscriptions s
              WHERE s.tenant_id=@tenant ORDER BY s.created_at DESC LIMIT 1
            ), selected_module AS (
              SELECT m.* FROM barber.saas_modules m WHERE m.module_key=@module LIMIT 1
            ), selected_contract AS (
              SELECT c.* FROM barber.tenant_module_contracts c JOIN selected_module m ON m.id=c.module_id
              WHERE c.tenant_id=@tenant AND c.deleted_at IS NULL ORDER BY c.created_at DESC LIMIT 1
            ), selected_setting AS (
              SELECT s.* FROM barber.tenant_module_settings s WHERE s.tenant_id=@tenant AND s.module_key=@module LIMIT 1
            )
            SELECT
              EXISTS(SELECT 1 FROM usr u WHERE u.tenant_id=@tenant AND u.is_active AND u.status='Active' AND u.deleted_at IS NULL),
              EXISTS(SELECT 1 FROM barber.tenants t WHERE t.id=@tenant AND t.is_active AND t.status='Active' AND t.deleted_at IS NULL),
              EXISTS(SELECT 1 FROM barber.branches b,usr u WHERE b.id=@branch AND b.tenant_id=@tenant AND b.is_active AND b.status='Active' AND b.deleted_at IS NULL
                AND (u.branch_id=@branch OR EXISTS(SELECT 1 FROM barber.user_branch_access uba WHERE uba.user_id=u.id AND uba.branch_id=@branch AND uba.is_active))),
              (SELECT status FROM selected_subscription),
              EXISTS(SELECT 1 FROM selected_subscription s WHERE s.status IN('Trial','Active','GracePeriod')
                AND coalesce(s.starts_at,s.period_start::timestamptz)<=now()
                AND coalesce(s.ends_at,s.period_end::timestamptz)>now()),
              (SELECT status FROM selected_module),
              EXISTS(SELECT 1 FROM selected_subscription s JOIN barber.saas_plan_modules pm ON pm.plan_id=s.plan_id
                JOIN selected_module m ON m.id=pm.module_id WHERE pm.is_included),
              (SELECT status FROM selected_contract),
              EXISTS(SELECT 1 FROM selected_contract c WHERE c.status IN('Trial','Active','GracePeriod') AND c.starts_at<=now()
                AND (c.ends_at IS NULL OR c.ends_at>now()) AND (c.trial_ends_at IS NULL OR c.status<>'Trial' OR c.trial_ends_at>now())),
              coalesce((SELECT is_enabled FROM selected_setting),true),
              CASE WHEN @permission IS NULL OR EXISTS(SELECT 1 FROM barber.platform_scope_sessions ps WHERE ps.actor_user_id=@user AND ps.tenant_id=@tenant AND ps.branch_id=@branch AND ps.status='Active' AND ps.expires_at>now()) THEN true ELSE
                NOT EXISTS(SELECT 1 FROM barber.user_permissions up JOIN barber.permissions p ON p.id=up.permission_id
                  WHERE up.user_id=@user AND p.code=@permission AND NOT up.is_allowed)
                AND (EXISTS(SELECT 1 FROM barber.user_permissions up JOIN barber.permissions p ON p.id=up.permission_id
                      WHERE up.user_id=@user AND p.code=@permission AND up.is_allowed)
                  OR EXISTS(SELECT 1 FROM barber.user_roles ur JOIN barber.role_permissions rp ON rp.role_id=ur.role_id
                      JOIN barber.permissions p ON p.id=rp.permission_id WHERE ur.user_id=@user AND p.code=@permission)) END,
              CASE WHEN @feature IS NULL THEN true ELSE coalesce((SELECT lower(settings_json->'features'->>@feature)='true' FROM selected_setting),false) END,
              CASE WHEN @quota IS NULL THEN true ELSE coalesce((
                SELECT coalesce(uc.used,0)<(splan.limits->>@quota)::bigint FROM selected_subscription ss
                JOIN barber.saas_plans splan ON splan.id=ss.plan_id
                LEFT JOIN barber.usage_counters uc ON uc.tenant_id=@tenant AND uc.metric=@quota AND uc.period_start=date_trunc('month',now())::date
                WHERE splan.limits ? @quota),true) END
            """;

        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("user", request.UserId);
        command.Parameters.AddWithValue("tenant", request.TenantId);
        command.Parameters.AddWithValue("branch", request.BranchId);
        command.Parameters.AddWithValue("module", request.ModuleKey.Trim().ToUpperInvariant());
        AddNullable(command, "permission", request.Permission);
        AddNullable(command, "feature", request.FeatureKey);
        AddNullable(command, "quota", request.QuotaKey);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        var userActive = reader.GetBoolean(0);
        var tenantActive = reader.GetBoolean(1);
        var branchValid = reader.GetBoolean(2);
        var subscriptionStatus = reader.IsDBNull(3) ? null : reader.GetString(3);
        var subscriptionActive = reader.GetBoolean(4);
        var moduleStatus = reader.IsDBNull(5) ? null : reader.GetString(5);
        var planIncluded = reader.GetBoolean(6);
        var contractStatus = reader.IsDBNull(7) ? null : reader.GetString(7);
        var contractActive = reader.GetBoolean(8);
        var settingEnabled = reader.GetBoolean(9);
        var permissionAllowed = reader.GetBoolean(10);
        var featureEnabled = reader.GetBoolean(11);
        var quotaAvailable = reader.GetBoolean(12);

        var decision = BuildDecision(request, userActive, tenantActive, branchValid, subscriptionStatus, subscriptionActive,
            moduleStatus, planIncluded, contractStatus, contractActive, settingEnabled, permissionAllowed, featureEnabled, quotaAvailable);
        cache.Set(key, decision, CacheLifetime);
        return decision;
    }

    public async Task<IReadOnlyDictionary<string, ModuleAccessDecision>> GetEffectiveAccessAsync(Guid userId, Guid tenantId, Guid branchId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH context AS (
              SELECT u.id user_id,
                u.is_active AND u.status='Active' AND u.deleted_at IS NULL user_ok,
                t.is_active AND t.status='Active' AND t.deleted_at IS NULL tenant_ok,
                b.is_active AND b.status='Active' AND b.deleted_at IS NULL AND b.tenant_id=t.id
                  AND (u.branch_id=b.id OR EXISTS(SELECT 1 FROM barber.user_branch_access x WHERE x.user_id=u.id AND x.branch_id=b.id AND x.is_active)) branch_ok
              FROM (SELECT id,tenant_id,branch_id,is_active,status,deleted_at FROM barber.users WHERE id=@user
                    UNION ALL SELECT actor_user_id,tenant_id,branch_id,true,'Active',NULL::timestamptz FROM barber.platform_scope_sessions WHERE actor_user_id=@user AND tenant_id=@tenant AND branch_id=@branch AND status='Active' AND expires_at>now()) u
              JOIN barber.tenants t ON t.id=u.tenant_id JOIN barber.branches b ON b.id=@branch
              WHERE u.id=@user AND u.tenant_id=@tenant
            ), subscription AS (
              SELECT s.* FROM barber.tenant_subscriptions s WHERE s.tenant_id=@tenant ORDER BY s.created_at DESC LIMIT 1
            )
            SELECT m.module_key,s.status,
              (SELECT c.status FROM barber.tenant_module_contracts c WHERE c.tenant_id=@tenant AND c.module_id=m.id AND c.deleted_at IS NULL ORDER BY c.created_at DESC LIMIT 1),
              c.user_ok AND c.tenant_ok AND c.branch_ok AND s.status IN('Trial','Active','GracePeriod')
              AND coalesce(s.starts_at,s.period_start::timestamptz)<=now() AND coalesce(s.ends_at,s.period_end::timestamptz)>now()
              AND m.status='Active'
              AND (EXISTS(SELECT 1 FROM barber.saas_plan_modules pm WHERE pm.plan_id=s.plan_id AND pm.module_id=m.id AND pm.is_included)
                OR EXISTS(SELECT 1 FROM barber.tenant_module_contracts mc WHERE mc.tenant_id=@tenant AND mc.module_id=m.id AND mc.deleted_at IS NULL
                  AND mc.status IN('Trial','Active','GracePeriod') AND mc.starts_at<=now() AND (mc.ends_at IS NULL OR mc.ends_at>now())
                  AND (mc.trial_ends_at IS NULL OR mc.status<>'Trial' OR mc.trial_ends_at>now())))
              AND coalesce((SELECT ms.is_enabled FROM barber.tenant_module_settings ms WHERE ms.tenant_id=@tenant AND ms.module_key=m.module_key),true)
              AND (EXISTS(SELECT 1 FROM barber.platform_scope_sessions ps WHERE ps.actor_user_id=@user AND ps.tenant_id=@tenant AND ps.branch_id=@branch AND ps.status='Active' AND ps.expires_at>now())
                OR NOT EXISTS(SELECT 1 FROM barber.saas_module_permissions mp WHERE mp.module_id=m.id)
                OR EXISTS(SELECT 1 FROM barber.saas_module_permissions mp JOIN barber.permissions p ON p.id=mp.permission_id
                    JOIN barber.role_permissions rp ON rp.permission_id=p.id JOIN barber.user_roles ur ON ur.role_id=rp.role_id
                    WHERE mp.module_id=m.id AND ur.user_id=@user
                      AND NOT EXISTS(SELECT 1 FROM barber.user_permissions deny WHERE deny.user_id=@user AND deny.permission_id=p.id AND NOT deny.is_allowed))
                OR EXISTS(SELECT 1 FROM barber.saas_module_permissions mp JOIN barber.user_permissions allow ON allow.permission_id=mp.permission_id
                    WHERE mp.module_id=m.id AND allow.user_id=@user AND allow.is_allowed)) allowed
            FROM barber.saas_modules m CROSS JOIN context c CROSS JOIN subscription s
            WHERE m.status<>'Archived' ORDER BY m.display_order
            """;
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("user", userId); command.Parameters.AddWithValue("tenant", tenantId); command.Parameters.AddWithValue("branch", branchId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new Dictionary<string, ModuleAccessDecision>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(cancellationToken))
        {
            var module = reader.GetString(0); var subscription = reader.GetString(1); var contract = reader.IsDBNull(2)?null:reader.GetString(2); var allowed = reader.GetBoolean(3);
            result[module] = new(allowed,module,allowed?ModuleAccessReasonCodes.Allowed:ModuleAccessReasonCodes.ModuleNotEntitled,
                allowed?"Acesso permitido.":"Módulo indisponível para o acesso efetivo deste usuário.",tenantId,userId,subscription,contract);
        }
        return result;
    }

    public void Invalidate(Guid tenantId, Guid? userId = null, Guid? branchId = null) => cache.Invalidate(tenantId, userId, branchId);

    private static ModuleAccessDecision BuildDecision(ModuleAccessRequest request, bool userActive, bool tenantActive, bool branchValid,
        string? subscriptionStatus, bool subscriptionActive, string? moduleStatus, bool planIncluded, string? contractStatus,
        bool contractActive, bool settingEnabled, bool permissionAllowed, bool featureEnabled, bool quotaAvailable)
    {
        (string Code, string Reason) result = !userActive
            ? (ModuleAccessReasonCodes.Unauthenticated,"Usuário autenticado inválido ou inativo.")
            : !tenantActive ? (ModuleAccessReasonCodes.TenantInactive,"Cliente suspenso ou inativo.")
            : !branchValid ? (ModuleAccessReasonCodes.BranchInvalid,"Unidade inválida para este usuário.")
            : !subscriptionActive ? (ModuleAccessReasonCodes.SubscriptionInactive,"A assinatura base não está vigente.")
            : !string.Equals(moduleStatus,"Active",StringComparison.OrdinalIgnoreCase) ? (ModuleAccessReasonCodes.ModuleSuspended,"O módulo está indisponível na plataforma.")
            : !(planIncluded || contractActive) ? (ModuleAccessReasonCodes.ModuleNotEntitled,"O módulo não está incluído na assinatura atual.")
            : !settingEnabled ? (ModuleAccessReasonCodes.ModuleDisabled,"O módulo está desabilitado para este cliente.")
            : !permissionAllowed ? (ModuleAccessReasonCodes.PermissionDenied,"Você não possui acesso a esta funcionalidade.")
            : !quotaAvailable ? (ModuleAccessReasonCodes.QuotaExceeded,"O limite contratado para esta funcionalidade foi atingido.")
            : !featureEnabled ? (ModuleAccessReasonCodes.FeatureDisabled,"Esta funcionalidade não está habilitada.")
            : (ModuleAccessReasonCodes.Allowed,"Acesso permitido.");
        return new(result.Code==ModuleAccessReasonCodes.Allowed,request.ModuleKey,result.Code,result.Reason,request.TenantId,request.UserId,subscriptionStatus,contractStatus);
    }

    private static void AddNullable(NpgsqlCommand command, string name, string? value) =>
        command.Parameters.Add(name,NpgsqlDbType.Text).Value=(object?)value??DBNull.Value;
}
