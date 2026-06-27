using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Venues;

public sealed class Venue
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public User Creator { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}