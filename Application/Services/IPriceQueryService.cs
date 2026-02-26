using Caching.Etag.Api.Infrastructure.Stores;

namespace Caching.Etag.Api.Application.Services;

public interface IPriceQueryService
{
    PriceItem? GetPrice(string symbol);
    IReadOnlyList<PriceItem> GetPrices(IEnumerable<string> symbols);
    IReadOnlyList<PriceItem> GetAllPrices();
    IReadOnlyList<PriceItem> GetStalePrices(TimeSpan staleAfter);
    PriceItem UpsertPrice(string symbol, decimal price);
    string BuildEtag(PriceItem item);
    string BuildCollectionEtag(IEnumerable<PriceItem> items);
    void Invalidate(string symbol);
}
