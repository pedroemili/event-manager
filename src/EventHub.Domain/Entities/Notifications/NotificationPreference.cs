using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Notifications;

public sealed class NotificationPreference
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public bool EventReminders { get; set; } = true;
    public bool TicketPurchase { get; set; } = true;
    public bool EventStatusChanges { get; set; } = true;
    public bool PromotionsAndNews { get; set; }
    public bool StaffInvitations { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}