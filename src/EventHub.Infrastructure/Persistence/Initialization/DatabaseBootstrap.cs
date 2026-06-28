using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Persistence.Initialization;

/// <summary>
/// Static entry-point used by Program.cs at startup. Wraps the schema
/// bootstrap + (optional) developer seeding behind a single call.
/// </summary>
public static class DatabaseBootstrap
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<PostgresDatabaseInitializer>>();

        var init = sp.GetRequiredService<PostgresDatabaseInitializer>();
        await init.InitializeAsync(ct);

        var enricher = sp.GetRequiredService<DeveloperSeedEnricher>();
        await enricher.EnsureDeveloperUserAsync(ct);

        logger.LogInformation("Database initialization complete.");
    }
}
