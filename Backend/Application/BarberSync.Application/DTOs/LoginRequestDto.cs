namespace BarberSync.Application.DTOs;

public sealed record LoginRequestDto(
    string? Email,
    string Password,
    string? TenantSlug = null,
    string? TenantIdentifier = null,
    string? UserIdentifier = null)
{
    public string EffectiveUserIdentifier => (UserIdentifier ?? Email ?? string.Empty).Trim();
    public string? EffectiveTenantIdentifier => string.IsNullOrWhiteSpace(TenantIdentifier) ? TenantSlug?.Trim() : TenantIdentifier.Trim();
}
public sealed record RefreshTokenRequestDto(string RefreshToken);
public sealed record LogoutRequestDto(string RefreshToken);
public sealed record LoginResponseDto(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, string TokenType = "Bearer");
