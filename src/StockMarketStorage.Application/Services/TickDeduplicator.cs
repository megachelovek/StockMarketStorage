using System.Collections.Concurrent;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Application.Services;

/// <summary>
/// Дедупликатор по ticker + price + volume + timestamp.
/// Использует in-memory HashSet с ограничением размера для предотвращения утечки памяти.
/// </summary>
public sealed class TickDeduplicator : ITickDeduplicator
{
    private const int MaxCacheSize = 100_000;
    private readonly ConcurrentDictionary<int, byte> _seen = new();
    private readonly Queue<int> _order = new();
    private readonly object _lock = new();

    public bool IsDuplicate(Tick tick)
    {
        var hash = tick.GetHashCode();
        return _seen.ContainsKey(hash);
    }

    public void MarkSeen(Tick tick)
    {
        var hash = tick.GetHashCode();
        if (_seen.TryAdd(hash, 0))
        {
            lock (_lock)
            {
                _order.Enqueue(hash);
                while (_order.Count > MaxCacheSize && _order.TryDequeue(out var old))
                    _seen.TryRemove(old, out _);
            }
        }
    }
}
