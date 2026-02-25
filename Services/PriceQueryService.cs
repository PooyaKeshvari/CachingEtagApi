using Caching.Etag.Api.Store;
using Microsoft.Extensions.Caching.Memory;

namespace Caching.Etag.Api.Services;

public sealed class PriceQueryService : IPriceQueryService
{
    private readonly IPriceStore _store;
    private readonly IMemoryCache _cache;

    public PriceQueryService(IPriceStore store, IMemoryCache cache)
    {
        _store = store;
        _cache = cache;
    }

    public PriceItem? GetPrice(string symbol)
    {
        var normalized = Normalize(symbol);
        var cacheKey = GetCacheKey(normalized);

        if (_cache.TryGetValue<PriceItem>(cacheKey, out var cached))
        {
            return cached;
        }

        var item = _store.Get(normalized);
        if (item is null)
        {
            return null;
        }

        _cache.Set(cacheKey, item, TimeSpan.FromSeconds(30));
        return item;
    }

    public IReadOnlyList<PriceItem> GetAllPrices()
    {
        return _store.GetAll();
    }

    public PriceItem UpsertPrice(string symbol, decimal price)
    {
        var normalized = Normalize(symbol);
        var item = _store.Upsert(normalized, price);
        Invalidate(normalized);
        return item;
    }

    public string BuildEtag(PriceItem item)
    {
        return $"\"{item.Symbol}-{item.Version}\"";
    }

    public void Invalidate(string symbol)
    {
        _cache.Remove(GetCacheKey(Normalize(symbol)));
    }

    private static string Normalize(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }

    private static string GetCacheKey(string symbol)
    {
        return $"price:{symbol}";
    }
}
