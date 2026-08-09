using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string? ipAddress, string correlationId, CancellationToken cancellationToken);
    Task<LoginResponseDto?> RefreshAsync(string refreshToken, string? ipAddress, string correlationId, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, string? ipAddress, string correlationId, CancellationToken cancellationToken);
}

public interface ICurrentUserContext
{
    Guid UserId { get; }
    Guid TenantId { get; }
    Guid BranchId { get; }
    IReadOnlySet<string> Roles { get; }
    IReadOnlySet<string> Permissions { get; }
}
