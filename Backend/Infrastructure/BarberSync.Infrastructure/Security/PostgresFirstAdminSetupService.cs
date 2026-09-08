using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace BarberSync.Infrastructure.Security;

public sealed class PostgresFirstAdminSetupService(
    IDbConnectionFactory connectionFactory,
    IPasswordHasher<AuthUser> passwordHasher) : IFirstAdminSetupService
{
    public async Task<FirstAdminSetupResult> CreateAsync(FirstAdminRequestDto request, string correlationId, CancellationToken cancellationToken)
    {
        await using var connection = (NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        await using (var setupLock = new NpgsqlCommand("SELECT pg_advisory_xact_lock(4200202402)", connection, transaction))
            await setupLock.ExecuteNonQueryAsync(cancellationToken);

        await using (var activeUsers = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM barber.platform_users WHERE is_active AND deleted_at IS NULL)", connection, transaction))
        {
            if ((bool)(await activeUsers.ExecuteScalarAsync(cancellationToken) ?? false))
                return new(false, "A configuração inicial já foi concluída.");
        }

        var tenantId = await FindIdAsync(connection, transaction,
            "SELECT id FROM barber.tenants WHERE lower(slug)=lower(@value) LIMIT 1", request.TenantSlug, cancellationToken) ?? Guid.NewGuid();
        var branchId = await FindIdAsync(connection, transaction,
            "SELECT id FROM barber.branches WHERE tenant_id=@tenant_id AND lower(code)=lower(@value) AND deleted_at IS NULL LIMIT 1",
            request.BranchCode, cancellationToken, tenantId) ?? Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerRoleId = Guid.Parse("20000000-0000-4000-8000-000000000001");
        var platformUserId = Guid.NewGuid();
        var platformRoleId = Guid.Parse("40000000-0000-4000-8000-000000000001");
        var tenantUser = new AuthUser(userId, tenantId, branchId, request.Email.Trim(), ["Owner"], []);
        var platformUser = new AuthUser(platformUserId, Guid.Empty, Guid.Empty, request.Email.Trim(), ["SuperAdmin"], [], true, platformUserId);
        var tenantPasswordHash = passwordHasher.HashPassword(tenantUser, request.Password);
        var platformPasswordHash = passwordHasher.HashPassword(platformUser, request.Password);

        const string sql = """
            INSERT INTO barber.tenants(id,slug,name,institutional_email) VALUES(@tenant_id,lower(@slug),@tenant_name,lower(@email)) ON CONFLICT DO NOTHING;
            INSERT INTO barber.branches(id,tenant_id,name,code) VALUES(@branch_id,@tenant_id,@branch_name,upper(@branch_code)) ON CONFLICT DO NOTHING;
            INSERT INTO barber.users(id,tenant_id,branch_id,email,password_hash,full_name)
              VALUES(@user_id,@tenant_id,@branch_id,lower(@email),@tenant_password_hash,@full_name);
            INSERT INTO barber.user_roles(user_id,role_id) VALUES(@user_id,@owner_role);
            INSERT INTO barber.platform_users(id,email,full_name,password_hash) VALUES(@platform_user_id,lower(@email),@full_name,@platform_password_hash);
            INSERT INTO barber.platform_user_roles(user_id,role_id,assigned_by) VALUES(@platform_user_id,@platform_role,@platform_user_id);
            INSERT INTO barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,correlation_id,module,action,description,actor_user_id,target_tenant_id,target_branch_id)
              VALUES(@audit_id,@tenant_id,@branch_id,@user_id,'FirstPlatformAdminCreated','platform_users',@platform_user_id,@correlation_id,'Setup','FirstPlatformAdminCreated','Primeiro operador da plataforma e Owner do tenant criados com segurança.',@platform_user_id,@tenant_id,@branch_id);
            """;
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("slug", request.TenantSlug.Trim());
        command.Parameters.AddWithValue("tenant_name", request.FullName.Trim());
        command.Parameters.AddWithValue("branch_id", branchId);
        command.Parameters.AddWithValue("branch_name", "Unidade principal");
        command.Parameters.AddWithValue("branch_code", request.BranchCode.Trim());
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("email", request.Email.Trim());
        command.Parameters.AddWithValue("tenant_password_hash", tenantPasswordHash);
        command.Parameters.AddWithValue("platform_password_hash", platformPasswordHash);
        command.Parameters.AddWithValue("platform_user_id", platformUserId);
        command.Parameters.AddWithValue("full_name", request.FullName.Trim());
        command.Parameters.AddWithValue("owner_role", ownerRoleId);
        command.Parameters.AddWithValue("platform_role", platformRoleId);
        command.Parameters.AddWithValue("audit_id", Guid.NewGuid());
        command.Parameters.AddWithValue("correlation_id", correlationId);
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(true, "Primeiro Super Administrador da plataforma e Owner do tenant criados. O endpoint de configuração foi desativado.");
    }

    private static async Task<Guid?> FindIdAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql, string value, CancellationToken ct, Guid? tenantId = null)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("value", value.Trim());
        if (tenantId.HasValue) command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        var result = await command.ExecuteScalarAsync(ct);
        return result is Guid id ? id : null;
    }
}
