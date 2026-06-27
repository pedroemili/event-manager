namespace EventHub.Domain.Entities.Events;

public sealed class ScheduledPublication
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public bool IsExecuted { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
}