namespace StockMarketStorage.Host;

public sealed class AppHostOptions
{
    public const string SectionName = "Host";
    public int FlushIntervalSeconds { get; set; } = 5;
    public int TickCounterIntervalSeconds { get; set; } = 10;
}
