using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Events;

public sealed class EventFavorite
{
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
}