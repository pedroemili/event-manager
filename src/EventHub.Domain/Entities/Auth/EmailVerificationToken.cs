using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Auth;

public sealed class EmailVerificationToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;

    public User User { get; set; } = null!;
}