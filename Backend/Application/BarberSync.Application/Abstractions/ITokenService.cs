namespace BarberSync.Application.Abstractions;

public interface ITokenService
{
    string Generate(AuthUser user);
}

public sealed record AuthUser(
    Guid Id,
    Guid TenantId,
    Guid BranchId,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    bool IsPlatformUser = false,
    Guid? ActorUserId = null,
    Guid? ScopeSessionId = null,
    IReadOnlyList<string>? Modules = null,
    string? TenantName = null,
    string? BranchName = null);
