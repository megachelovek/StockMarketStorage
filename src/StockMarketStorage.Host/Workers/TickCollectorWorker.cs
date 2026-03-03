using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMarketStorage.Application.Services;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Host.Workers;

public sealed class TickCollectorWorker : BackgroundService
{
    private readonly ITickSourceFactory _sourceFactory;
    private readonly ITickProcessorService _processor;
    private readonly ILogger<TickCollectorWorker> _logger;
    private IReadOnlyList<ITickSource> _sources = [];

    public TickCollectorWorker(
        ITickSourceFactory sourceFactory,
        ITickProcessorService processor,
        ILogger<TickCollectorWorker> logger)
    {
        _sourceFactory = sourceFactory;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _sources = _sourceFactory.CreateAll();
        if (_sources.Count == 0)
        {
            _logger.LogWarning("No WebSocket sources configured");
            return;
        }

        foreach (var source in _sources)
        {
            source.TickReceived += OnTickReceived;
            source.Connected += OnConnected;
            source.Disconnected += OnDisconnected;
            source.Error += OnError;
        }

        var connectTasks = _sources.Select(s => ConnectWithRetryAsync(s, stoppingToken)).ToArray();
        await Task.WhenAll(connectTasks);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void OnTickReceived(object? sender, TickReceivedEventArgs e)
    {
        _processor.ProcessRawTick(e.SourceName, e.RawTick);
    }

    private void OnConnected(object? sender, ConnectionEventArgs e) =>
        _logger.LogInformation("Source connected: {Source}", e.SourceName);

    private void OnDisconnected(object? sender, ConnectionEventArgs e) =>
        _logger.LogInformation("Source disconnected: {Source}", e.SourceName);

    private void OnError(object? sender, TickSourceErrorEventArgs e) =>
        _logger.LogError(e.Exception, "Source error: {Source}", e.SourceName);

    private async Task ConnectWithRetryAsync(ITickSource source, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await source.ConnectAsync(ct);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect {Source}, retrying...", source.SourceName);
                await Task.Delay(3000, ct);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var source in _sources)
        {
            source.TickReceived -= OnTickReceived;
            source.Connected -= OnConnected;
            source.Disconnected -= OnDisconnected;
            source.Error -= OnError;
            await source.DisconnectAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}
