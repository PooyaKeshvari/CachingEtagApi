# Caching and ETag API

API sample focused on performance-oriented HTTP behavior.

## Features
- In-memory server-side caching with `IMemoryCache`.
- ETag generation from logical item version (single item and collection).
- Conditional GET support via `If-None-Match` and `304 Not Modified`.
- Cache invalidation when data is updated.
- Input validation for symbol, price, and watchlist holdings.
- Health endpoint: `GET /health`.
- Swagger/OpenAPI UI in Development at `/swagger`.

## Endpoints
- `GET /api/prices`
- `GET /api/prices/{symbol}`
- `GET /api/prices/watchlist?symbols=BTC,ETH`
- `GET /api/prices/stale?olderThanSeconds=300`
- `POST /api/prices/watchlist/value`
- `PUT /api/prices/{symbol}`

## Sample Update Payload
```json
{
  "price": 76400.25
}
```

## Sample Watchlist Value Payload
```json
{
  "holdings": [
    { "symbol": "BTC", "quantity": 2 },
    { "symbol": "ETH", "quantity": 10 }
  ]
}
```

## Run
```bash
dotnet run --project Caching.Etag.Api.csproj
```

## Key Files
- `Presentation/Controllers/PricesController.cs`
- `Application/Services/PriceQueryService.cs`
- `Infrastructure/Stores/InMemoryPriceStore.cs`
- `Composition/ServiceRegistrationExtensions.cs`, `Composition/PipelineExtensions.cs`
