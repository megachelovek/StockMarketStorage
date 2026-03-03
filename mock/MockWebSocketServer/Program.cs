using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

var tickers = new[] { "AAPL", "GOOGL", "MSFT", "AMZN", "TSLA" };
var random = new Random();

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var ws = await context.WebSockets.AcceptWebSocketAsync();
    var port = context.Connection.LocalPort;
    var sourceName = $"MockSource{port % 3 + 1}";

    Console.WriteLine($"[{sourceName}] Client connected on port {port}");

    while (ws.State == WebSocketState.Open)
    {
        var tick = new
        {
            ticker = tickers[random.Next(tickers.Length)],
            price = Math.Round(100 + (decimal)(random.NextDouble() * 500), 2),
            volume = Math.Round((decimal)(random.NextDouble() * 1000), 2),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var json = JsonSerializer.Serialize(tick);
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

        await Task.Delay(random.Next(20, 100));
    }

    Console.WriteLine($"[{sourceName}] Client disconnected");
});

app.MapGet("/", () => "Mock WebSocket tick server. Connect to /ws");

app.Run();
