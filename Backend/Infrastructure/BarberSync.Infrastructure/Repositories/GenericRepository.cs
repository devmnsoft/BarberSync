using BarberSync.Application.Abstractions;

namespace BarberSync.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items = [];
    public Task AddAsync(T entity) { _items.Add(entity); return Task.CompletedTask; }
    public Task DeleteAsync(Guid id) => Task.CompletedTask;
    public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(_items);
    public Task<T?> GetByIdAsync(Guid id) => Task.FromResult<T?>(default);
    public Task UpdateAsync(T entity) => Task.CompletedTask;
}
