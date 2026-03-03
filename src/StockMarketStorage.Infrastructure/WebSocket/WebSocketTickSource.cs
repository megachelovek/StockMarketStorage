using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Infrastructure.WebSocket;

public sealed class WebSocketTickSourceOptions
{
    public required string SourceName { get; set; }
    public required string Uri { get; set; }
    public int ReconnectDelayMs { get; set; } = 3000;
}

public sealed class WebSocketTickSource : ITickSource, IAsyncDisposable
{
    private readonly WebSocketTickSourceOptions _options;
    private readonly ILogger<WebSocketTickSource> _logger;
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;
    private bool _disposed;

    public string SourceName => _options.SourceName;
    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public event EventHandler<TickReceivedEventArgs>? TickReceived;
    public event EventHandler<ConnectionEventArgs>? Connected;
    public event EventHandler<ConnectionEventArgs>? Disconnected;
    public event EventHandler<TickSourceErrorEventArgs>? Error;

    public WebSocketTickSource(IOptions<WebSocketTickSourceOptions> options, ILogger<WebSocketTickSource> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_ws?.State == WebSocketState.Open)
            return;

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        try
        {
            await _ws.ConnectAsync(new Uri(_options.Uri), cancellationToken);
            _logger.LogInformation("WebSocket connected: {Source} -> {Uri}", SourceName, _options.Uri);
            Connected?.Invoke(this, new ConnectionEventArgs(SourceName));

            _receiveTask = ReceiveLoopAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebSocket connection failed: {Source}", SourceName);
            Error?.Invoke(this, new TickSourceErrorEventArgs(SourceName, ex));
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_ws is null)
            return;

        _cts?.Cancel();
        try
        {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect: {Source}", SourceName);
        }

        _ws.Dispose();
        _ws = null;
        _logger.LogInformation("WebSocket disconnected: {Source}", SourceName);
        Disconnected?.Invoke(this, new ConnectionEventArgs(SourceName));
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];
        var segment = new ArraySegment<byte>(buffer);

        while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            try
            {
                var result = await _ws.ReceiveAsync(segment, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                if (result.MessageType == WebSocketMessageType.Text && result.Count > 0)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var raw = JsonSerializer.Deserialize<object>(json) ?? json;
                    TickReceived?.Invoke(this, new TickReceivedEventArgs(SourceName, raw));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket receive error: {Source}", SourceName);
                Error?.Invoke(this, new TickSourceErrorEventArgs(SourceName, ex));
                await ReconnectAsync(ct);
            }
        }
    }

    private async Task ReconnectAsync(CancellationToken ct)
    {
        await DisconnectAsync(ct);
        await Task.Delay(_options.ReconnectDelayMs, ct);
        await ConnectAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        await DisconnectAsync();
        _cts?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
