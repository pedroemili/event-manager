namespace EventHub.Application.Auth.DTOs;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public sealed record UserProfileResponse(Guid Id, string FirstName, string LastName, string Email, string? AvatarUrl, string? PhoneNumber, bool EmailVerified, IReadOnlyList<string> Roles);