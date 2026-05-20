using BarberSync.Domain.Interfaces;
using System.Reflection;

namespace BarberSync.Infrastructure.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : class, new()
{
    private readonly List<T> _items = new();

    public Task<T> AddAsync(T entity) { _items.Add(entity); return Task.FromResult(entity); }
    public Task<bool> DeleteAsync(Guid id)
    {
        var item = _items.FirstOrDefault(x => (Guid)(x!.GetType().GetProperty("Id")!.GetValue(x)!) == id);
        if (item is null) return Task.FromResult(false);
        _items.Remove(item);
        return Task.FromResult(true);
    }
    public Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir)
    {
        IEnumerable<T> q = _items;
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => (x!.GetType().GetProperty("Name")?.GetValue(x)?.ToString() ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));
        var total = q.Count();
        q = q.Skip((page - 1) * pageSize).Take(pageSize);
        return Task.FromResult((q, total));
    }
    public Task<T?> GetByIdAsync(Guid id) => Task.FromResult(_items.FirstOrDefault(x => (Guid)(x!.GetType().GetProperty("Id")!.GetValue(x)!) == id));
    public Task<T?> UpdateAsync(Guid id, T entity)
    {
        var idx = _items.FindIndex(x => (Guid)(x!.GetType().GetProperty("Id")!.GetValue(x)!) == id);
        if (idx < 0) return Task.FromResult<T?>(null);
        _items[idx] = entity;
        return Task.FromResult<T?>(entity);
    }
}
