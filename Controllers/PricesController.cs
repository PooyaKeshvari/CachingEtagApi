using Caching.Etag.Api.Contracts;
using Caching.Etag.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caching.Etag.Api.Controllers;

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
