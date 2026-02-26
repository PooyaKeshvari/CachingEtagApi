using Caching.Etag.Api.Infrastructure.Stores;

namespace Caching.Etag.Api.Application.Contracts;

public sealed class PriceWatchlistResponse
{
    public IReadOnlyList<string> RequestedSymbols { get; init; } = Array.Empty<string>();
    public int MatchedSymbols { get; init; }
    public IReadOnlyList<PriceItem> Prices { get; init; } = Array.Empty<PriceItem>();
}
