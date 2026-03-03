using Microsoft.Extensions.Logging;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Application.Services;

/// <summary>
/// Обрабатывает поток тиков: нормализация, дедупликация, буферизация.
/// </summary>
public interface ITickProcessorService
{
    void ProcessRawTick(string sourceName, object rawTick);
    long ProcessedCount { get; }
}

public sealed class TickProcessorService : ITickProcessorService
{
    private long _processedCount;
    private readonly ITickNormalizer _normalizer;
    private readonly ITickDeduplicator _deduplicator;
    private readonly ITickBuffer _buffer;
    private readonly ILogger<TickProcessorService> _logger;

    public TickProcessorService(
        ITickNormalizer normalizer,
        ITickDeduplicator deduplicator,
        ITickBuffer buffer,
        ILogger<TickProcessorService> logger)
    {
        _normalizer = normalizer;
        _deduplicator = deduplicator;
        _buffer = buffer;
        _logger = logger;
    }

    public long ProcessedCount => Interlocked.Read(ref _processedCount);

    public void ProcessRawTick(string sourceName, object rawTick)
    {
        var tick = _normalizer.TryNormalize(sourceName, rawTick);
        if (tick is null)
        {
            _logger.LogDebug("Failed to normalize tick from {Source}", sourceName);
            return;
        }

        Interlocked.Increment(ref _processedCount);

        if (_deduplicator.IsDuplicate(tick))
            return;

        _deduplicator.MarkSeen(tick);
        _buffer.Add(tick);
    }
}
