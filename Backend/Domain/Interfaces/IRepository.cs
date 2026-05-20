namespace BarberSync.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<T?> UpdateAsync(Guid id, T entity);
    Task<bool> DeleteAsync(Guid id);
}
