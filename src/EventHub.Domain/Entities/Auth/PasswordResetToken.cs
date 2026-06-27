using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Auth;

public sealed class PasswordResetToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;

    public User User { get; set; } = null!;
}