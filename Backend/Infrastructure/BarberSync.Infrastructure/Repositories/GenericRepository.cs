using BarberSync.Application.Abstractions;

namespace BarberSync.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items = [];

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<T?>(default);
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<T>>(_items.ToList());
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _items.Add(entity);
        return Task.CompletedTask;
    }
}
