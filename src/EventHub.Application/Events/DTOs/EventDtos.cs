namespace EventHub.Application.Events.DTOs;

public sealed record EventListResponse(
    Guid Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? MainImageUrl,
    string? CardImageUrl,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    bool IsFeatured,
    string? CategoryName,
    string? City,
    decimal? MinPrice,
    int? FavoriteCount,
    int? ViewCount
);

public sealed record EventDetailResponse(
    Guid Id,
    Guid OrganizerId,
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string Visibility,
    DateTime StartDate,
    DateTime EndDate,
    string Timezone,
    string Status,
    string? MainImageUrl,
    string? ThumbnailUrl,
    string? CardImageUrl,
    string? HeroImageUrl,
    int? MaxAttendees,
    int? AgeRestriction,
    bool IsFeatured,
    int ViewCount,
    string? ExternalUrl,
    DateTime? PublishedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    string? RejectionReason,
    DateTime CreatedAt,
    OrganizerInfo? Organizer,
    CategoryInfo? Category,
    VenueInfo? Venue,
    IReadOnlyList<TicketTypeInfo>? TicketTypes,
    IReadOnlyList<EventImageInfo>? Images,
    IReadOnlyList<TagInfo>? Tags,
    bool IsFavorited,
    int FavoriteCount
);

public sealed record OrganizerInfo(Guid Id, string FirstName, string LastName);
public sealed record CategoryInfo(Guid Id, string Name, string Slug, string? IconName);
public sealed record VenueInfo(Guid Id, string Name, string Address, string City, string State, string Country, decimal? Latitude, decimal? Longitude, int Capacity);
public sealed record TicketTypeInfo(Guid Id, string Name, string? Description, decimal Price, string Currency, int TotalQuantity, int SoldQuantity, int MinPerOrder, int MaxPerOrder, string Type, DateTime? SalesStartAt, DateTime? SalesEndAt, bool IsActive, int DisplayOrder);
public sealed record EventImageInfo(Guid Id, string ImageUrl, int OrderIndex, string? AltText);
public sealed record TagInfo(Guid Id, string Name, string Slug);