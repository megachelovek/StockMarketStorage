using Microsoft.Extensions.Logging;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Application.Services;

/// <summary>
/// Периодически сбрасывает буфер тиков в БД.
/// </summary>
public interface ITickFlushService
{
    Task FlushAsync(CancellationToken cancellationToken = default);
}

public sealed class TickFlushService : ITickFlushService
{
    private readonly ITickBuffer _buffer;
    private readonly ITickRepository _repository;
    private readonly ILogger<TickFlushService> _logger;

    public TickFlushService(
        ITickBuffer buffer,
        ITickRepository repository,
        ILogger<TickFlushService> logger)
    {
        _buffer = buffer;
        _repository = repository;
        _logger = logger;
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        var ticks = _buffer.Drain();
        if (ticks.Count == 0)
            return;

        try
        {
            await _repository.SaveTicksAsync(ticks, cancellationToken);
            _logger.LogInformation("Flushed {Count} ticks to database", ticks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush ticks to database");
            foreach (var tick in ticks)
                _buffer.Add(tick);
        }
    }
}
