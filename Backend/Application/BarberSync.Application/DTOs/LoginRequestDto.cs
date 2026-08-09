namespace BarberSync.Application.DTOs;

public sealed record LoginRequestDto(string Email, string Password, string? TenantSlug = null);
public sealed record RefreshTokenRequestDto(string RefreshToken);
public sealed record LogoutRequestDto(string RefreshToken);
public sealed record LoginResponseDto(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, string TokenType = "Bearer");
