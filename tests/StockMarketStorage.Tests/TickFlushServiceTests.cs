using Microsoft.Extensions.Logging;
using Moq;
using StockMarketStorage.Application.Services;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;
using Xunit;

namespace StockMarketStorage.Tests;

public sealed class TickFlushServiceTests
{
    [Fact]
    public async Task FlushAsync_EmptyBuffer_DoesNotCallRepository()
    {
        var buffer = new TickBuffer();
        var repoMock = new Mock<ITickRepository>();
        var loggerMock = new Mock<ILogger<TickFlushService>>();

        var service = new TickFlushService(buffer, repoMock.Object, loggerMock.Object);

        await service.FlushAsync();

        repoMock.Verify(r => r.SaveTicksAsync(It.IsAny<IReadOnlyList<Tick>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FlushAsync_WithTicks_CallsRepository()
    {
        var buffer = new TickBuffer();
        var tick = new Tick { Ticker = "AAPL", Price = 150m, Volume = 100m, Timestamp = DateTime.UtcNow, Source = "Test" };
        buffer.Add(tick);

        var repoMock = new Mock<ITickRepository>();
        var loggerMock = new Mock<ILogger<TickFlushService>>();

        var service = new TickFlushService(buffer, repoMock.Object, loggerMock.Object);

        await service.FlushAsync();

        repoMock.Verify(r => r.SaveTicksAsync(It.Is<IReadOnlyList<Tick>>(l => l.Count == 1 && l[0].Ticker == "AAPL"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FlushAsync_RepositoryThrows_ReAddsTicksToBuffer()
    {
        var buffer = new TickBuffer();
        var tick = new Tick { Ticker = "AAPL", Price = 150m, Volume = 100m, Timestamp = DateTime.UtcNow, Source = "Test" };
        buffer.Add(tick);

        var repoMock = new Mock<ITickRepository>();
        repoMock.Setup(r => r.SaveTicksAsync(It.IsAny<IReadOnlyList<Tick>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        var loggerMock = new Mock<ILogger<TickFlushService>>();

        var service = new TickFlushService(buffer, repoMock.Object, loggerMock.Object);

        await service.FlushAsync();

        Assert.Equal(1, buffer.Count);
    }
}
