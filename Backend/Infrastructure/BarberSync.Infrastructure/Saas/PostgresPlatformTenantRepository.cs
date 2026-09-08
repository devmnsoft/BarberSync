using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using BarberSync.Domain.ValueObjects;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Infrastructure.Saas;

public sealed class PostgresPlatformTenantRepository(IDbConnectionFactory connections) : IPlatformTenantRepository
{
    public async Task<PlatformDashboardDto> DashboardAsync(DateOnly from, DateOnly until, CancellationToken cancellationToken)
    {
        const string totalsSql="""
            SELECT
             count(*) FILTER(WHERE t.status='Active' AND t.is_active AND t.deleted_at IS NULL),
             count(DISTINCT t.id) FILTER(WHERE s.status='Trial' AND t.deleted_at IS NULL),
             count(*) FILTER(WHERE t.status='Suspended' AND t.deleted_at IS NULL),
             count(*) FILTER(WHERE t.status='Cancelled' OR t.deleted_at IS NOT NULL),
             (SELECT count(DISTINCT tenant_id) FROM barber.tenant_module_contracts WHERE status='PastDue' AND deleted_at IS NULL),
             count(*) FILTER(WHERE t.created_at>=@from AND t.created_at<@until),
             (SELECT count(*) FROM barber.branches WHERE deleted_at IS NULL),
             (SELECT count(*) FROM barber.users WHERE deleted_at IS NULL),
             (SELECT count(*) FROM barber.professionals WHERE deleted_at IS NULL),
             (SELECT count(*) FROM barber.clients WHERE deleted_at IS NULL),
             (SELECT count(*) FROM barber.kiosk_devices WHERE deleted_at IS NULL),
             0::bigint,0::bigint,
             coalesce((SELECT sum(CASE s.billing_cycle WHEN 'Annual' THEN p.annual_price/12 WHEN 'Monthly' THEN p.monthly_price ELSE 0 END)
                       FROM barber.tenant_subscriptions s JOIN barber.saas_plans p ON p.id=s.plan_id
                       WHERE s.status IN('Trial','Active','GracePeriod')),0)
             + coalesce((SELECT sum(CASE billing_cycle WHEN 'Annual' THEN contracted_price/12 WHEN 'Monthly' THEN contracted_price ELSE 0 END)
                         FROM barber.tenant_module_contracts WHERE status IN('Trial','Active','GracePeriod') AND deleted_at IS NULL AND contracted_price IS NOT NULL),0)
            FROM barber.tenants t
            LEFT JOIN LATERAL(SELECT status FROM barber.tenant_subscriptions x WHERE x.tenant_id=t.id ORDER BY x.created_at DESC LIMIT 1)s ON true
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        long active,trial,suspended,cancelled,pastDue,newTenants,branches,users,professionals,clients,kiosks,mobile,integrations; decimal mrr;
        await using(var command=new NpgsqlCommand(totalsSql,connection)){command.Parameters.AddWithValue("from",from.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc));command.Parameters.AddWithValue("until",until.AddDays(1).ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc));await using var reader=await command.ExecuteReaderAsync(cancellationToken);await reader.ReadAsync(cancellationToken);active=reader.GetInt64(0);trial=reader.GetInt64(1);suspended=reader.GetInt64(2);cancelled=reader.GetInt64(3);pastDue=reader.GetInt64(4);newTenants=reader.GetInt64(5);branches=reader.GetInt64(6);users=reader.GetInt64(7);professionals=reader.GetInt64(8);clients=reader.GetInt64(9);kiosks=reader.GetInt64(10);mobile=reader.GetInt64(11);integrations=reader.GetInt64(12);mrr=reader.GetDecimal(13);}
        var plans=await LoadPlans(connection,cancellationToken);var modules=await LoadModules(connection,active,cancellationToken);var ticket=active==0?0:mrr/active;
        return new(active,trial,suspended,cancelled,pastDue,newTenants,branches,users,professionals,clients,kiosks,mobile,integrations,mrr,mrr*12,ticket,plans,modules);
    }

    public async Task<PagedResult<PlatformTenantListItemDto>> ListAsync(PlatformTenantQuery query, CancellationToken cancellationToken)
    {
        var page=Math.Max(query.Page,1);var size=Math.Clamp(query.PageSize,1,100);var normalized=BrazilianDocument.Normalize(query.Search);
        const string sql="""
            SELECT t.id,t.name,t.slug,coalesce(t.document_number_normalized,t.document),t.institutional_email,t.status,p.name,
              (SELECT count(DISTINCT module_id) FROM barber.tenant_module_contracts c WHERE c.tenant_id=t.id AND c.deleted_at IS NULL AND c.status IN('Pending','PendingActivation','Trial','Active','GracePeriod','PastDue','Suspended')),
              (SELECT count(*) FROM barber.users u WHERE u.tenant_id=t.id AND u.deleted_at IS NULL),
              (SELECT count(*) FROM barber.branches b WHERE b.tenant_id=t.id AND b.deleted_at IS NULL),t.created_at,
              (SELECT max(u.last_login_at) FROM barber.users u WHERE u.tenant_id=t.id),count(*) over()
            FROM barber.tenants t
            LEFT JOIN LATERAL(SELECT s.* FROM barber.tenant_subscriptions s WHERE s.tenant_id=t.id ORDER BY s.created_at DESC LIMIT 1)s ON true
            LEFT JOIN barber.saas_plans p ON p.id=s.plan_id
            WHERE t.deleted_at IS NULL
              AND (@search IS NULL OR t.name ILIKE '%'||@search||'%' OR t.slug ILIKE '%'||@search||'%' OR t.institutional_email ILIKE '%'||@search||'%' OR (@document<>'' AND coalesce(t.document_number_normalized,t.document)=@document))
              AND (@status IS NULL OR t.status=@status) AND (@plan IS NULL OR s.plan_id=@plan)
              AND (@module IS NULL OR EXISTS(SELECT 1 FROM barber.tenant_module_contracts c WHERE c.tenant_id=t.id AND c.module_id=@module AND c.deleted_at IS NULL))
              AND (@created_from IS NULL OR t.created_at>=@created_from) AND (@created_until IS NULL OR t.created_at<@created_until)
              AND (@trial IS NULL OR (s.status='Trial')=@trial)
              AND (@commercial IS NULL OR EXISTS(SELECT 1 FROM barber.tenant_module_contracts c WHERE c.tenant_id=t.id AND c.status=@commercial AND c.deleted_at IS NULL))
              AND (@branch IS NULL OR EXISTS(SELECT 1 FROM barber.branches b WHERE b.tenant_id=t.id AND b.id=@branch AND b.deleted_at IS NULL))
              AND (@min_users IS NULL OR (SELECT count(*) FROM barber.users u WHERE u.tenant_id=t.id AND u.deleted_at IS NULL)>=@min_users)
            ORDER BY t.created_at DESC OFFSET @offset LIMIT @limit
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);Add(command,"search",string.IsNullOrWhiteSpace(query.Search)?null:query.Search.Trim());Add(command,"document",normalized);Add(command,"status",query.Status);command.Parameters.Add("plan",NpgsqlDbType.Uuid).Value=(object?)query.PlanId??DBNull.Value;command.Parameters.Add("module",NpgsqlDbType.Uuid).Value=(object?)query.ModuleId??DBNull.Value;command.Parameters.Add("created_from",NpgsqlDbType.TimestampTz).Value=query.CreatedFrom.HasValue?query.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc):DBNull.Value;command.Parameters.Add("created_until",NpgsqlDbType.TimestampTz).Value=query.CreatedUntil.HasValue?query.CreatedUntil.Value.AddDays(1).ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc):DBNull.Value;command.Parameters.Add("trial",NpgsqlDbType.Boolean).Value=(object?)query.Trial??DBNull.Value;Add(command,"commercial",query.CommercialStatus);command.Parameters.Add("branch",NpgsqlDbType.Uuid).Value=(object?)query.BranchId??DBNull.Value;command.Parameters.Add("min_users",NpgsqlDbType.Integer).Value=(object?)query.MinUsers??DBNull.Value;command.Parameters.AddWithValue("offset",(page-1)*size);command.Parameters.AddWithValue("limit",size);
        await using var reader=await command.ExecuteReaderAsync(cancellationToken);var items=new List<PlatformTenantListItemDto>();long total=0;while(await reader.ReadAsync(cancellationToken)){total=reader.GetInt64(12);items.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.IsDBNull(3)?null:BrazilianDocument.Mask(reader.GetString(3)),reader.IsDBNull(4)?null:reader.GetString(4),reader.GetString(5),reader.IsDBNull(6)?null:reader.GetString(6),Convert.ToInt32(reader.GetInt64(7)),Convert.ToInt32(reader.GetInt64(8)),Convert.ToInt32(reader.GetInt64(9)),reader.GetFieldValue<DateTimeOffset>(10),reader.IsDBNull(11)?null:reader.GetFieldValue<DateTimeOffset>(11)));}return new(items,page,size,total);
    }

    public async Task<PlatformTenantDetailDto?> DetailAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql="""
            SELECT t.id,t.name,t.slug,t.document_type,coalesce(t.document_number_normalized,t.document),t.institutional_email,t.status,t.created_at,p.name,s.status,s.billing_cycle,coalesce(s.starts_at,s.period_start::timestamptz),coalesce(s.ends_at,s.period_end::timestamptz),
              (SELECT count(*) FROM barber.branches b WHERE b.tenant_id=t.id AND b.deleted_at IS NULL),(SELECT count(*) FROM barber.users u WHERE u.tenant_id=t.id AND u.deleted_at IS NULL),(SELECT count(*) FROM barber.professionals x WHERE x.tenant_id=t.id AND x.deleted_at IS NULL),(SELECT count(*) FROM barber.clients x WHERE x.tenant_id=t.id AND x.deleted_at IS NULL),(SELECT max(u.last_login_at) FROM barber.users u WHERE u.tenant_id=t.id)
            FROM barber.tenants t LEFT JOIN LATERAL(SELECT x.* FROM barber.tenant_subscriptions x WHERE x.tenant_id=t.id ORDER BY x.created_at DESC LIMIT 1)s ON true LEFT JOIN barber.saas_plans p ON p.id=s.plan_id WHERE t.id=@tenant
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("tenant",tenantId);await using var reader=await command.ExecuteReaderAsync(cancellationToken);if(!await reader.ReadAsync(cancellationToken))return null;return new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.IsDBNull(3)?null:reader.GetString(3),reader.IsDBNull(4)?null:BrazilianDocument.Mask(reader.GetString(4)),reader.IsDBNull(5)?null:reader.GetString(5),reader.GetString(6),reader.GetFieldValue<DateTimeOffset>(7),reader.IsDBNull(8)?null:reader.GetString(8),reader.IsDBNull(9)?null:reader.GetString(9),reader.IsDBNull(10)?null:reader.GetString(10),reader.IsDBNull(11)?null:reader.GetFieldValue<DateTimeOffset>(11),reader.IsDBNull(12)?null:reader.GetFieldValue<DateTimeOffset>(12),Convert.ToInt32(reader.GetInt64(13)),Convert.ToInt32(reader.GetInt64(14)),Convert.ToInt32(reader.GetInt64(15)),Convert.ToInt32(reader.GetInt64(16)),reader.IsDBNull(17)?null:reader.GetFieldValue<DateTimeOffset>(17));
    }

    public async Task<PagedResult<PlatformTenantUserDto>> UsersAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page=Math.Max(page,1);pageSize=Math.Clamp(pageSize,1,100);const string sql="""
            SELECT u.id,coalesce(u.full_name,''),u.cpf_normalized,coalesce(u.email,''),u.status,b.name,
              ARRAY(SELECT r.code FROM barber.user_roles ur JOIN barber.roles r ON r.id=ur.role_id WHERE ur.user_id=u.id ORDER BY r.code),
              ARRAY(SELECT DISTINCT p.code FROM barber.user_roles ur JOIN barber.role_permissions rp ON rp.role_id=ur.role_id JOIN barber.permissions p ON p.id=rp.permission_id WHERE ur.user_id=u.id
                    UNION SELECT p.code FROM barber.user_permissions up JOIN barber.permissions p ON p.id=up.permission_id WHERE up.user_id=u.id AND up.is_allowed ORDER BY 1),u.last_login_at,count(*) over()
            FROM barber.users u LEFT JOIN barber.branches b ON b.id=u.branch_id WHERE u.tenant_id=@tenant AND u.deleted_at IS NULL ORDER BY u.created_at DESC OFFSET @offset LIMIT @limit
            """;await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("tenant",tenantId);command.Parameters.AddWithValue("offset",(page-1)*pageSize);command.Parameters.AddWithValue("limit",pageSize);await using var reader=await command.ExecuteReaderAsync(cancellationToken);var items=new List<PlatformTenantUserDto>();long total=0;while(await reader.ReadAsync(cancellationToken)){total=reader.GetInt64(9);items.Add(new(reader.GetGuid(0),reader.GetString(1),reader.IsDBNull(2)?null:BrazilianDocument.Mask(reader.GetString(2)),reader.GetString(3),reader.GetString(4),reader.IsDBNull(5)?null:reader.GetString(5),reader.GetFieldValue<string[]>(6),reader.GetFieldValue<string[]>(7),reader.IsDBNull(8)?null:reader.GetFieldValue<DateTimeOffset>(8)));}return new(items,page,pageSize,total);
    }

    public async Task<IReadOnlyList<PlatformTenantRoleDto>> RolesAsync(Guid tenantId,CancellationToken cancellationToken)
    {
        const string sql="""
            SELECT r.id,r.name,r.code,r.is_system,
              count(DISTINCT ur.user_id) FILTER(WHERE u.tenant_id=@tenant AND u.deleted_at IS NULL),
              ARRAY(SELECT p.code FROM barber.role_permissions rp JOIN barber.permissions p ON p.id=rp.permission_id WHERE rp.role_id=r.id ORDER BY p.code)
            FROM barber.roles r LEFT JOIN barber.user_roles ur ON ur.role_id=r.id LEFT JOIN barber.users u ON u.id=ur.user_id
            WHERE r.tenant_id IS NULL OR r.tenant_id=@tenant GROUP BY r.id,r.name,r.code,r.is_system ORDER BY r.is_system DESC,r.name
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("tenant",tenantId);await using var reader=await command.ExecuteReaderAsync(cancellationToken);var result=new List<PlatformTenantRoleDto>();while(await reader.ReadAsync(cancellationToken))result.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.GetBoolean(3),reader.GetInt64(4),reader.GetFieldValue<string[]>(5)));return result;
    }

    public async Task<IReadOnlyList<PlatformTenantBranchDto>> BranchesAsync(Guid tenantId,CancellationToken cancellationToken)
    {
        const string sql="""
            SELECT b.id,b.name,b.code,b.status,
              (SELECT count(*) FROM barber.users u WHERE u.tenant_id=b.tenant_id AND u.branch_id=b.id AND u.deleted_at IS NULL),
              (SELECT count(*) FROM barber.professionals p WHERE p.tenant_id=b.tenant_id AND p.branch_id=b.id AND p.deleted_at IS NULL)
            FROM barber.branches b WHERE b.tenant_id=@tenant AND b.deleted_at IS NULL ORDER BY b.created_at,b.name
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("tenant",tenantId);await using var reader=await command.ExecuteReaderAsync(cancellationToken);var result=new List<PlatformTenantBranchDto>();while(await reader.ReadAsync(cancellationToken))result.Add(new(reader.GetGuid(0),reader.GetString(1),reader.IsDBNull(2)?null:reader.GetString(2),reader.GetString(3),reader.GetInt64(4),reader.GetInt64(5)));return result;
    }

    public async Task<IReadOnlyList<PlatformTenantUsageDto>> UsageAsync(Guid tenantId,DateOnly from,DateOnly until,CancellationToken cancellationToken)
    {
        const string sql="""
            SELECT u.module_id,m.module_key,m.name,u.usage_date,u.active_users,u.requests,u.relevant_operations,u.usage_units
            FROM barber.tenant_module_usage_daily u JOIN barber.saas_modules m ON m.id=u.module_id
            WHERE u.tenant_id=@tenant AND u.usage_date BETWEEN @from AND @until ORDER BY u.usage_date DESC,m.display_order LIMIT 1000
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("tenant",tenantId);command.Parameters.AddWithValue("from",from);command.Parameters.AddWithValue("until",until);await using var reader=await command.ExecuteReaderAsync(cancellationToken);var result=new List<PlatformTenantUsageDto>();while(await reader.ReadAsync(cancellationToken))result.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.GetFieldValue<DateOnly>(3),reader.GetInt32(4),reader.GetInt64(5),reader.GetInt64(6),reader.IsDBNull(7)?null:reader.GetDecimal(7)));return result;
    }

    public async Task<IReadOnlyList<TenantModuleContractDto>> ModulesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var repository=new PostgresSaasModuleRepository(connections,new NoOpCache());return await repository.ListContractsAsync(tenantId,null,null,cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformAuditDto>> AuditAsync(Guid? tenantId, int pageSize, CancellationToken cancellationToken)
    {
        pageSize=Math.Clamp(pageSize,1,500);const string sql="SELECT id,actor_user_id,coalesce(target_tenant_id,tenant_id),coalesce(target_branch_id,branch_id),target_user_id,coalesce(module,'Unknown'),coalesce(action,operation),entity_name,entity_id,reason,correlation_id,created_at FROM barber.audit_logs WHERE (@tenant IS NULL OR coalesce(target_tenant_id,tenant_id)=@tenant) ORDER BY created_at DESC LIMIT @limit";await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.Add("tenant",NpgsqlDbType.Uuid).Value=(object?)tenantId??DBNull.Value;command.Parameters.AddWithValue("limit",pageSize);await using var reader=await command.ExecuteReaderAsync(cancellationToken);var items=new List<PlatformAuditDto>();while(await reader.ReadAsync(cancellationToken))items.Add(new(reader.GetGuid(0),reader.IsDBNull(1)?null:reader.GetGuid(1),reader.IsDBNull(2)?null:reader.GetGuid(2),reader.IsDBNull(3)?null:reader.GetGuid(3),reader.IsDBNull(4)?null:reader.GetGuid(4),reader.GetString(5),reader.GetString(6),reader.GetString(7),reader.IsDBNull(8)?null:reader.GetGuid(8),reader.IsDBNull(9)?null:reader.GetString(9),reader.IsDBNull(10)?null:reader.GetString(10),reader.GetFieldValue<DateTimeOffset>(11)));return items;
    }

    private static async Task<IReadOnlyList<PlatformPlanMetricDto>> LoadPlans(NpgsqlConnection connection,CancellationToken ct){const string sql="SELECT p.id,p.name,count(s.tenant_id),coalesce(sum(CASE s.billing_cycle WHEN 'Annual' THEN p.annual_price/12 WHEN 'Monthly' THEN p.monthly_price ELSE 0 END),0) FROM barber.saas_plans p LEFT JOIN barber.tenant_subscriptions s ON s.plan_id=p.id AND s.status IN('Trial','Active','GracePeriod') GROUP BY p.id,p.name ORDER BY count(s.tenant_id) DESC";await using var command=new NpgsqlCommand(sql,connection);await using var reader=await command.ExecuteReaderAsync(ct);var result=new List<PlatformPlanMetricDto>();while(await reader.ReadAsync(ct))result.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetInt64(2),reader.GetDecimal(3)));return result;}
    private static async Task<IReadOnlyList<PlatformModuleMetricDto>> LoadModules(NpgsqlConnection connection,long activeTenants,CancellationToken ct){const string sql="""
        WITH access AS (
          SELECT pm.module_id,s.tenant_id,s.status
          FROM barber.tenant_subscriptions s JOIN barber.saas_plan_modules pm ON pm.plan_id=s.plan_id
          WHERE s.status IN('Trial','Active','GracePeriod') AND pm.is_included
          UNION ALL
          SELECT module_id,tenant_id,status FROM barber.tenant_module_contracts WHERE deleted_at IS NULL
        ), access_stats AS (
          SELECT module_id,count(DISTINCT tenant_id) contracted,count(DISTINCT tenant_id) FILTER(WHERE status='Trial') trial,
            count(DISTINCT tenant_id) FILTER(WHERE status IN('Active','GracePeriod')) active,count(DISTINCT tenant_id) FILTER(WHERE status='Suspended') suspended
          FROM access GROUP BY module_id
        ), usage AS (
          SELECT module_id,sum(active_users) active_users,sum(requests) requests,sum(relevant_operations) operations
          FROM barber.tenant_module_usage_daily WHERE usage_date>=date_trunc('month',now())::date GROUP BY module_id
        ), revenue AS (
          SELECT module_id,sum(CASE billing_cycle WHEN 'Annual' THEN contracted_price/12 WHEN 'Monthly' THEN contracted_price ELSE 0 END) revenue
          FROM barber.tenant_module_contracts WHERE status IN('Trial','Active','GracePeriod') AND deleted_at IS NULL AND contracted_price IS NOT NULL GROUP BY module_id
        )
        SELECT m.id,m.module_key,m.name,coalesce(a.contracted,0),coalesce(a.trial,0),coalesce(a.active,0),coalesce(a.suspended,0),
          coalesce(u.active_users,0),coalesce(u.requests,0),coalesce(u.operations,0),coalesce(r.revenue,0)
        FROM barber.saas_modules m LEFT JOIN access_stats a ON a.module_id=m.id LEFT JOIN usage u ON u.module_id=m.id LEFT JOIN revenue r ON r.module_id=m.id
        WHERE m.status<>'Archived' ORDER BY m.display_order
        """;await using var command=new NpgsqlCommand(sql,connection);await using var reader=await command.ExecuteReaderAsync(ct);var result=new List<PlatformModuleMetricDto>();while(await reader.ReadAsync(ct)){var active=reader.GetInt64(5);result.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.GetInt64(3),reader.GetInt64(4),active,reader.GetInt64(6),reader.GetInt64(7),reader.GetInt64(8),reader.GetInt64(9),activeTenants==0?0:Math.Round(active*100m/activeTenants,2),0,reader.GetDecimal(10)));}return result;}
    private static void Add(NpgsqlCommand command,string name,string? value)=>command.Parameters.Add(name,NpgsqlDbType.Text).Value=(object?)value??DBNull.Value;
    private sealed class NoOpCache:IEffectiveAccessCache{public bool TryGet(string key,out ModuleAccessDecision decision){decision=default!;return false;}public void Set(string key,ModuleAccessDecision decision,TimeSpan lifetime){}public void Invalidate(Guid tenantId,Guid? userId,Guid? branchId){}}
}
