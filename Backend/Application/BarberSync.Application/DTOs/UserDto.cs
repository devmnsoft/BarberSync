namespace BarberSync.Application.DTOs;

public record UserDto(Guid Id, string Name, string Email, bool Active);
