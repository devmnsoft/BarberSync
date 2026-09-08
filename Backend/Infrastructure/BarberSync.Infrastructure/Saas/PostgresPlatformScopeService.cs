using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using BarberSync.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BarberSync.Infrastructure.Saas;

public sealed class PostgresPlatformScopeService(
    IDbConnectionFactory connections,
    ITokenService tokens,
    IOptions<JwtOptions> jwtOptions) : IPlatformScopeService
{
    public async Task<PlatformScopeTokenDto?> StartAsync(Guid actorUserId, StartPlatformScopeRequest request, string? ipAddress, string correlationId, CancellationToken cancellationToken)
    {
        await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);
        await using var transaction=await connection.BeginTransactionAsync(cancellationToken);
        var actor=await LoadActor(connection,transaction,actorUserId,cancellationToken);
        if(actor is null||(!actor.Roles.Contains("SuperAdmin")&&!actor.Permissions.Contains("Platform.Scope.Enter")))return null;
        var branchId=request.BranchId??await FirstBranch(connection,transaction,request.TenantId,cancellationToken);
        if(!branchId.HasValue)return null;
        const string validate="SELECT t.name,b.name FROM barber.tenants t JOIN barber.branches b ON b.tenant_id=t.id WHERE t.id=@tenant AND b.id=@branch AND t.is_active AND t.status='Active' AND t.deleted_at IS NULL AND b.is_active AND b.status='Active' AND b.deleted_at IS NULL";
        string tenantName,branchName;await using(var check=new NpgsqlCommand(validate,connection,transaction)){check.Parameters.AddWithValue("tenant",request.TenantId);check.Parameters.AddWithValue("branch",branchId.Value);await using var reader=await check.ExecuteReaderAsync(cancellationToken);if(!await reader.ReadAsync(cancellationToken))return null;tenantName=reader.GetString(0);branchName=reader.GetString(1);}
        var id=Guid.NewGuid();var expires=DateTimeOffset.UtcNow.AddMinutes(Math.Min(jwtOptions.Value.AccessTokenMinutes,15));
        const string sql="""
            INSERT INTO barber.platform_scope_sessions(id,actor_user_id,tenant_id,branch_id,reason,ip_address,correlation_id,expires_at)
            VALUES(@id,@actor,@tenant,@branch,@reason,cast(@ip AS inet),@correlation,@expires);
            INSERT INTO barber.audit_logs(id,tenant_id,branch_id,operation,entity_name,entity_id,correlation_id,module,action,description,actor_user_id,target_tenant_id,target_branch_id,reason,scope_session_id)
            VALUES(gen_random_uuid(),@tenant,@branch,'PlatformContextStarted','platform_scope_sessions',@id,@correlation,'Platform','PlatformContextStarted','Operador entrou no contexto controlado do cliente.',@actor,@tenant,@branch,@reason,@id)
            """;
        await using(var command=new NpgsqlCommand(sql,connection,transaction)){command.Parameters.AddWithValue("id",id);command.Parameters.AddWithValue("actor",actorUserId);command.Parameters.AddWithValue("tenant",request.TenantId);command.Parameters.AddWithValue("branch",branchId.Value);command.Parameters.AddWithValue("reason",request.Reason.Trim());command.Parameters.AddWithValue("ip",ipAddress??"0.0.0.0");command.Parameters.AddWithValue("correlation",correlationId);command.Parameters.AddWithValue("expires",expires);await command.ExecuteNonQueryAsync(cancellationToken);}await transaction.CommitAsync(cancellationToken);
        var scoped=new AuthUser(actorUserId,request.TenantId,branchId.Value,actor.Email,actor.Roles,actor.Permissions,true,actorUserId,id,TenantName:tenantName,BranchName:branchName);
        return new(id,request.TenantId,branchId.Value,tokens.Generate(scoped),expires);
    }

    public async Task EndAsync(Guid actorUserId, Guid scopeSessionId, string correlationId, CancellationToken cancellationToken)
    {
        const string sql="""
            WITH ended AS (UPDATE barber.platform_scope_sessions SET status='Ended',ended_at=now() WHERE id=@id AND actor_user_id=@actor AND status='Active' RETURNING *)
            INSERT INTO barber.audit_logs(id,tenant_id,branch_id,operation,entity_name,entity_id,correlation_id,module,action,description,actor_user_id,target_tenant_id,target_branch_id,scope_session_id)
            SELECT gen_random_uuid(),tenant_id,branch_id,'PlatformContextEnded','platform_scope_sessions',id,@correlation,'Platform','PlatformContextEnded','Operador saiu do contexto controlado do cliente.',@actor,tenant_id,branch_id,id FROM ended
            """;await using var connection=(NpgsqlConnection)await connections.OpenConnectionAsync(cancellationToken);await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("id",scopeSessionId);command.Parameters.AddWithValue("actor",actorUserId);command.Parameters.AddWithValue("correlation",correlationId);await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Actor?> LoadActor(NpgsqlConnection connection,NpgsqlTransaction transaction,Guid id,CancellationToken ct){const string sql="SELECT u.email,ARRAY(SELECT r.code FROM barber.platform_user_roles ur JOIN barber.platform_roles r ON r.id=ur.role_id WHERE ur.user_id=u.id),ARRAY(SELECT DISTINCT p.code FROM barber.platform_user_roles ur JOIN barber.platform_role_permissions rp ON rp.role_id=ur.role_id JOIN barber.platform_permissions p ON p.id=rp.permission_id WHERE ur.user_id=u.id) FROM barber.platform_users u WHERE u.id=@id AND u.is_active AND u.status='Active' AND u.deleted_at IS NULL";await using var command=new NpgsqlCommand(sql,connection,transaction);command.Parameters.AddWithValue("id",id);await using var reader=await command.ExecuteReaderAsync(ct);return await reader.ReadAsync(ct)?new(reader.GetString(0),reader.GetFieldValue<string[]>(1),reader.GetFieldValue<string[]>(2)):null;}
    private static async Task<Guid?> FirstBranch(NpgsqlConnection connection,NpgsqlTransaction transaction,Guid tenant,CancellationToken ct){await using var command=new NpgsqlCommand("SELECT id FROM barber.branches WHERE tenant_id=@tenant AND is_active AND status='Active' AND deleted_at IS NULL ORDER BY created_at LIMIT 1",connection,transaction);command.Parameters.AddWithValue("tenant",tenant);var result=await command.ExecuteScalarAsync(ct);return result is Guid id?id:null;}
    private sealed record Actor(string Email,IReadOnlyList<string> Roles,IReadOnlyList<string> Permissions);
}
