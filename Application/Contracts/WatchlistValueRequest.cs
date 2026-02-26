namespace Caching.Etag.Api.Application.Contracts;

public sealed class WatchlistValueRequest
{
    public IReadOnlyList<WatchlistHoldingRequest> Holdings { get; set; } = [];
}

public sealed class WatchlistHoldingRequest
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
