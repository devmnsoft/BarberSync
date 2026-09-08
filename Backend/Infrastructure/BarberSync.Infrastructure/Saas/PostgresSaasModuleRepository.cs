using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using Npgsql;
using NpgsqlTypes;

namespace BarberSync.Infrastructure.Saas;

public sealed class PostgresSaasModuleRepository(
    IDbConnectionFactory connections,
    IEffectiveAccessCache accessCache) : ISaasModuleRepository
{
    public async Task<IReadOnlyList<SaasModuleListItemDto>> ListAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT m.id,m.module_key,m.name,m.description,m.category,m.status,m.display_order,m.icon_key,m.is_core,m.is_sellable,
              price.price,coalesce(price.currency,'BRL'),
              (SELECT count(DISTINCT c.tenant_id) FROM barber.tenant_module_contracts c WHERE c.module_id=m.id AND c.deleted_at IS NULL AND c.status IN('Trial','Active','GracePeriod')),
              ARRAY(SELECT dependency.module_key FROM barber.saas_module_dependencies d JOIN barber.saas_modules dependency ON dependency.id=d.depends_on_module_id WHERE d.module_id=m.id AND d.is_required ORDER BY dependency.display_order)
            FROM barber.saas_modules m
            LEFT JOIN LATERAL(SELECT p.price,p.currency FROM barber.saas_module_prices p WHERE p.module_id=m.id AND p.billing_cycle='Monthly' AND p.status='Active' AND p.valid_from<=now() AND (p.valid_until IS NULL OR p.valid_until>now()) ORDER BY p.valid_from DESC LIMIT 1) price ON true
            WHERE m.status<>'Archived' ORDER BY m.display_order,m.name
            """;
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<SaasModuleListItemDto>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(ReadModule(reader));
        return result;
    }

    public async Task<SaasModuleListItemDto?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT m.id,m.module_key,m.name,m.description,m.category,m.status,m.display_order,m.icon_key,m.is_core,m.is_sellable,
              price.price,coalesce(price.currency,'BRL'),
              (SELECT count(DISTINCT c.tenant_id) FROM barber.tenant_module_contracts c WHERE c.module_id=m.id AND c.deleted_at IS NULL AND c.status IN('Trial','Active','GracePeriod')),
              ARRAY(SELECT dependency.module_key FROM barber.saas_module_dependencies d JOIN barber.saas_modules dependency ON dependency.id=d.depends_on_module_id WHERE d.module_id=m.id AND d.is_required ORDER BY dependency.display_order)
            FROM barber.saas_modules m
            LEFT JOIN LATERAL(SELECT p.price,p.currency FROM barber.saas_module_prices p WHERE p.module_id=m.id AND p.billing_cycle='Monthly' AND p.status='Active' AND p.valid_from<=now() AND (p.valid_until IS NULL OR p.valid_until>now()) ORDER BY p.valid_from DESC LIMIT 1) price ON true
            WHERE m.id=@id
            """;
        await using var connection = (NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection); command.Parameters.AddWithValue("id",id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadModule(reader) : null;
    }

    public async Task<Guid> CreateAsync(CreateSaasModuleRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken)
    {
        const string sql = """
            WITH created AS (
              INSERT INTO barber.saas_modules(id,module_key,name,description,category,status,display_order,icon_key,is_core,is_sellable)
              VALUES(@id,@key,@name,@description,@category,@status,@display_order,@icon,@core,@sellable) RETURNING *
            )
            INSERT INTO barber.audit_logs(id,operation,entity_name,entity_id,correlation_id,module,action,description,after_data,actor_user_id)
            SELECT gen_random_uuid(),'PlatformModuleCreated','saas_modules',id,@correlation,'Platform','ModuleCreated','Módulo comercial criado.',to_jsonb(created),@actor FROM created
            """;
        var id=Guid.NewGuid();
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command=new NpgsqlCommand(sql,connection);
        command.Parameters.AddWithValue("id",id); Add(command,"key",request.ModuleKey.Trim().ToUpperInvariant()); Add(command,"name",request.Name.Trim()); Add(command,"description",request.Description); Add(command,"category",request.Category.Trim()); Add(command,"status",request.Status); command.Parameters.AddWithValue("display_order",request.DisplayOrder); Add(command,"icon",request.IconKey); command.Parameters.AddWithValue("core",request.IsCore); command.Parameters.AddWithValue("sellable",request.IsSellable); command.Parameters.AddWithValue("actor",actorUserId); Add(command,"correlation",correlationId);
        await command.ExecuteNonQueryAsync(cancellationToken); return id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateSaasModuleRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken)
    {
        const string sql = """
            WITH previous AS (SELECT * FROM barber.saas_modules WHERE id=@id FOR UPDATE), updated AS (
              UPDATE barber.saas_modules SET name=@name,description=@description,category=@category,status=@status,display_order=@display_order,icon_key=@icon,is_core=@core,is_sellable=@sellable,updated_at=now()
              WHERE id=@id RETURNING *)
            INSERT INTO barber.audit_logs(id,operation,entity_name,entity_id,correlation_id,module,action,description,before_data,after_data,actor_user_id)
            SELECT gen_random_uuid(),'PlatformModuleUpdated','saas_modules',updated.id,@correlation,'Platform','ModuleUpdated','Módulo comercial atualizado.',to_jsonb(previous),to_jsonb(updated),@actor FROM previous,updated
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var command=new NpgsqlCommand(sql,connection); command.Parameters.AddWithValue("id",id); Add(command,"name",request.Name.Trim()); Add(command,"description",request.Description); Add(command,"category",request.Category.Trim()); Add(command,"status",request.Status); command.Parameters.AddWithValue("display_order",request.DisplayOrder); Add(command,"icon",request.IconKey); command.Parameters.AddWithValue("core",request.IsCore); command.Parameters.AddWithValue("sellable",request.IsSellable); command.Parameters.AddWithValue("actor",actorUserId); Add(command,"correlation",correlationId);
        return await command.ExecuteNonQueryAsync(cancellationToken)>0;
    }

    public async Task<IReadOnlyList<SaasModulePriceDto>> ListPricesAsync(Guid? moduleId, CancellationToken cancellationToken)
    {
        const string sql="SELECT p.id,p.module_id,m.module_key,m.name,p.billing_cycle,p.currency,p.price,p.valid_from,p.valid_until,p.status FROM barber.saas_module_prices p JOIN barber.saas_modules m ON m.id=p.module_id WHERE (@module IS NULL OR p.module_id=@module) AND p.status<>'Archived' ORDER BY m.display_order,p.billing_cycle,p.valid_from DESC";
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken); await using var command=new NpgsqlCommand(sql,connection); command.Parameters.Add("module",NpgsqlDbType.Uuid).Value=(object?)moduleId??DBNull.Value; await using var reader=await command.ExecuteReaderAsync(cancellationToken); var result=new List<SaasModulePriceDto>();
        while(await reader.ReadAsync(cancellationToken)) result.Add(new(reader.GetGuid(0),reader.GetGuid(1),reader.GetString(2),reader.GetString(3),reader.GetString(4),reader.GetString(5),reader.GetDecimal(6),reader.GetFieldValue<DateTimeOffset>(7),reader.IsDBNull(8)?null:reader.GetFieldValue<DateTimeOffset>(8),reader.GetString(9))); return result;
    }

    public async Task<Guid> UpsertPriceAsync(Guid? id, UpsertSaasModulePriceRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken)
    {
        var priceId=id??Guid.NewGuid(); const string sql="""
            INSERT INTO barber.saas_module_prices(id,module_id,billing_cycle,currency,price,valid_from,valid_until,status)
            VALUES(@id,@module,@cycle,upper(@currency),@price,@from,@until,@status)
            ON CONFLICT(id) DO UPDATE SET module_id=excluded.module_id,billing_cycle=excluded.billing_cycle,currency=excluded.currency,price=excluded.price,valid_from=excluded.valid_from,valid_until=excluded.valid_until,status=excluded.status,updated_at=now();
            INSERT INTO barber.audit_logs(id,operation,entity_name,entity_id,correlation_id,module,action,description,actor_user_id)
            VALUES(gen_random_uuid(),'PlatformModulePriceSaved','saas_module_prices',@id,@correlation,'Platform','ModulePriceSaved','Preço de módulo salvo sem alterar contratos existentes.',@actor);
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken); await using var transaction=await connection.BeginTransactionAsync(cancellationToken); await using var command=new NpgsqlCommand(sql,connection,transaction); command.Parameters.AddWithValue("id",priceId); command.Parameters.AddWithValue("module",request.ModuleId); Add(command,"cycle",request.BillingCycle); Add(command,"currency",request.Currency); command.Parameters.AddWithValue("price",request.Price); command.Parameters.AddWithValue("from",request.ValidFrom); command.Parameters.Add("until",NpgsqlDbType.TimestampTz).Value=(object?)request.ValidUntil??DBNull.Value; Add(command,"status",request.Status); command.Parameters.AddWithValue("actor",actorUserId); Add(command,"correlation",correlationId); await command.ExecuteNonQueryAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return priceId;
    }

    public async Task<IReadOnlyList<TenantModuleContractDto>> ListContractsAsync(Guid? tenantId, Guid? moduleId, string? status, CancellationToken cancellationToken)
    {
        const string sql="SELECT c.id,c.tenant_id,t.name,c.module_id,m.module_key,m.name,c.status,c.billing_cycle,c.contracted_price,c.currency,c.starts_at,c.trial_ends_at,c.ends_at,c.suspended_at,c.cancelled_at FROM barber.tenant_module_contracts c JOIN barber.tenants t ON t.id=c.tenant_id JOIN barber.saas_modules m ON m.id=c.module_id WHERE c.deleted_at IS NULL AND (@tenant IS NULL OR c.tenant_id=@tenant) AND (@module IS NULL OR c.module_id=@module) AND (@status IS NULL OR c.status=@status) ORDER BY c.created_at DESC LIMIT 500";
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken); await using var command=new NpgsqlCommand(sql,connection); command.Parameters.Add("tenant",NpgsqlDbType.Uuid).Value=(object?)tenantId??DBNull.Value; command.Parameters.Add("module",NpgsqlDbType.Uuid).Value=(object?)moduleId??DBNull.Value; Add(command,"status",status); await using var reader=await command.ExecuteReaderAsync(cancellationToken); var result=new List<TenantModuleContractDto>(); while(await reader.ReadAsync(cancellationToken)) result.Add(ReadContract(reader)); return result;
    }

    public async Task<Guid> CreateContractAsync(CreateTenantModuleContractRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken)
    {
        var id=Guid.NewGuid(); const string dependencySql="""
            SELECT dependency.module_key FROM barber.saas_module_dependencies d JOIN barber.saas_modules dependency ON dependency.id=d.depends_on_module_id
            WHERE d.module_id=@module AND d.is_required AND NOT EXISTS(
              SELECT 1 FROM barber.tenant_module_contracts c WHERE c.tenant_id=@tenant AND c.module_id=d.depends_on_module_id AND c.deleted_at IS NULL AND c.status IN('Trial','Active','GracePeriod') AND c.starts_at<=now() AND (c.ends_at IS NULL OR c.ends_at>now())
              UNION ALL SELECT 1 FROM barber.tenant_subscriptions s JOIN barber.saas_plan_modules pm ON pm.plan_id=s.plan_id WHERE s.tenant_id=@tenant AND pm.module_id=d.depends_on_module_id AND pm.is_included AND s.status IN('Trial','Active','GracePeriod')) LIMIT 1
            """;
        const string insertSql="""
            INSERT INTO barber.tenant_module_contracts(id,tenant_id,module_id,status,billing_cycle,contracted_price,currency,starts_at,trial_ends_at,ends_at,reason,created_by,updated_by)
            VALUES(@id,@tenant,@module,@status,@cycle,@price,upper(@currency),@starts,@trial,@ends,@reason,@actor,@actor);
            INSERT INTO barber.audit_logs(id,tenant_id,operation,entity_name,entity_id,correlation_id,module,action,description,actor_user_id,target_tenant_id,reason)
            VALUES(gen_random_uuid(),@tenant,'ModuleContracted','tenant_module_contracts',@id,@correlation,'Platform','ModuleContracted','Contrato de módulo criado.',@actor,@tenant,@reason);
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken); await using var transaction=await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,cancellationToken); await using(var lockCommand=new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended(@key,0))",connection,transaction)){lockCommand.Parameters.AddWithValue("key",$"{request.TenantId:N}:{request.ModuleId:N}");await lockCommand.ExecuteNonQueryAsync(cancellationToken);} await using(var dependencies=new NpgsqlCommand(dependencySql,connection,transaction)){dependencies.Parameters.AddWithValue("tenant",request.TenantId);dependencies.Parameters.AddWithValue("module",request.ModuleId);var missing=await dependencies.ExecuteScalarAsync(cancellationToken) as string;if(missing is not null)throw new InvalidOperationException($"O módulo depende de {missing}, que ainda não está ativo.");}
        await using(var command=new NpgsqlCommand(insertSql,connection,transaction)){command.Parameters.AddWithValue("id",id);command.Parameters.AddWithValue("tenant",request.TenantId);command.Parameters.AddWithValue("module",request.ModuleId);Add(command,"status",request.Status);Add(command,"cycle",request.BillingCycle);command.Parameters.Add("price",NpgsqlDbType.Numeric).Value=(object?)request.ContractedPrice??DBNull.Value;Add(command,"currency",request.Currency);command.Parameters.AddWithValue("starts",request.StartsAt);command.Parameters.Add("trial",NpgsqlDbType.TimestampTz).Value=(object?)request.TrialEndsAt??DBNull.Value;command.Parameters.Add("ends",NpgsqlDbType.TimestampTz).Value=(object?)request.EndsAt??DBNull.Value;Add(command,"reason",request.Reason);command.Parameters.AddWithValue("actor",actorUserId);Add(command,"correlation",correlationId);await command.ExecuteNonQueryAsync(cancellationToken);} await transaction.CommitAsync(cancellationToken); accessCache.Invalidate(request.TenantId,null,null); return id;
    }

    public async Task<bool> ChangeContractStatusAsync(Guid id, ChangeModuleContractStatusRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken)
    {
        const string sql="""
            WITH previous AS (SELECT * FROM barber.tenant_module_contracts WHERE id=@id AND deleted_at IS NULL FOR UPDATE), updated AS (
              UPDATE barber.tenant_module_contracts SET status=@status,reason=@reason,updated_by=@actor,updated_at=now(),suspended_at=CASE WHEN @status='Suspended' THEN now() ELSE suspended_at END,cancelled_at=CASE WHEN @status='Cancelled' THEN now() ELSE cancelled_at END WHERE id=@id AND deleted_at IS NULL RETURNING *)
            INSERT INTO barber.audit_logs(id,tenant_id,operation,entity_name,entity_id,correlation_id,module,action,description,before_data,after_data,actor_user_id,target_tenant_id,reason)
            SELECT gen_random_uuid(),updated.tenant_id,'ModuleContractStatusChanged','tenant_module_contracts',updated.id,@correlation,'Platform','ModuleContractStatusChanged','Situação do contrato alterada.',to_jsonb(previous),to_jsonb(updated),@actor,updated.tenant_id,@reason FROM previous,updated RETURNING tenant_id
            """;
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var transaction=await connection.BeginTransactionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection,transaction);command.Parameters.AddWithValue("id",id);Add(command,"status",request.Status);Add(command,"reason",request.Reason);command.Parameters.AddWithValue("actor",actorUserId);Add(command,"correlation",correlationId);var tenant=await command.ExecuteScalarAsync(cancellationToken);await transaction.CommitAsync(cancellationToken);if(tenant is Guid tenantId){accessCache.Invalidate(tenantId,null,null);return true;}return false;
    }

    private static SaasModuleListItemDto ReadModule(NpgsqlDataReader reader)=>new(reader.GetGuid(0),reader.GetString(1),reader.GetString(2),reader.IsDBNull(3)?null:reader.GetString(3),reader.GetString(4),reader.GetString(5),reader.GetInt32(6),reader.IsDBNull(7)?null:reader.GetString(7),reader.GetBoolean(8),reader.GetBoolean(9),reader.IsDBNull(10)?null:reader.GetDecimal(10),reader.GetString(11),Convert.ToInt32(reader.GetInt64(12)),reader.GetFieldValue<string[]>(13));
    private static TenantModuleContractDto ReadContract(NpgsqlDataReader reader)=>new(reader.GetGuid(0),reader.GetGuid(1),reader.GetString(2),reader.GetGuid(3),reader.GetString(4),reader.GetString(5),reader.GetString(6),reader.GetString(7),reader.IsDBNull(8)?null:reader.GetDecimal(8),reader.GetString(9),reader.GetFieldValue<DateTimeOffset>(10),reader.IsDBNull(11)?null:reader.GetFieldValue<DateTimeOffset>(11),reader.IsDBNull(12)?null:reader.GetFieldValue<DateTimeOffset>(12),reader.IsDBNull(13)?null:reader.GetFieldValue<DateTimeOffset>(13),reader.IsDBNull(14)?null:reader.GetFieldValue<DateTimeOffset>(14));
    private static void Add(NpgsqlCommand command,string name,string? value)=>command.Parameters.Add(name,NpgsqlDbType.Text).Value=(object?)value??DBNull.Value;
}
