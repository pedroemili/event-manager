using System.Security.Claims;

namespace EventHub.Application.Common.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IReadOnlyList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
    int GetAccessTokenExpirationMinutes();
    int GetRefreshTokenExpirationDays();
}