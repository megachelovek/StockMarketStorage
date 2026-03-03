namespace StockMarketStorage.Domain.Interfaces;

/// <summary>
/// Фабрика для создания источников тиков.
/// </summary>
public interface ITickSourceFactory
{
    IReadOnlyList<ITickSource> CreateAll();
}
