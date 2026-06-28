using EventHub.Application.Auth.DTOs;
using MediatR;

namespace EventHub.Application.Auth.Commands;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest;

public sealed record LoginCommand(
    string Email,
    string Password,
    string IpAddress
) : IRequest<AuthResponse>;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string IpAddress
) : IRequest<AuthResponse>;

public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed record ForgotPasswordCommand(string Email) : IRequest;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

public sealed record VerifyEmailCommand(string Token) : IRequest;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest;

public sealed record GetProfileQuery(Guid UserId) : IRequest<UserProfileResponse>;