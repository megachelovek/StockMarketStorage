using Microsoft.EntityFrameworkCore;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Infrastructure.Persistence;

public sealed class TickRepository : ITickRepository
{
    private readonly IDbContextFactory<TicksDbContext> _factory;

    public TickRepository(IDbContextFactory<TicksDbContext> factory) => _factory = factory;

    public async Task SaveTicksAsync(IReadOnlyList<Tick> ticks, CancellationToken cancellationToken = default)
    {
        if (ticks.Count == 0)
            return;

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var entities = ticks.Select(t => new TickEntity
        {
            Ticker = t.Ticker,
            Price = t.Price,
            Volume = t.Volume,
            Timestamp = t.Timestamp,
            Source = t.Source
        }).ToList();

        await db.Ticks.AddRangeAsync(entities, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
