using EventHub.Domain.Entities.Auth;

namespace EventHub.Application.Common.Interfaces.Services;

public interface IRefreshTokenStore
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
