using System.Text.Json;
using StockMarketStorage.Application.Services;
using Xunit;

namespace StockMarketStorage.Tests;

public sealed class TickNormalizerTests
{
    private readonly TickNormalizer _normalizer = new();

    [Fact]
    public void TryNormalize_ValidJson_ReturnsTick()
    {
        var raw = JsonSerializer.SerializeToElement(new
        {
            ticker = "AAPL",
            price = 150.5m,
            volume = 100m,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        var result = _normalizer.TryNormalize("TestSource", raw);

        Assert.NotNull(result);
        Assert.Equal("AAPL", result.Ticker);
        Assert.Equal(150.5m, result.Price);
        Assert.Equal(100m, result.Volume);
        Assert.Equal("TestSource", result.Source);
    }

    [Fact]
    public void TryNormalize_AlternateKeys_ReturnsTick()
    {
        var raw = JsonSerializer.SerializeToElement(new
        {
            symbol = "GOOGL",
            p = 2800.25m,
            vol = 50m,
            ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        var result = _normalizer.TryNormalize("MyCustomSource", raw);

        Assert.NotNull(result);
        Assert.Equal("GOOGL", result.Ticker);
        Assert.Equal(2800.25m, result.Price);
        Assert.Equal(50m, result.Volume);
        Assert.Equal("MyCustomSource", result.Source);
    }

    [Fact]
    public void TryNormalize_InvalidJson_ReturnsNull()
    {
        var raw = JsonSerializer.SerializeToElement(new { foo = "bar" });

        var result = _normalizer.TryNormalize("Test", raw);

        Assert.Null(result);
    }

    [Fact]
    public void TryNormalize_StringInput_ReturnsTick()
    {
        var json = """{"ticker":"MSFT","price":400,"volume":200,"timestamp":1700000000000}""";

        var result = _normalizer.TryNormalize("Source", json);

        Assert.NotNull(result);
        Assert.Equal("MSFT", result.Ticker);
        Assert.Equal(400m, result.Price);
    }
}
