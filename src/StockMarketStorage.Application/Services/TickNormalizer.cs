using System.Text.Json;
using StockMarketStorage.Domain.Entities;
using StockMarketStorage.Domain.Interfaces;

namespace StockMarketStorage.Application.Services;

/// <summary>
/// Нормализатор, поддерживающий несколько форматов сырых тиков (JSON).
/// </summary>
public sealed class TickNormalizer : ITickNormalizer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Tick? TryNormalize(string sourceName, object rawTick)
    {
        return rawTick switch
        {
            JsonElement je => TryNormalizeFromJson(sourceName, je),
            string s => TryNormalizeFromString(sourceName, s),
            _ => null
        };
    }

    private static Tick? TryNormalizeFromJson(string sourceName, JsonElement je)
    {
        try
        {
            var ticker = GetString(je, "ticker", "symbol", "s");
            var price = GetDecimal(je, "price", "p", "last");
            var volume = GetDecimal(je, "volume", "vol", "v", "qty");
            var timestamp = GetTimestamp(je);

            if (string.IsNullOrEmpty(ticker) || price is null || volume is null || timestamp is null)
                return null;

            return new Tick
            {
                Ticker = ticker,
                Price = price.Value,
                Volume = volume.Value,
                Timestamp = timestamp.Value,
                Source = sourceName
            };
        }
        catch
        {
            return null;
        }
    }

    private static Tick? TryNormalizeFromString(string sourceName, string s)
    {
        try
        {
            var je = JsonSerializer.Deserialize<JsonElement>(s);
            return TryNormalizeFromJson(sourceName, je);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement je, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (je.TryGetProperty(key, out var prop))
                return prop.GetString();
        }
        return null;
    }

    private static decimal? GetDecimal(JsonElement je, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (je.TryGetProperty(key, out var prop))
            {
                if (prop.TryGetDecimal(out var v))
                    return v;
                if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var parsed))
                    return parsed;
            }
        }
        return null;
    }

    private static DateTime? GetTimestamp(JsonElement je)
    {
        var keys = new[] { "timestamp", "ts", "time", "t" };
        foreach (var key in keys)
        {
            if (!je.TryGetProperty(key, out var prop))
                continue;

            if (prop.ValueKind == JsonValueKind.Number)
            {
                if (prop.TryGetInt64(out var unixMs))
                    return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
                if (prop.TryGetDouble(out var unixSec))
                    return DateTimeOffset.FromUnixTimeSeconds((long)unixSec).UtcDateTime;
            }
            if (prop.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.GetString(), out var dt))
                return dt;
        }
        return DateTime.UtcNow;
    }
}
