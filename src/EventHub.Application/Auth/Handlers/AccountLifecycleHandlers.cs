using System.Security.Cryptography;
using EventHub.Application.Auth.Commands;
using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Domain.Entities.Auth;
using EventHub.Domain.Entities.Users;
using EventHub.Shared.Exceptions;
using MediatR;

namespace EventHub.Application.Auth.Handlers;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;

    public ForgotPasswordHandler(IUserRepository userRepo, IEmailService emailService)
    {
        _userRepo = userRepo;
        _emailService = emailService;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null) return;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });

        await _emailService.SendPasswordResetEmailAsync(user.Email, token, cancellationToken);
    }
}

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordHandler(IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = (await _userRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(u => u.PasswordResetTokens.Any(
                t => t.Token == request.Token && t.ExpiresAt > DateTime.UtcNow && !t.IsUsed));

        if (user is null) throw new BadRequestException("Token inválido o expirado.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        var token = user.PasswordResetTokens.First(t => t.Token == request.Token);
        token.IsUsed = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailHandler(IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepo.GetAllAsync(cancellationToken);
        var token = users
            .SelectMany(u => u.EmailVerificationTokens)
            .FirstOrDefault(t => t.Token == request.Token
                && t.ExpiresAt > DateTime.UtcNow
                && t.UsedAt == null);

        if (token is null) throw new BadRequestException("Token inválido o expirado.");

        token.UsedAt = DateTime.UtcNow;
        var user = users.First(u => u.Id == token.UserId);
        user.EmailVerified = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) throw new NotFoundException(nameof(User), request.UserId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("La contraseña actual no es correcta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
