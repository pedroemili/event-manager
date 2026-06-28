using EventHub.Application.Common.Interfaces.Services;
using EventHub.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using EventHub.Infrastructure.Persistence;

namespace EventHub.Infrastructure.Services;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly EventHubDbContext _context;

    public RefreshTokenStore(EventHubDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}
