using System.Collections.Concurrent;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;

namespace BarberSync.Infrastructure.Saas;

public sealed class LocalEffectiveAccessCache : IEffectiveAccessCache
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);

    public bool TryGet(string key, out ModuleAccessDecision decision)
    {
        if (_entries.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTimeOffset.UtcNow)
        {
            decision = entry.Decision;
            return true;
        }

        _entries.TryRemove(key, out _);
        decision = default!;
        return false;
    }

    public void Set(string key, ModuleAccessDecision decision, TimeSpan lifetime) =>
        _entries[key] = new(decision, DateTimeOffset.UtcNow.Add(lifetime));

    public void Invalidate(Guid tenantId, Guid? userId, Guid? branchId)
    {
        var tenantMarker = $"|{tenantId:N}|";
        var userMarker = userId.HasValue ? $"|{userId.Value:N}|" : null;
        var branchMarker = branchId.HasValue ? $"|{branchId.Value:N}|" : null;
        foreach (var key in _entries.Keys.Where(key => key.Contains(tenantMarker, StringComparison.Ordinal) &&
                     (userMarker is null || key.Contains(userMarker, StringComparison.Ordinal)) &&
                     (branchMarker is null || key.Contains(branchMarker, StringComparison.Ordinal))))
            _entries.TryRemove(key, out _);
    }

    private sealed record Entry(ModuleAccessDecision Decision, DateTimeOffset ExpiresAt);
}
