using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IStockService
{
    Task<(IEnumerable<StocDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<StocDto?> GetByIdAsync(Guid id);
    Task<StocDto> CreateAsync(CreateStocDto dto);
    Task<StocDto?> UpdateAsync(Guid id, UpdateStocDto dto);
    Task<bool> DeleteAsync(Guid id);
}
