using BarberSync.Application.Abstractions;

namespace BarberSync.Application.Services;

public class GenericCrudService<TDto> : ICrudService<TDto>
{
    public Task<TDto> CreateAsync(TDto dto) => Task.FromResult(dto);
    public Task<bool> DeleteAsync(Guid id) => Task.FromResult(true);
    public Task<IEnumerable<TDto>> GetAllAsync(int page, int pageSize, string? sortBy, string? sortOrder, string? search) => Task.FromResult<IEnumerable<TDto>>(Array.Empty<TDto>());
    public Task<TDto?> GetByIdAsync(Guid id) => Task.FromResult<TDto?>(default);
    public Task<TDto?> UpdateAsync(Guid id, TDto dto) => Task.FromResult<TDto?>(dto);
}
