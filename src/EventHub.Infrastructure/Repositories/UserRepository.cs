using EventHub.Application.Common.Interfaces;
using EventHub.Domain.Entities.Users;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(EventHubDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var normalized = email.ToLowerInvariant();
        return await Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(u => u.EmailVerificationTokens)
            .Include(u => u.PasswordResetTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var record = await Context.EmailVerificationTokens
            .AsNoTracking()
            .Where(t => t.Token == token
                && t.ExpiresAt > DateTime.UtcNow
                && t.UsedAt == null)
            .Select(t => new { t.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null) return null;
        return await Context.Users
            .Include(u => u.EmailVerificationTokens)
            .FirstOrDefaultAsync(u => u.Id == record.UserId, cancellationToken);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var record = await Context.PasswordResetTokens
            .AsNoTracking()
            .Where(t => t.Token == token
                && t.ExpiresAt > DateTime.UtcNow
                && !t.IsUsed)
            .Select(t => new { t.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null) return null;
        return await Context.Users
            .Include(u => u.PasswordResetTokens)
            .FirstOrDefaultAsync(u => u.Id == record.UserId, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var normalized = email.ToLowerInvariant();
        return !await Context.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
    }
}
