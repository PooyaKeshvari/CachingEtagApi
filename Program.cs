using Caching.Etag.Api.Services;
using Caching.Etag.Api.Store;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Caching and ETag API",
        Version = "v1",
        Description = "Demonstrates IMemoryCache with ETag/If-None-Match semantics."
    });
});
builder.Services.AddMemoryCache();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IPriceStore, InMemoryPriceStore>();
builder.Services.AddSingleton<IPriceQueryService, PriceQueryService>();

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/", () => Results.Ok(new { service = "Caching.Etag.Api", status = "Running" }));
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
