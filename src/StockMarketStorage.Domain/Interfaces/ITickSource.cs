using StockMarketStorage.Domain.Entities;

namespace StockMarketStorage.Domain.Interfaces;

/// <summary>
/// Источник потока тиков (WebSocket и т.п.).
/// </summary>
public interface ITickSource
{
    string SourceName { get; }
    bool IsConnected { get; }

    event EventHandler<TickReceivedEventArgs>? TickReceived;
    event EventHandler<ConnectionEventArgs>? Connected;
    event EventHandler<ConnectionEventArgs>? Disconnected;
    event EventHandler<TickSourceErrorEventArgs>? Error;

    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}

public sealed class TickReceivedEventArgs(string sourceName, object rawTick) : EventArgs
{
    public string SourceName { get; } = sourceName;
    public object RawTick { get; } = rawTick;
}

public sealed class ConnectionEventArgs(string sourceName) : EventArgs
{
    public string SourceName { get; } = sourceName;
}

public sealed class TickSourceErrorEventArgs(string sourceName, Exception exception) : EventArgs
{
    public string SourceName { get; } = sourceName;
    public Exception Exception { get; } = exception;
}
