using Caching.Etag.Api.Application.Services;
using Caching.Etag.Api.Infrastructure.Stores;
using Microsoft.OpenApi.Models;

namespace Caching.Etag.Api.Composition;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddCachingEtagApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Caching and ETag API",
                Version = "v1",
                Description = "Demonstrates IMemoryCache with ETag/If-None-Match semantics."
            });
        });
        services.AddMemoryCache();
        services.AddProblemDetails();
        services.AddHealthChecks();

        services.AddSingleton<IPriceStore, InMemoryPriceStore>();
        services.AddSingleton<IPriceQueryService, PriceQueryService>();

        return services;
    }
}
