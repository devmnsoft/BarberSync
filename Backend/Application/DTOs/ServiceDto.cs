namespace BarberSync.Application.DTOs;

public record ServiceDto(Guid Id, string Name, bool IsActive);
public record CreateServiceDto(string Name);
public record UpdateServiceDto(string Name, bool IsActive);
