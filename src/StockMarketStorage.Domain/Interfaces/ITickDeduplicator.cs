using StockMarketStorage.Domain.Entities;

namespace StockMarketStorage.Domain.Interfaces;

/// <summary>
/// Дедупликатор тиков по ticker + price + volume + timestamp.
/// </summary>
public interface ITickDeduplicator
{
    bool IsDuplicate(Tick tick);
    void MarkSeen(Tick tick);
}
