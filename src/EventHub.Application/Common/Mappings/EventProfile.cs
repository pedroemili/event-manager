using AutoMapper;
using EventHub.Application.Events.DTOs;
using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Tickets;

namespace EventHub.Application.Common.Mappings;

public sealed class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<Event, EventDetailResponse>()
            .ConstructUsing(src => new EventDetailResponse(
                src.Id, src.OrganizerId, src.Title, src.Slug, src.ShortDescription, src.Description,
                src.Visibility, src.StartDate, src.EndDate, src.Timezone, src.Status,
                src.MainImageUrl, src.ThumbnailUrl, src.CardImageUrl, src.HeroImageUrl,
                src.MaxAttendees, src.AgeRestriction, src.IsFeatured, src.ViewCount, src.ExternalUrl,
                src.PublishedAt, src.CancelledAt, src.CancellationReason, src.RejectionReason, src.CreatedAt,
                src.Organizer == null ? null : new OrganizerInfo(src.Organizer.Id, src.Organizer.FirstName, src.Organizer.LastName),
                src.Category == null ? null : new CategoryInfo(src.Category.Id, src.Category.Name, src.Category.Slug, src.Category.IconName),
                src.Venue == null ? null : new VenueInfo(src.Venue.Id, src.Venue.Name, src.Venue.Address, src.Venue.City,
                    src.Venue.State ?? "", src.Venue.Country, src.Venue.Latitude, src.Venue.Longitude, src.Venue.Capacity),
                MapTicketTypes(src.TicketTypes),
                MapImages(src.Images),
                MapTags(src.EventTags),
                false,
                src.EventFavorites == null ? 0 : src.EventFavorites.Count))
            .ForMember(d => d.Organizer, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Venue, o => o.Ignore())
            .ForMember(d => d.TicketTypes, o => o.Ignore())
            .ForMember(d => d.Images, o => o.Ignore())
            .ForMember(d => d.Tags, o => o.Ignore())
            .ForMember(d => d.IsFavorited, o => o.Ignore())
            .ForMember(d => d.FavoriteCount, o => o.Ignore());

        CreateMap<Event, EventListResponse>()
            .ConstructUsing(src => new EventListResponse(
                src.Id, src.Title, src.Slug, src.ShortDescription, src.MainImageUrl, src.CardImageUrl,
                src.StartDate, src.EndDate, src.Status, src.IsFeatured,
                src.Category == null ? null : src.Category.Name,
                src.Venue == null ? null : src.Venue.City,
                src.TicketTypes != null && src.TicketTypes.Count > 0
                    ? (decimal?)src.TicketTypes.Min(t => t.Price)
                    : null,
                src.EventFavorites == null ? 0 : src.EventFavorites.Count,
                src.ViewCount))
            .ForMember(d => d.CategoryName, o => o.Ignore())
            .ForMember(d => d.City, o => o.Ignore())
            .ForMember(d => d.MinPrice, o => o.Ignore())
            .ForMember(d => d.FavoriteCount, o => o.Ignore());
    }

    private static IReadOnlyList<TicketTypeInfo> MapTicketTypes(ICollection<TicketType> ticketTypes)
    {
        if (ticketTypes == null) return Array.Empty<TicketTypeInfo>();
        return ticketTypes.Select(tt => new TicketTypeInfo(
            tt.Id, tt.Name, tt.Description, tt.Price, tt.Currency,
            tt.TotalQuantity, tt.SoldQuantity, tt.MinPerOrder, tt.MaxPerOrder,
            tt.Type, tt.SalesStartAt, tt.SalesEndAt, tt.IsActive, tt.DisplayOrder)).ToList();
    }

    private static IReadOnlyList<EventImageInfo> MapImages(ICollection<EventImage> images)
    {
        if (images == null) return Array.Empty<EventImageInfo>();
        return images.Select(i => new EventImageInfo(i.Id, i.ImageUrl, i.OrderIndex, i.AltText)).ToList();
    }

    private static IReadOnlyList<TagInfo> MapTags(ICollection<EventTag> eventTags)
    {
        if (eventTags == null) return Array.Empty<TagInfo>();
        return eventTags.Select(et => new TagInfo(et.Tag.Id, et.Tag.Name, et.Tag.Slug)).ToList();
    }
}
