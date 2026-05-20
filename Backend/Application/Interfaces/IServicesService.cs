using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IServicesService
{
    Task<(IEnumerable<ServiceDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<ServiceDto?> GetByIdAsync(Guid id);
    Task<ServiceDto> CreateAsync(CreateServiceDto dto);
    Task<ServiceDto?> UpdateAsync(Guid id, UpdateServiceDto dto);
    Task<bool> DeleteAsync(Guid id);
}
