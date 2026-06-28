namespace EventHub.Application.Venues.DTOs;

public sealed record VenueResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    string? State,
    string Country,
    decimal? Latitude,
    decimal? Longitude,
    int Capacity);
