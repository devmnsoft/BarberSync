using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IClientsService
{
    Task<(IEnumerable<ClientDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<ClientDto?> GetByIdAsync(Guid id);
    Task<ClientDto> CreateAsync(CreateClientDto dto);
    Task<ClientDto?> UpdateAsync(Guid id, UpdateClientDto dto);
    Task<bool> DeleteAsync(Guid id);
}
