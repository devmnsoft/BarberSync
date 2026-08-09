namespace BarberSync.Application.Abstractions;

public interface ITokenService
{
    string Generate(AuthUser user);
}

public sealed record AuthUser(Guid Id, Guid TenantId, Guid BranchId, string Email, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
