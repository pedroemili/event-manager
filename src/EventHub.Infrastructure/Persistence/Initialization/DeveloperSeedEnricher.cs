using EventHub.Domain.Entities.Auth;
using EventHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Persistence.Initialization;

/// <summary>
/// Adds a default developer user (admin@eventhub.local) on a fresh database
/// so the API is testable without going through the public registration flow.
/// </summary>
public sealed class DeveloperSeedEnricher
{
    private readonly EventHubDbContext _context;
    private readonly ILogger<DeveloperSeedEnricher> _logger;

    public DeveloperSeedEnricher(EventHubDbContext context, ILogger<DeveloperSeedEnricher> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureDeveloperUserAsync(CancellationToken cancellationToken = default)
    {
        var email = "admin@eventhub.local";
        var existing = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (existing)
        {
            _logger.LogDebug("Developer user {Email} already exists.", email);
            return;
        }

        var adminRoleId = JwtRoleIds.Admin;

        var user = new User
        {
            Email = email,
            FirstName = "Dev",
            LastName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("DevAdmin!2026", 12),
            EmailVerified = true,
            IsActive = true
        };

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRoleId });
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded developer Admin user ({Email}). Email-verified, password 'DevAdmin!2026'.", email);
    }
}

/// <summary>
/// Hardcoded Admin role GUID from the seed file. We reference it instead
/// of querying Roles to avoid an extra round-trip.
/// </summary>
internal static class JwtRoleIds
{
    public static readonly Guid Admin = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
}
