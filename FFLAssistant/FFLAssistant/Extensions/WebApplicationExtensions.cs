using FFLAssistant.Services;

namespace FFLAssistant.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> InitializeCriticalDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            logger.LogInformation("Starting critical data initialization...");

            var initService = scope.ServiceProvider.GetRequiredService<DataFileInitializationService>();
            await initService.InitializeAsync();

            logger.LogInformation("Critical data initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to initialize critical data. Application startup aborted.");
            throw;
        }

        return app;
    }
}