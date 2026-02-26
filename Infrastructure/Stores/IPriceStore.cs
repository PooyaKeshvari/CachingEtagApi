namespace Caching.Etag.Api.Infrastructure.Stores;

public interface IPriceStore
{
    PriceItem? Get(string symbol);
    IReadOnlyList<PriceItem> GetAll();
    PriceItem Upsert(string symbol, decimal price);
}
