namespace StockMarketStorage.Domain.Entities;

/// <summary>
/// Нормализованный тик котировки.
/// </summary>
public sealed class Tick
{
    public required string Ticker { get; init; }
    public required decimal Price { get; init; }
    public required decimal Volume { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Source { get; init; }

    public override bool Equals(object? obj) => obj is Tick other &&
        Ticker == other.Ticker &&
        Price == other.Price &&
        Volume == other.Volume &&
        Timestamp == other.Timestamp;

    public override int GetHashCode() =>
        HashCode.Combine(Ticker, Price, Volume, Timestamp);
}
