# Caching and ETag API

API sample focused on performance-oriented HTTP behavior.

## Features
- In-memory server-side caching with `IMemoryCache`.
- ETag generation from logical item version.
- Conditional GET support via `If-None-Match` and `304 Not Modified`.
- Cache invalidation when data is updated.
- Input validation for symbol and price.
- Health endpoint: `GET /health`.

## Endpoints
- `GET /api/prices`
- `GET /api/prices/{symbol}`
- `PUT /api/prices/{symbol}`

## Sample Update Payload
```json
{
  "price": 76400.25
}
```

## Run
```bash
dotnet run --project src/Caching-And-ETag-Api/Caching.Etag.Api.csproj
```

## Key Files
- `Controllers/PricesController.cs`
- `Services/PriceQueryService.cs`
- `Store/InMemoryPriceStore.cs`
