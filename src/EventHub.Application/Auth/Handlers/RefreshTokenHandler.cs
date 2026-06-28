using EventHub.Application.Auth.Commands;
using EventHub.Application.Auth.DTOs;
using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Domain.Entities.Auth;
using EventHub.Domain.Entities.Users;
using EventHub.Shared.Exceptions;
using MediatR;

namespace EventHub.Application.Auth.Handlers;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenStore _refreshTokens;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IJwtTokenService jwtService,
        IRefreshTokenStore refreshTokens,
        IUserRepository userRepo,
        IUnitOfWork unitOfWork)
    {
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken is null)
            throw new UnauthorizedException("Refresh token inválido.");

        if (storedToken.IsRevoked)
        {
            await RevokeDescendantTokensAsync(storedToken, request.IpAddress, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Refresh token revocado. Posible intento de reuso detectado.");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expirado.");

        var user = await _userRepo.GetByIdWithRolesAsync(storedToken.UserId, cancellationToken);
        if (user is null) throw new UnauthorizedException();

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roles.AsReadOnly());
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        storedToken.ReplacedByToken = newRefreshToken;

        user.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
            CreatedByIp = request.IpAddress
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken, _jwtService.GetAccessTokenExpirationMinutes() * 60);
    }

    private async Task RevokeDescendantTokensAsync(RefreshToken token, string ip, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token.ReplacedByToken)) return;

        var descendant = await _refreshTokens.GetByTokenAsync(token.ReplacedByToken, ct);
        if (descendant is null || descendant.IsRevoked) return;

        descendant.IsRevoked = true;
        descendant.RevokedAt = DateTime.UtcNow;
        descendant.RevokedByIp = ip;
    }
}
