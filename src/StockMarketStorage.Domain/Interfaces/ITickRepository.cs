using StockMarketStorage.Domain.Entities;

namespace StockMarketStorage.Domain.Interfaces;

/// <summary>
/// Репозиторий для сохранения тиков в БД.
/// </summary>
public interface ITickRepository
{
    Task SaveTicksAsync(IReadOnlyList<Tick> ticks, CancellationToken cancellationToken = default);
}
