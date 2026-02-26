using Caching.Etag.Api.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCachingEtagApi();

var app = builder.Build();
app.UseCachingEtagApi();

app.Run();
