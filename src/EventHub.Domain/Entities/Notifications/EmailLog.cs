using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Notifications;

public sealed class EmailLog
{
    public Guid Id { get; init; }
    public Guid? UserId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}