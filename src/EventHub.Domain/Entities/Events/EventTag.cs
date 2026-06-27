namespace EventHub.Domain.Entities.Events;

public sealed class EventTag
{
    public Guid EventId { get; set; }
    public Guid TagId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}