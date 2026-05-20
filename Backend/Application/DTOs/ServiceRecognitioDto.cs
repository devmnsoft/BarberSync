namespace BarberSync.Application.DTOs;

public record ServiceRecognitioDto(Guid Id, string Name, bool IsActive);
public record CreateServiceRecognitioDto(string Name);
public record UpdateServiceRecognitioDto(string Name, bool IsActive);
