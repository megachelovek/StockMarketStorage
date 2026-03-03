using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMarketStorage.Application.Services;

namespace StockMarketStorage.Host.Workers;

public sealed class TickCounterWorker : BackgroundService
{
    private readonly ITickProcessorService _processor;
    private readonly ILogger<TickCounterWorker> _logger;
    private readonly IOptions<AppHostOptions> _options;

    public TickCounterWorker(
        ITickProcessorService processor,
        ILogger<TickCounterWorker> logger,
        IOptions<AppHostOptions> options)
    {
        _processor = processor;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.TickCounterIntervalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);
            var count = _processor.ProcessedCount;
            _logger.LogInformation("Processed ticks: {Count}", count);
        }
    }
}
