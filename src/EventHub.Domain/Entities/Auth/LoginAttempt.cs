using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Auth;

public sealed class LoginAttempt
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTime AttemptedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;

    public User? User { get; set; }
}