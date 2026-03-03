using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMarketStorage.Application.Services;

namespace StockMarketStorage.Host.Workers;

public sealed class TickFlushWorker : BackgroundService
{
    private readonly ITickFlushService _flushService;
    private readonly ILogger<TickFlushWorker> _logger;
    private readonly IOptions<AppHostOptions> _options;

    public TickFlushWorker(
        ITickFlushService flushService,
        ILogger<TickFlushWorker> logger,
        IOptions<AppHostOptions> options)
    {
        _flushService = flushService;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.FlushIntervalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);
            await _flushService.FlushAsync(stoppingToken);
        }
    }
}
