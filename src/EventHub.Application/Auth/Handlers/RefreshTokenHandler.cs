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
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(IJwtTokenService jwtService, IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _jwtService = jwtService;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var allUsers = await _userRepo.GetAllAsync(cancellationToken);
        var storedToken = allUsers
            .SelectMany(u => u.RefreshTokens)
            .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            if (storedToken is not null)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            throw new UnauthorizedException("Refresh token inválido o expirado.");
        }

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
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
            CreatedByIp = request.IpAddress
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken, _jwtService.GetAccessTokenExpirationMinutes() * 60);
    }
}