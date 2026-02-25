namespace Caching.Etag.Api.Store;

public sealed class PriceItem
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public long Version { get; set; }
    public DateTimeOffset LastUpdatedUtc { get; set; }
}
