using System.Collections.Concurrent;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Application.Services;

/// <summary>
/// In-memory буфер для батчевой записи тиков.
/// </summary>
public sealed class TickBuffer : ITickBuffer
{
    private readonly ConcurrentQueue<Tick> _queue = new();

    public void Add(Tick tick) => _queue.Enqueue(tick);

    public int Count => _queue.Count;

    public IReadOnlyList<Tick> Drain()
    {
        var list = new List<Tick>();
        while (_queue.TryDequeue(out var tick))
            list.Add(tick);
        return list;
    }
}
