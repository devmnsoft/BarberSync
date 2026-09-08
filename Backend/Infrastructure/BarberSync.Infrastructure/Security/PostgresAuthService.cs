using System.Security.Cryptography;
using System.Text;
using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using BarberSync.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using BarberSync.Application.Abstractions.Saas;

namespace BarberSync.Infrastructure.Security;

public sealed class PostgresAuthService(
    IDbConnectionFactory connectionFactory,
    ITokenService tokenService,
    IPasswordHasher<AuthUser> passwordHasher,
    IOptions<JwtOptions> jwtOptions,
    IModuleEntitlementService entitlements) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string? ipAddress, string correlationId, CancellationToken cancellationToken)
    {
        await using var connection=(NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        UserRecord? user=null;
        if(string.IsNullOrWhiteSpace(request.EffectiveTenantIdentifier)) user=await FindPlatformUserAsync(connection,request.EffectiveUserIdentifier,cancellationToken);
        user??=await FindTenantUserAsync(connection,request.EffectiveUserIdentifier,request.EffectiveTenantIdentifier,cancellationToken);
        if(user is null||string.IsNullOrWhiteSpace(user.PasswordHash)||passwordHasher.VerifyHashedPassword(user.AuthUser,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed)
        {
            await AuditAsync(connection,null,"LoginFailed",request.EffectiveUserIdentifier,ipAddress,correlationId,cancellationToken);
            return null;
        }
        user=await WithModules(user,cancellationToken);
        var refresh=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await using var transaction=await connection.BeginTransactionAsync(cancellationToken);
        await StoreRefreshAsync(connection,transaction,user.AuthUser,refresh,cancellationToken);
        await AuditAsync(connection,transaction,"LoginSucceeded",request.EffectiveUserIdentifier,ipAddress,correlationId,cancellationToken,user.AuthUser);
        await transaction.CommitAsync(cancellationToken);
        return CreateResponse(user.AuthUser,refresh);
    }

    public async Task<LoginResponseDto?> RefreshAsync(string refreshToken,string? ipAddress,string correlationId,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(refreshToken))return null;
        await using var connection=(NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        var user=await FindByRefreshTokenAsync(connection,Hash(refreshToken),cancellationToken);
        if(user is null)return null;
        user=await WithModules(user,cancellationToken);
        var replacement=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await using var transaction=await connection.BeginTransactionAsync(cancellationToken);
        await StoreRefreshAsync(connection,transaction,user,replacement,cancellationToken);
        await AuditAsync(connection,transaction,"TokenRefreshed",user.Email,ipAddress,correlationId,cancellationToken,user);
        await transaction.CommitAsync(cancellationToken);
        return CreateResponse(user,replacement);
    }

    public async Task LogoutAsync(string refreshToken,string? ipAddress,string correlationId,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(refreshToken))return;
        await using var connection=(NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        var user=await FindByRefreshTokenAsync(connection,Hash(refreshToken),cancellationToken);
        if(user is null)return;
        await using var transaction=await connection.BeginTransactionAsync(cancellationToken);
        var table=user.IsPlatformUser?"platform_users":"users";
        await using(var command=new NpgsqlCommand($"UPDATE barber.{table} SET refresh_token_hash=NULL,refresh_token_expires_at=NULL,updated_at=now() WHERE id=@id",connection,transaction)){command.Parameters.AddWithValue("id",user.Id);await command.ExecuteNonQueryAsync(cancellationToken);}
        await AuditAsync(connection,transaction,"Logout",user.Email,ipAddress,correlationId,cancellationToken,user);
        await transaction.CommitAsync(cancellationToken);
    }

    private LoginResponseDto CreateResponse(AuthUser user,string refresh)=>new(tokenService.Generate(user),refresh,DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes));

    private async Task<UserRecord> WithModules(UserRecord user,CancellationToken cancellationToken)
    {
        if(user.AuthUser.IsPlatformUser||user.AuthUser.TenantId==Guid.Empty)return user;
        var access=await entitlements.GetEffectiveAccessAsync(user.AuthUser.Id,user.AuthUser.TenantId,user.AuthUser.BranchId,cancellationToken);
        return user with{AuthUser=user.AuthUser with{Modules=access.Values.Where(x=>x.Allowed).Select(x=>x.ModuleKey).ToArray()}};
    }

    private async Task<AuthUser> WithModules(AuthUser user,CancellationToken cancellationToken)
    {
        if(user.IsPlatformUser||user.TenantId==Guid.Empty)return user;
        var access=await entitlements.GetEffectiveAccessAsync(user.Id,user.TenantId,user.BranchId,cancellationToken);
        return user with{Modules=access.Values.Where(x=>x.Allowed).Select(x=>x.ModuleKey).ToArray()};
    }

    private static async Task<UserRecord?> FindTenantUserAsync(NpgsqlConnection connection,string userIdentifier,string? tenantIdentifier,CancellationToken ct)
    {
        const string sql="""
            SELECT u.id,u.tenant_id,u.branch_id,u.email,u.password_hash,t.name,b.name
            FROM barber.users u JOIN barber.tenants t ON t.id=u.tenant_id JOIN barber.branches b ON b.id=u.branch_id AND b.tenant_id=t.id
            WHERE (lower(u.email)=lower(@user_identifier) OR u.cpf_normalized=@user_normalized)
              AND u.is_active AND u.status='Active' AND u.deleted_at IS NULL
              AND t.is_active AND t.status='Active' AND t.deleted_at IS NULL
              AND b.is_active AND b.status='Active' AND b.deleted_at IS NULL
              AND (@tenant_identifier IS NULL OR lower(t.slug)=lower(@tenant_identifier) OR lower(t.institutional_email)=lower(@tenant_identifier)
                OR t.document_number_normalized=@tenant_normalized OR regexp_replace(coalesce(t.document,''),'[^0-9]','','g')=@tenant_normalized)
            ORDER BY u.created_at LIMIT 2
            """;
        await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("user_identifier",userIdentifier.Trim());command.Parameters.AddWithValue("user_normalized",BrazilianDocument.Normalize(userIdentifier));command.Parameters.Add("tenant_identifier",NpgsqlDbType.Text).Value=string.IsNullOrWhiteSpace(tenantIdentifier)?DBNull.Value:tenantIdentifier.Trim();command.Parameters.AddWithValue("tenant_normalized",BrazilianDocument.Normalize(tenantIdentifier));
        var records=new List<(Guid Id,Guid Tenant,Guid Branch,string Email,string Hash,string TenantName,string BranchName)>();await using var reader=await command.ExecuteReaderAsync(ct);while(await reader.ReadAsync(ct))records.Add((reader.GetGuid(0),reader.GetGuid(1),reader.GetGuid(2),reader.GetString(3),reader.GetString(4),reader.GetString(5),reader.GetString(6)));if(records.Count!=1)return null;await reader.DisposeAsync();var value=records[0];return new(await LoadTenantClaimsAsync(connection,value.Id,value.Tenant,value.Branch,value.Email,ct,value.TenantName,value.BranchName),value.Hash);
    }

    private static async Task<UserRecord?> FindPlatformUserAsync(NpgsqlConnection connection,string identifier,CancellationToken ct)
    {
        const string sql="SELECT id,email,password_hash FROM barber.platform_users WHERE (lower(email)=lower(@identifier) OR cpf_normalized=@normalized) AND is_active AND status='Active' AND deleted_at IS NULL LIMIT 2";
        await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("identifier",identifier.Trim());command.Parameters.AddWithValue("normalized",BrazilianDocument.Normalize(identifier));var records=new List<(Guid Id,string Email,string Hash)>();await using var reader=await command.ExecuteReaderAsync(ct);while(await reader.ReadAsync(ct))records.Add((reader.GetGuid(0),reader.GetString(1),reader.GetString(2)));if(records.Count!=1)return null;await reader.DisposeAsync();var item=records[0];return new(await LoadPlatformClaimsAsync(connection,item.Id,item.Email,ct),item.Hash);
    }

    private static async Task<AuthUser?> FindByRefreshTokenAsync(NpgsqlConnection connection,string hash,CancellationToken ct)
    {
        const string platformSql="SELECT id,email FROM barber.platform_users WHERE refresh_token_hash=@hash AND refresh_token_expires_at>now() AND is_active AND status='Active' AND deleted_at IS NULL";
        await using(var command=new NpgsqlCommand(platformSql,connection)){command.Parameters.AddWithValue("hash",hash);await using var reader=await command.ExecuteReaderAsync(ct);if(await reader.ReadAsync(ct)){var id=reader.GetGuid(0);var email=reader.GetString(1);await reader.DisposeAsync();return await LoadPlatformClaimsAsync(connection,id,email,ct);}}
        const string tenantSql="SELECT u.id,u.tenant_id,u.branch_id,u.email,t.name,b.name FROM barber.users u JOIN barber.tenants t ON t.id=u.tenant_id JOIN barber.branches b ON b.id=u.branch_id WHERE u.refresh_token_hash=@hash AND u.refresh_token_expires_at>now() AND u.is_active AND u.status='Active' AND u.deleted_at IS NULL";
        await using(var command=new NpgsqlCommand(tenantSql,connection)){command.Parameters.AddWithValue("hash",hash);await using var reader=await command.ExecuteReaderAsync(ct);if(!await reader.ReadAsync(ct))return null;var values=(reader.GetGuid(0),reader.GetGuid(1),reader.GetGuid(2),reader.GetString(3),reader.GetString(4),reader.GetString(5));await reader.DisposeAsync();return await LoadTenantClaimsAsync(connection,values.Item1,values.Item2,values.Item3,values.Item4,ct,values.Item5,values.Item6);}
    }

    private static async Task<AuthUser> LoadTenantClaimsAsync(NpgsqlConnection connection,Guid id,Guid tenant,Guid branch,string email,CancellationToken ct,string? tenantName=null,string? branchName=null)
    {
        const string sql="SELECT DISTINCT r.code,p.code FROM barber.user_roles ur JOIN barber.roles r ON r.id=ur.role_id LEFT JOIN barber.role_permissions rp ON rp.role_id=r.id LEFT JOIN barber.permissions p ON p.id=rp.permission_id WHERE ur.user_id=@id";await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("id",id);var roles=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var permissions=new HashSet<string>(StringComparer.OrdinalIgnoreCase);await using var reader=await command.ExecuteReaderAsync(ct);while(await reader.ReadAsync(ct)){roles.Add(reader.GetString(0));if(!reader.IsDBNull(1))permissions.Add(reader.GetString(1));}await reader.DisposeAsync();
        const string direct="SELECT p.code,up.is_allowed FROM barber.user_permissions up JOIN barber.permissions p ON p.id=up.permission_id WHERE up.user_id=@id";await using var directCommand=new NpgsqlCommand(direct,connection);directCommand.Parameters.AddWithValue("id",id);await using var directReader=await directCommand.ExecuteReaderAsync(ct);while(await directReader.ReadAsync(ct)){var code=directReader.GetString(0);if(directReader.GetBoolean(1))permissions.Add(code);else permissions.Remove(code);}return new(id,tenant,branch,email,roles.ToArray(),permissions.ToArray(),TenantName:tenantName,BranchName:branchName);
    }

    private static async Task<AuthUser> LoadPlatformClaimsAsync(NpgsqlConnection connection,Guid id,string email,CancellationToken ct)
    {
        const string sql="SELECT DISTINCT r.code,p.code FROM barber.platform_user_roles ur JOIN barber.platform_roles r ON r.id=ur.role_id LEFT JOIN barber.platform_role_permissions rp ON rp.role_id=r.id LEFT JOIN barber.platform_permissions p ON p.id=rp.permission_id WHERE ur.user_id=@id";await using var command=new NpgsqlCommand(sql,connection);command.Parameters.AddWithValue("id",id);var roles=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var permissions=new HashSet<string>(StringComparer.OrdinalIgnoreCase);await using var reader=await command.ExecuteReaderAsync(ct);while(await reader.ReadAsync(ct)){roles.Add(reader.GetString(0));if(!reader.IsDBNull(1))permissions.Add(reader.GetString(1));}return new(id,Guid.Empty,Guid.Empty,email,roles.ToArray(),permissions.ToArray(),true,id);
    }

    private async Task StoreRefreshAsync(NpgsqlConnection connection,NpgsqlTransaction transaction,AuthUser user,string token,CancellationToken ct)
    {
        var table=user.IsPlatformUser?"platform_users":"users";await using var command=new NpgsqlCommand($"UPDATE barber.{table} SET refresh_token_hash=@hash,refresh_token_expires_at=@expires,last_login_at=now(),updated_at=now() WHERE id=@id",connection,transaction);command.Parameters.AddWithValue("hash",Hash(token));command.Parameters.AddWithValue("expires",DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays));command.Parameters.AddWithValue("id",user.Id);await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task AuditAsync(NpgsqlConnection connection,NpgsqlTransaction? transaction,string action,string identifier,string? ip,string correlation,CancellationToken ct,AuthUser? user=null)
    {
        const string sql="INSERT INTO barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,correlation_id,module,action,description,metadata,actor_user_id) VALUES(@id,@tenant,@branch,@user,@action,'users',@entity,@correlation,'Auth',@action,@description,jsonb_build_object('ip_address',@ip,'identifier_hash',@identifier_hash),@actor)";await using var command=new NpgsqlCommand(sql,connection,transaction);command.Parameters.AddWithValue("id",Guid.NewGuid());command.Parameters.Add("tenant",NpgsqlDbType.Uuid).Value=user is {IsPlatformUser:false}?user.TenantId:DBNull.Value;command.Parameters.Add("branch",NpgsqlDbType.Uuid).Value=user is {IsPlatformUser:false}?user.BranchId:DBNull.Value;command.Parameters.Add("user",NpgsqlDbType.Uuid).Value=user is {IsPlatformUser:false}?user.Id:DBNull.Value;command.Parameters.Add("entity",NpgsqlDbType.Uuid).Value=user is null?DBNull.Value:user.Id;command.Parameters.Add("actor",NpgsqlDbType.Uuid).Value=user is {IsPlatformUser:true}?user.Id:DBNull.Value;command.Parameters.AddWithValue("action",action);command.Parameters.AddWithValue("correlation",correlation);command.Parameters.AddWithValue("description",action=="LoginFailed"?"Tentativa de login não concluída.":"Evento de autenticação concluído.");command.Parameters.AddWithValue("ip",ip??"unknown");command.Parameters.AddWithValue("identifier_hash",Fingerprint(identifier));await command.ExecuteNonQueryAsync(ct);
    }

    private static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static string Fingerprint(string value)=>Hash(value.Trim().ToLowerInvariant())[..16];
    private sealed record UserRecord(AuthUser AuthUser,string PasswordHash);
}
