using System.Security.Cryptography;
using EventHub.Application.Auth.Commands;
using EventHub.Application.Auth.DTOs;
using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Domain.Entities.Auth;
using EventHub.Domain.Entities.Users;
using EventHub.Shared.Exceptions;
using MediatR;

namespace EventHub.Application.Auth.Handlers;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand>
{
    private readonly IUserRepository _userRepo;

    public RegisterHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepo.IsEmailUniqueAsync(request.Email, cancellationToken))
            throw new BadRequestException("El email ya está en uso.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            IsActive = true
        };

        user.UserRoles.Add(new UserRole
        {
            RoleId = EventHub.Domain.Entities.Users.Role.Ids.Customer
        });

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });

        await _userRepo.AddAsync(user, cancellationToken);
        await _userRepo.SaveChangesAsync(cancellationToken);

    }
}

public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenStore _refreshTokens;
    private readonly IUnitOfWork _uow;

    public LoginHandler(
        IUserRepository userRepo,
        IJwtTokenService jwtService,
        IRefreshTokenStore refreshTokens,
        IUnitOfWork uow)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _uow = uow;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Email o contraseña incorrectos.");
        }

        if (!user.EmailVerified)
            throw new UnauthorizedException("Verifica tu email antes de iniciar sesión.");

        if (!user.IsActive)
            throw new ForbiddenException("Tu cuenta ha sido desactivada.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new ForbiddenException("Cuenta bloqueada temporalmente. Intenta más tarde.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roles.AsReadOnly());
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
            CreatedByIp = request.IpAddress
        };

        await _refreshTokens.AddAsync(refreshToken, cancellationToken);

        var tracked = await _userRepo.GetByIdAsync(user.Id, cancellationToken);
        if (tracked is not null)
        {
            tracked.LastLoginAt = DateTime.UtcNow;
            tracked.FailedLoginAttempts = 0;
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return new AuthResponse(accessToken, refreshTokenValue, _jwtService.GetAccessTokenExpirationMinutes() * 60);
    }
}

public sealed class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenStore _refreshTokens;

    public LogoutHandler(IRefreshTokenStore refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (token is null) return;
        await _refreshTokens.RevokeAsync(token, cancellationToken);
    }
}

public sealed class GetProfileHandler : IRequestHandler<GetProfileQuery, UserProfileResponse>
{
    private readonly IUserRepository _userRepo;

    public GetProfileHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null) throw new NotFoundException(nameof(User), request.UserId);

        return new UserProfileResponse(
            user.Id, user.FirstName, user.LastName, user.Email,
            user.AvatarUrl, user.PhoneNumber, user.EmailVerified,
            user.UserRoles.Select(ur => ur.Role.Name).ToList().AsReadOnly()
        );
    }
}
