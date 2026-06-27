
using EventHub.Domain.Entities.Staff;
using EventHub.Domain.Entities.Tickets;
using EventHub.Domain.Entities.Users;
using EventHub.Domain.Entities.Venues;

namespace EventHub.Domain.Entities.Events;

public sealed class Event
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OrganizerId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string Visibility { get; set; } = "Public";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string Status { get; set; } = "Draft";
    public string? MainImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? CardImageUrl { get; set; }
    public string? HeroImageUrl { get; set; }
    public int? MaxAttendees { get; set; }
    public int? AgeRestriction { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public string? ExternalUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public User Organizer { get; set; } = null!;
    public Category? Category { get; set; }
    public Venue? Venue { get; set; }
    public ScheduledPublication? ScheduledPublication { get; set; }
    public ICollection<EventImage> Images { get; set; } = new List<EventImage>();
    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<EventFavorite> EventFavorites { get; set; } = new List<EventFavorite>();
    public ICollection<EventStaff> StaffList { get; set; } = new List<EventStaff>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<DiscountCode> DiscountCodes { get; set; } = new List<DiscountCode>();
}