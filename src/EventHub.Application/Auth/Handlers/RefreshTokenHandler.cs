using EventHub.Application.Auth.Commands;
using EventHub.Application.Auth.DTOs;
using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Domain.Entities.Auth;
using EventHub.Shared.Exceptions;
using MediatR;

namespace EventHub.Application.Auth.Handlers;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenStore _refreshTokens;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;

    public RefreshTokenHandler(
        IJwtTokenService jwtService,
        IRefreshTokenStore refreshTokens,
        IUserRepository userRepo,
        IUnitOfWork uow)
    {
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken is null)
            throw new UnauthorizedException("Refresh token inválido.");

        if (storedToken.IsRevoked)
        {
            await RevokeDescendantTokensAsync(storedToken, request.IpAddress, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Refresh token revocado. Posible intento de reuso detectado.");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expirado.");

        // re-fetch tracked to mutate + insert new sibling in same SaveChanges.
        var trackedStored = await _refreshTokens.GetByTokenForUpdateAsync(request.RefreshToken, cancellationToken);
        if (trackedStored is null)
            throw new UnauthorizedException("Refresh token inválido.");

        var user = await _userRepo.GetByIdWithRolesAsync(trackedStored.UserId, cancellationToken);
        if (user is null) throw new UnauthorizedException();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roles.AsReadOnly());
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        trackedStored.IsRevoked = true;
        trackedStored.RevokedAt = DateTime.UtcNow;
        trackedStored.RevokedByIp = request.IpAddress;
        trackedStored.ReplacedByToken = newRefreshTokenValue;

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
            CreatedByIp = request.IpAddress
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshTokenValue, _jwtService.GetAccessTokenExpirationMinutes() * 60);
    }

    private async Task RevokeDescendantTokensAsync(RefreshToken token, string ip, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token.ReplacedByToken)) return;

        var descendant = await _refreshTokens.GetByTokenForUpdateAsync(token.ReplacedByToken, ct);
        if (descendant is null || descendant.IsRevoked) return;

        descendant.IsRevoked = true;
        descendant.RevokedAt = DateTime.UtcNow;
        descendant.RevokedByIp = ip;
    }
}
