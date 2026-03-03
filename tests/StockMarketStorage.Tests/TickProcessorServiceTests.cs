using Microsoft.Extensions.Logging;
using Moq;
using StockMarketStorage.Application.Services;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;
using Xunit;

namespace StockMarketStorage.Tests;

public sealed class TickProcessorServiceTests
{
    private readonly Mock<ITickNormalizer> _normalizerMock = new();
    private readonly Mock<ITickDeduplicator> _deduplicatorMock = new();
    private readonly Mock<ITickBuffer> _bufferMock = new();
    private readonly Mock<ILogger<TickProcessorService>> _loggerMock = new();

    [Fact]
    public void ProcessRawTick_NormalizedTick_NotDuplicate_AddsToBuffer()
    {
        var tick = new Tick { Ticker = "AAPL", Price = 150m, Volume = 100m, Timestamp = DateTime.UtcNow, Source = "Test" };
        _normalizerMock.Setup(n => n.TryNormalize("Source", It.IsAny<object>())).Returns(tick);
        _deduplicatorMock.Setup(d => d.IsDuplicate(tick)).Returns(false);

        var processor = new TickProcessorService(
            _normalizerMock.Object,
            _deduplicatorMock.Object,
            _bufferMock.Object,
            _loggerMock.Object);

        processor.ProcessRawTick("Source", new object());

        _bufferMock.Verify(b => b.Add(tick), Times.Once);
        _deduplicatorMock.Verify(d => d.MarkSeen(tick), Times.Once);
        Assert.Equal(1, processor.ProcessedCount);
    }

    [Fact]
    public void ProcessRawTick_Duplicate_DoesNotAddToBuffer()
    {
        var tick = new Tick { Ticker = "AAPL", Price = 150m, Volume = 100m, Timestamp = DateTime.UtcNow, Source = "Test" };
        _normalizerMock.Setup(n => n.TryNormalize("Source", It.IsAny<object>())).Returns(tick);
        _deduplicatorMock.Setup(d => d.IsDuplicate(tick)).Returns(true);

        var processor = new TickProcessorService(
            _normalizerMock.Object,
            _deduplicatorMock.Object,
            _bufferMock.Object,
            _loggerMock.Object);

        processor.ProcessRawTick("Source", new object());

        _bufferMock.Verify(b => b.Add(It.IsAny<Tick>()), Times.Never);
        _deduplicatorMock.Verify(d => d.MarkSeen(It.IsAny<Tick>()), Times.Never);
        Assert.Equal(1, processor.ProcessedCount);
    }

    [Fact]
    public void ProcessRawTick_NormalizeReturnsNull_DoesNotAddToBuffer()
    {
        _normalizerMock.Setup(n => n.TryNormalize("Source", It.IsAny<object>())).Returns((Tick?)null);

        var processor = new TickProcessorService(
            _normalizerMock.Object,
            _deduplicatorMock.Object,
            _bufferMock.Object,
            _loggerMock.Object);

        processor.ProcessRawTick("Source", new object());

        _bufferMock.Verify(b => b.Add(It.IsAny<Tick>()), Times.Never);
        Assert.Equal(0, processor.ProcessedCount);
    }
}
