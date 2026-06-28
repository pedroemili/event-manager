using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Persistence.Initialization;

/// <summary>
/// Idempotent Postgres schema + seed bootstrap. Runs the bundled
/// InitialCreate.sql on startup when the public schema is empty.
/// </summary>
/// <remarks>
/// Why raw SQL instead of EF Core Migrations:
///   - We control the schema wholesale (enums-as-VARCHAR, custom CHECK constraints,
///     pgcrypto extension, partial unique indexes, sequence order_number_seq...).
///   - This avoids a noisy one-off EF migration that would diverge from the
///     hand-rolled schema already in production-equivalent state.
/// </remarks>
public sealed class PostgresDatabaseInitializer
{
    private readonly EventHubDbContext _context;
    private readonly ILogger<PostgresDatabaseInitializer> _logger;

    public PostgresDatabaseInitializer(EventHubDbContext context, ILogger<PostgresDatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        await EnsureSeedAsync(cancellationToken);
    }

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        var hasTables = await _context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*)::int AS \"Value\" FROM information_schema.tables WHERE table_schema = 'public'")
            .FirstAsync(cancellationToken);

        if (hasTables > 0)
        {
            _logger.LogInformation(
                "Database schema already initialized (tables in public schema: {Count}). Skipping InitialCreate.sql.",
                hasTables);
            return;
        }

        _logger.LogInformation("Public schema is empty. Applying InitialCreate.sql...");

        var sql = await LoadEmbeddedSqlAsync(cancellationToken);

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);

        _logger.LogInformation("InitialCreate.sql applied successfully.");
    }

    private async Task EnsureSeedAsync(CancellationToken cancellationToken)
    {
        // Seed data lives inside InitialCreate.sql already. This method is the
        // extension point if we ever need to programmatically seed developer
        // environments with sample users (e.g., a default Admin).
        await Task.CompletedTask;
    }

    private async Task<string> LoadEmbeddedSqlAsync(CancellationToken cancellationToken)
    {
        // Lives next to the executing assembly: EventHub.Infrastructure.Persistence.Migrations.InitialCreate.sql
        var asm = Assembly.GetExecutingAssembly();
        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("InitialCreate.sql", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            // Fallback to file path (during `dotnet build -c Debug` the file is
            // copied to the bin directory).
            var candidate = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence", "Migrations", "InitialCreate.sql");
            if (File.Exists(candidate))
                return await File.ReadAllTextAsync(candidate, cancellationToken);

            throw new FileNotFoundException(
                $"InitialCreate.sql not found as embedded resource or at '{candidate}'.");
        }

        await using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be opened.");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
