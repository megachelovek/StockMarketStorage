using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockMarketStorage.Application.Services;
using StockMarketStorage.Domain.Interfaces;
using StockMarketStorage.Host;
using StockMarketStorage.Infrastructure.Persistence;
using StockMarketStorage.Host.Workers;
using StockMarketStorage.Infrastructure.WebSocket;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AppHostOptions>(
    builder.Configuration.GetSection(AppHostOptions.SectionName));

builder.Services.Configure<WebSocketSourcesConfig>(
    builder.Configuration.GetSection(WebSocketSourcesConfig.SectionName));

builder.Services.AddDbContextFactory<TicksDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=stockmarket;Username=postgres;Password=postgres";
    options.UseNpgsql(conn);
});

builder.Services.AddSingleton<ITickRepository, TickRepository>();
builder.Services.AddSingleton<ITickBuffer, TickBuffer>();
builder.Services.AddSingleton<ITickDeduplicator, TickDeduplicator>();
builder.Services.AddSingleton<ITickNormalizer, TickNormalizer>();
builder.Services.AddSingleton<ITickProcessorService, TickProcessorService>();
builder.Services.AddSingleton<ITickFlushService, TickFlushService>();
builder.Services.AddSingleton<ITickSourceFactory, WebSocketTickSourceFactory>();

builder.Services.AddHostedService<TickCollectorWorker>();
builder.Services.AddHostedService<TickFlushWorker>();
builder.Services.AddHostedService<TickCounterWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TicksDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

await host.RunAsync();
