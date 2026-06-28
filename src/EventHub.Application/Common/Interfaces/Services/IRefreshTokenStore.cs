using EventHub.Domain.Entities.Auth;

namespace EventHub.Application.Common.Interfaces.Services;

public interface IRefreshTokenStore
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked read for mutation. The token must be persisted back via
    /// IUnitOfWork.SaveChangesAsync once the caller has applied changes.
    /// </summary>
    Task<RefreshToken?> GetByTokenForUpdateAsync(string token, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    void MarkRevoked(RefreshToken token);
}
