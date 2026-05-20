namespace BarberSync.Application.Abstractions;

public interface ICrudService<TDto>
{
    Task<IEnumerable<TDto>> GetAllAsync(int page, int pageSize, string? sortBy, string? sortOrder, string? search);
    Task<TDto?> GetByIdAsync(Guid id);
    Task<TDto> CreateAsync(TDto dto);
    Task<TDto?> UpdateAsync(Guid id, TDto dto);
    Task<bool> DeleteAsync(Guid id);
}
