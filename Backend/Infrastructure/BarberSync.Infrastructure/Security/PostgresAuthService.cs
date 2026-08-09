using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace BarberSync.Infrastructure.Security;

public sealed class PostgresAuthService(
    IDbConnectionFactory connectionFactory,
    ITokenService tokenService,
    IPasswordHasher<AuthUser> passwordHasher,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string? ipAddress, string correlationId, CancellationToken cancellationToken)
    {
        await using var connection = (NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        var user = await FindByCredentialsAsync(connection, request.Email, request.TenantSlug, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash) ||
            passwordHasher.VerifyHashedPassword(user.AuthUser, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            await AuditAsync(connection, null, "LoginFailed", request.Email, ipAddress, correlationId, cancellationToken);
            return null;
        }

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await StoreRefreshAsync(connection, transaction, user.AuthUser.Id, refreshToken, cancellationToken);
        await AuditAsync(connection, transaction, "LoginSucceeded", user.AuthUser.Email, ipAddress, correlationId, cancellationToken, user.AuthUser);
        await transaction.CommitAsync(cancellationToken);
        return CreateResponse(user.AuthUser, refreshToken);
    }

    public async Task<LoginResponseDto?> RefreshAsync(string refreshToken, string? ipAddress, string correlationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        await using var connection = (NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        var user = await FindByRefreshTokenAsync(connection, Hash(refreshToken), cancellationToken);
        if (user is null) return null;
        var replacement = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await StoreRefreshAsync(connection, transaction, user.Id, replacement, cancellationToken);
        await AuditAsync(connection, transaction, "TokenRefreshed", user.Email, ipAddress, correlationId, cancellationToken, user);
        await transaction.CommitAsync(cancellationToken);
        return CreateResponse(user, replacement);
    }

    public async Task LogoutAsync(string refreshToken, string? ipAddress, string correlationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        await using var connection = (NpgsqlConnection)await connectionFactory.OpenConnectionAsync(cancellationToken);
        var user = await FindByRefreshTokenAsync(connection, Hash(refreshToken), cancellationToken);
        if (user is null) return;
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("UPDATE barber.users SET refresh_token_hash=NULL, refresh_token_expires_at=NULL, updated_at=now() WHERE id=@id", connection, transaction);
        command.Parameters.AddWithValue("id", user.Id);
        await command.ExecuteNonQueryAsync(cancellationToken);
        await AuditAsync(connection, transaction, "Logout", user.Email, ipAddress, correlationId, cancellationToken, user);
        await transaction.CommitAsync(cancellationToken);
    }

    private LoginResponseDto CreateResponse(AuthUser user, string refresh) =>
        new(tokenService.Generate(user), refresh, DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes));

    private async Task<UserRecord?> FindByCredentialsAsync(NpgsqlConnection connection, string email, string? tenantSlug, CancellationToken ct)
    {
        const string sql = """
            SELECT u.id,u.tenant_id,u.branch_id,u.email,u.password_hash
            FROM barber.users u JOIN barber.tenants t ON t.id=u.tenant_id
            WHERE lower(u.email)=lower(@email) AND u.is_active AND u.deleted_at IS NULL
              AND t.is_active AND (@tenant_slug IS NULL OR lower(t.slug)=lower(@tenant_slug))
            ORDER BY u.created_at LIMIT 2
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("email", email.Trim());
        command.Parameters.Add("tenant_slug", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)tenantSlug?.Trim() ?? DBNull.Value;
        var records = new List<(Guid Id, Guid Tenant, Guid Branch, string Email, string Hash)>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) records.Add((reader.GetGuid(0), reader.GetGuid(1), reader.GetGuid(2), reader.GetString(3), reader.GetString(4)));
        if (records.Count != 1) return null;
        await reader.DisposeAsync();
        var value = records[0];
        var authUser = await LoadClaimsAsync(connection, value.Id, value.Tenant, value.Branch, value.Email, ct);
        return new UserRecord(authUser, value.Hash);
    }

    private async Task<AuthUser?> FindByRefreshTokenAsync(NpgsqlConnection connection, string hash, CancellationToken ct)
    {
        const string sql = "SELECT id,tenant_id,branch_id,email FROM barber.users WHERE refresh_token_hash=@hash AND refresh_token_expires_at>now() AND is_active AND deleted_at IS NULL";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("hash", hash);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var values = (reader.GetGuid(0), reader.GetGuid(1), reader.GetGuid(2), reader.GetString(3));
        await reader.DisposeAsync();
        return await LoadClaimsAsync(connection, values.Item1, values.Item2, values.Item3, values.Item4, ct);
    }

    private static async Task<AuthUser> LoadClaimsAsync(NpgsqlConnection connection, Guid id, Guid tenant, Guid branch, string email, CancellationToken ct)
    {
        const string sql = """
            SELECT DISTINCT r.code, p.code FROM barber.user_roles ur
            JOIN barber.roles r ON r.id=ur.role_id LEFT JOIN barber.role_permissions rp ON rp.role_id=r.id
            LEFT JOIN barber.permissions p ON p.id=rp.permission_id WHERE ur.user_id=@id
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) { roles.Add(reader.GetString(0)); if (!reader.IsDBNull(1)) permissions.Add(reader.GetString(1)); }
        return new AuthUser(id, tenant, branch, email, roles.ToArray(), permissions.ToArray());
    }

    private async Task StoreRefreshAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId, string token, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand("UPDATE barber.users SET refresh_token_hash=@hash,refresh_token_expires_at=@expires,updated_at=now() WHERE id=@id", connection, transaction);
        command.Parameters.AddWithValue("hash", Hash(token)); command.Parameters.AddWithValue("expires", DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays)); command.Parameters.AddWithValue("id", userId);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task AuditAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string action, string email, string? ip, string correlation, CancellationToken ct, AuthUser? user = null)
    {
        const string sql = "INSERT INTO barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,correlation_id,module,action,description,metadata) VALUES(@id,@tenant,@branch,@user,@action,'users',@user,@correlation,'Auth',@action,@description,jsonb_build_object('ip_address',@ip))";
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.Add("tenant", NpgsqlTypes.NpgsqlDbType.Uuid).Value = (object?)user?.TenantId ?? DBNull.Value;
        command.Parameters.Add("branch", NpgsqlTypes.NpgsqlDbType.Uuid).Value = (object?)user?.BranchId ?? DBNull.Value;
        command.Parameters.Add("user", NpgsqlTypes.NpgsqlDbType.Uuid).Value = (object?)user?.Id ?? DBNull.Value;
        command.Parameters.AddWithValue("action", action); command.Parameters.AddWithValue("correlation", correlation); command.Parameters.AddWithValue("description", $"{action}: {email}"); command.Parameters.AddWithValue("ip", ip ?? "unknown");
        await command.ExecuteNonQueryAsync(ct);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private sealed record UserRecord(AuthUser AuthUser, string PasswordHash);
}
