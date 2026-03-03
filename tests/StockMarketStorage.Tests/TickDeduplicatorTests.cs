using StockMarketStorage.Application.Services;
using StockMarketStorage.Domain.Entities;
using Xunit;

namespace StockMarketStorage.Tests;

public sealed class TickDeduplicatorTests
{
    [Fact]
    public void IsDuplicate_NewTick_ReturnsFalse()
    {
        var dedup = new TickDeduplicator();
        var tick = new Tick
        {
            Ticker = "AAPL",
            Price = 150m,
            Volume = 100m,
            Timestamp = DateTime.UtcNow,
            Source = "Test"
        };

        Assert.False(dedup.IsDuplicate(tick));
    }

    [Fact]
    public void IsDuplicate_AfterMarkSeen_ReturnsTrue()
    {
        var dedup = new TickDeduplicator();
        var tick = new Tick
        {
            Ticker = "AAPL",
            Price = 150m,
            Volume = 100m,
            Timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Source = "Test"
        };

        dedup.MarkSeen(tick);
        Assert.True(dedup.IsDuplicate(tick));
    }

    [Fact]
    public void IsDuplicate_DifferentPrice_ReturnsFalse()
    {
        var dedup = new TickDeduplicator();
        var tick1 = new Tick { Ticker = "AAPL", Price = 150m, Volume = 100m, Timestamp = DateTime.UtcNow, Source = "Test" };
        var tick2 = new Tick { Ticker = "AAPL", Price = 151m, Volume = 100m, Timestamp = tick1.Timestamp, Source = "Test" };

        dedup.MarkSeen(tick1);
        Assert.False(dedup.IsDuplicate(tick2));
    }
}
