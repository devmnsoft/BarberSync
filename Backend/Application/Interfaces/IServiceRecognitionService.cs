using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IServiceRecognitionService
{
    Task<(IEnumerable<ServiceRecognitioDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<ServiceRecognitioDto?> GetByIdAsync(Guid id);
    Task<ServiceRecognitioDto> CreateAsync(CreateServiceRecognitioDto dto);
    Task<ServiceRecognitioDto?> UpdateAsync(Guid id, UpdateServiceRecognitioDto dto);
    Task<bool> DeleteAsync(Guid id);
}
