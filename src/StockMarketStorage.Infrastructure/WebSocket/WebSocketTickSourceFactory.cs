using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Infrastructure.WebSocket;

public sealed class WebSocketSourcesConfig
{
    public const string SectionName = "WebSocketSources";
    public List<WebSocketTickSourceOptions> Sources { get; set; } = [];
}

public sealed class WebSocketTickSourceFactory : ITickSourceFactory
{
    private readonly IServiceProvider _services;

    public WebSocketTickSourceFactory(IServiceProvider services) => _services = services;

    public IReadOnlyList<ITickSource> CreateAll()
    {
        var options = _services.GetRequiredService<IOptions<WebSocketSourcesConfig>>().Value;
        var list = new List<ITickSource>();
        foreach (var opt in options.Sources)
        {
            var logger = _services.GetRequiredService<ILogger<WebSocketTickSource>>();
            var source = new WebSocketTickSource(Options.Create(opt), logger);
            list.Add(source);
        }
        return list;
    }
}
