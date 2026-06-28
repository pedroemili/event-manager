using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Auth;

public sealed class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public string? RevokedByIp { get; set; }
    public DateTime CreatedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;
}