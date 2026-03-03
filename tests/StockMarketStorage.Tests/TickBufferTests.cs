using StockMarketStorage.Application.Services;
using StockMarketStorage.Domain.Entities;
using Xunit;

namespace StockMarketStorage.Tests;

public sealed class TickBufferTests
{
    [Fact]
    public void Add_AndDrain_ReturnsAllTicks()
    {
        var buffer = new TickBuffer();
        var tick = new Tick { Ticker = "AAPL", Price = 150m, Volume = 100m, Timestamp = DateTime.UtcNow, Source = "Test" };

        buffer.Add(tick);
        buffer.Add(tick);

        Assert.Equal(2, buffer.Count);

        var drained = buffer.Drain();
        Assert.Equal(2, drained.Count);
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Drain_EmptyBuffer_ReturnsEmpty()
    {
        var buffer = new TickBuffer();
        var drained = buffer.Drain();
        Assert.Empty(drained);
    }
}
