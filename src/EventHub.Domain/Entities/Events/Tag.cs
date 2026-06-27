namespace EventHub.Domain.Entities.Events;

public sealed class Tag
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
}