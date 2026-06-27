namespace EventHub.Domain.Entities.Events;

public sealed class EventImage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? AltText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
}