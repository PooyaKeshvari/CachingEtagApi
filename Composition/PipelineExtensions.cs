namespace Caching.Etag.Api.Composition;

public static class PipelineExtensions
{
    public static WebApplication UseCachingEtagApi(this WebApplication app)
    {
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

        return app;
    }
}
