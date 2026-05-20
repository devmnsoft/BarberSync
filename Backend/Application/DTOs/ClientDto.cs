namespace BarberSync.Application.DTOs;

public record ClientDto(Guid Id, string Name, bool IsActive);
public record CreateClientDto(string Name);
public record UpdateClientDto(string Name, bool IsActive);
