using StockMarketStorage.Domain.Entities;

namespace StockMarketStorage.Domain.Interfaces;

/// <summary>
/// Нормализатор сырых тиков к единому формату.
/// </summary>
public interface ITickNormalizer
{
    /// <summary>
    /// Пытается нормализовать сырой тик. Возвращает null при ошибке парсинга.
    /// </summary>
    Tick? TryNormalize(string sourceName, object rawTick);
}
