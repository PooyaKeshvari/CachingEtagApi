using Caching.Etag.Api.Infrastructure.Stores;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace Caching.Etag.Api.Application.Services;

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

    public IReadOnlyList<PriceItem> GetPrices(IEnumerable<string> symbols)
    {
        var normalizedSymbols = symbols
            .Where(static symbol => !string.IsNullOrWhiteSpace(symbol))
            .Select(Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedSymbols.Count == 0)
        {
            return [];
        }

        var prices = new List<PriceItem>(normalizedSymbols.Count);
        foreach (var symbol in normalizedSymbols)
        {
            var item = GetPrice(symbol);
            if (item is not null)
            {
                prices.Add(item);
            }
        }

        return prices
            .OrderBy(item => item.Symbol)
            .ToList();
    }

    public IReadOnlyList<PriceItem> GetAllPrices()
    {
        return _store.GetAll();
    }

    public IReadOnlyList<PriceItem> GetStalePrices(TimeSpan staleAfter)
    {
        if (staleAfter <= TimeSpan.Zero)
        {
            return [];
        }

        var thresholdUtc = DateTimeOffset.UtcNow.Subtract(staleAfter);
        return _store.GetAll()
            .Where(item => item.LastUpdatedUtc <= thresholdUtc)
            .OrderBy(item => item.Symbol)
            .ToList();
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

    public string BuildCollectionEtag(IEnumerable<PriceItem> items)
    {
        var signature = string.Join(
            '|',
            items
                .OrderBy(item => item.Symbol)
                .Select(item => $"{item.Symbol}:{item.Version}"));

        if (string.IsNullOrEmpty(signature))
        {
            return "\"empty\"";
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(signature));
        return $"\"{Convert.ToHexString(hashBytes).ToLowerInvariant()}\"";
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
