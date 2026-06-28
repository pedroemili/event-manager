using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Domain.Entities.Auth;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Services;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly EventHubDbContext _context;
    private readonly IUnitOfWork _uow;

    public RefreshTokenStore(EventHubDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(token, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
