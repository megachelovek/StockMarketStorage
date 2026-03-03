using StockMarketStorage.Domain.Entities;

namespace StockMarketStorage.Domain.Interfaces;

/// <summary>
/// In-memory буфер для батчевой записи тиков в БД.
/// </summary>
public interface ITickBuffer
{
    void Add(Tick tick);
    int Count { get; }
    /// <summary>
    /// Извлекает и возвращает все накопленные тики.
    /// </summary>
    IReadOnlyList<Tick> Drain();
}
