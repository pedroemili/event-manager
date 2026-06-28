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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public RegisterHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12)
        };

        user.UserRoles.Add(new UserRole { RoleId = EventHub.Domain.Entities.Users.Role.Ids.Customer });

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });

        await _userRepo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationEmailAsync(user.Email, token, cancellationToken);
    }
}

public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(IUserRepository userRepo, IJwtTokenService jwtService, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
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
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
            CreatedByIp = request.IpAddress
        });

        user.LastLoginAt = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken, _jwtService.GetAccessTokenExpirationMinutes() * 60);
    }
}

public sealed class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepo.GetAllAsync(cancellationToken);
        var token = users.SelectMany(u => u.RefreshTokens)
            .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (token is not null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
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