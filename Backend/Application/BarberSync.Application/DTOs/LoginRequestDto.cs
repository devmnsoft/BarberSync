namespace BarberSync.Application.DTOs;

public sealed record LoginRequestDto(string Email, string Password);
public sealed record LoginResponseDto(string Token);
