namespace BarberSync.Application.DTOs;

public record UserDto(Guid Id, string Name, string Email, bool IsActive);
public record CreateUserDto(string Name);
public record UpdateUserDto(string Name, bool IsActive);
