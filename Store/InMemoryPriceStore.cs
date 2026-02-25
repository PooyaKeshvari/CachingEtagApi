namespace Caching.Etag.Api.Store;

public sealed class InMemoryPriceStore : IPriceStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, PriceItem> _prices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BTC"] = new PriceItem
        {
            Symbol = "BTC",
            Price = 75250.10m,
            Version = 1,
            LastUpdatedUtc = DateTimeOffset.UtcNow
        },
        ["ETH"] = new PriceItem
        {
            Symbol = "ETH",
            Price = 4100.40m,
            Version = 1,
            LastUpdatedUtc = DateTimeOffset.UtcNow
        }
    };

    public PriceItem? Get(string symbol)
    {
        lock (_sync)
        {
            if (!_prices.TryGetValue(symbol, out var item))
            {
                return null;
            }

            return Copy(item);
        }
    }

    public IReadOnlyList<PriceItem> GetAll()
    {
        lock (_sync)
        {
            return _prices.Values
                .Select(Copy)
                .OrderBy(p => p.Symbol)
                .ToList();
        }
    }

    public PriceItem Upsert(string symbol, decimal price)
    {
        lock (_sync)
        {
            if (_prices.TryGetValue(symbol, out var current))
            {
                current.Price = price;
                current.Version += 1;
                current.LastUpdatedUtc = DateTimeOffset.UtcNow;
                return Copy(current);
            }

            var created = new PriceItem
            {
                Symbol = symbol,
                Price = price,
                Version = 1,
                LastUpdatedUtc = DateTimeOffset.UtcNow
            };

            _prices[symbol] = created;
            return Copy(created);
        }
    }

    private static PriceItem Copy(PriceItem source)
    {
        return new PriceItem
        {
            Symbol = source.Symbol,
            Price = source.Price,
            Version = source.Version,
            LastUpdatedUtc = source.LastUpdatedUtc
        };
    }
}
