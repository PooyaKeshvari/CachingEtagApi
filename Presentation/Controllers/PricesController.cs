using Caching.Etag.Api.Application.Contracts;
using Caching.Etag.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caching.Etag.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PricesController : ControllerBase
{
    private readonly IPriceQueryService _service;

    public PricesController(IPriceQueryService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAllPrices());
    }

    [HttpGet("watchlist")]
    public IActionResult GetWatchlist([FromQuery] string? symbols)
    {
        if (string.IsNullOrWhiteSpace(symbols))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid watchlist",
                Detail = "Provide comma-separated symbols via query string. Example: ?symbols=BTC,ETH",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var requestedSymbols = symbols
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(symbol => symbol.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var items = _service.GetPrices(requestedSymbols);
        if (items.Count == 0)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Symbols not found",
                Detail = "None of the requested symbols exist in the store.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var etag = _service.BuildCollectionEtag(items);
        var ifNoneMatch = Request.Headers.IfNoneMatch.ToString();
        if (!string.IsNullOrWhiteSpace(ifNoneMatch) && ifNoneMatch == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "private, max-age=20";

        return Ok(new PriceWatchlistResponse
        {
            RequestedSymbols = requestedSymbols,
            MatchedSymbols = items.Count,
            Prices = items
        });
    }

    [HttpGet("{symbol}")]
    public IActionResult GetBySymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid symbol",
                Detail = "A non-empty symbol is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var item = _service.GetPrice(symbol);
        if (item is null)
        {
            return NotFound();
        }

        var etag = _service.BuildEtag(item);
        var ifNoneMatch = Request.Headers.IfNoneMatch.ToString();

        if (!string.IsNullOrWhiteSpace(ifNoneMatch) && ifNoneMatch == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "private, max-age=30";

        return Ok(item);
    }

    [HttpGet("stale")]
    public IActionResult GetStale([FromQuery] int olderThanSeconds = 300)
    {
        if (olderThanSeconds is < 1 or > 86400)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid range",
                Detail = "olderThanSeconds must be between 1 and 86400.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var staleItems = _service.GetStalePrices(TimeSpan.FromSeconds(olderThanSeconds));
        return Ok(new
        {
            OlderThanSeconds = olderThanSeconds,
            Count = staleItems.Count,
            Items = staleItems
        });
    }

    [HttpPost("watchlist/value")]
    public IActionResult CalculateWatchlistValue([FromBody] WatchlistValueRequest request)
    {
        if (request.Holdings is null || request.Holdings.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid holdings",
                Detail = "At least one holding is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var invalidHolding = request.Holdings.FirstOrDefault(holding =>
            string.IsNullOrWhiteSpace(holding.Symbol) || holding.Quantity <= 0);

        if (invalidHolding is not null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid holding",
                Detail = "Each holding requires symbol and quantity greater than zero.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var symbols = request.Holdings
            .Select(holding => holding.Symbol.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var prices = _service.GetPrices(symbols);
        var priceLookup = prices.ToDictionary(
            item => item.Symbol,
            item => item,
            StringComparer.OrdinalIgnoreCase);

        var items = new List<object>(request.Holdings.Count);
        var missingSymbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        decimal totalMarketValue = 0;

        foreach (var holding in request.Holdings)
        {
            var normalizedSymbol = holding.Symbol.Trim().ToUpperInvariant();
            if (!priceLookup.TryGetValue(normalizedSymbol, out var price))
            {
                missingSymbols.Add(normalizedSymbol);
                continue;
            }

            var marketValue = decimal.Round(price.Price * holding.Quantity, 2, MidpointRounding.AwayFromZero);
            totalMarketValue += marketValue;

            items.Add(new
            {
                Symbol = normalizedSymbol,
                Quantity = holding.Quantity,
                UnitPrice = price.Price,
                MarketValue = marketValue
            });
        }

        return Ok(new
        {
            TotalMarketValue = decimal.Round(totalMarketValue, 2, MidpointRounding.AwayFromZero),
            MatchedCount = items.Count,
            MissingSymbols = missingSymbols.OrderBy(symbol => symbol).ToArray(),
            Items = items
        });
    }

    [HttpPut("{symbol}")]
    public IActionResult Upsert(string symbol, [FromBody] UpdatePriceRequest request)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid symbol",
                Detail = "A non-empty symbol is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.Price <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid price",
                Detail = "Price must be greater than zero.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var updated = _service.UpsertPrice(symbol, request.Price);

        Response.Headers.ETag = _service.BuildEtag(updated);
        return Ok(updated);
    }
}
