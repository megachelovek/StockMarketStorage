using Microsoft.EntityFrameworkCore;

namespace StockMarketStorage.Infrastructure.Persistence;

public sealed class TicksDbContext : DbContext
{
    public TicksDbContext(DbContextOptions<TicksDbContext> options) : base(options) { }

    public DbSet<TickEntity> Ticks => Set<TickEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TickEntity>(e =>
        {
            e.HasIndex(x => new { x.Ticker, x.Timestamp });
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => x.Source);
        });
    }
}
