using Caching.Etag.Api.Store;

namespace Caching.Etag.Api.Services;

public interface IPriceQueryService
{
    PriceItem? GetPrice(string symbol);
    IReadOnlyList<PriceItem> GetAllPrices();
    PriceItem UpsertPrice(string symbol, decimal price);
    string BuildEtag(PriceItem item);
    void Invalidate(string symbol);
}
