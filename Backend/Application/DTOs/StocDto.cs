namespace BarberSync.Application.DTOs;

public record StocDto(Guid Id, string Name, bool IsActive);
public record CreateStocDto(string Name);
public record UpdateStocDto(string Name, bool IsActive);
