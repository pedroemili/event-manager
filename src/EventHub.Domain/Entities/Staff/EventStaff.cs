using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Staff;

public sealed class EventStaff
{
    public Guid Id { get; init; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public Guid InvitedBy { get; set; }
    public string? StaffRole { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
    public User Inviter { get; set; } = null!;
}