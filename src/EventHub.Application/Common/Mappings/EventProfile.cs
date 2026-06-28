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
            .ForMember(d => d.Organizer, o => o.MapFrom(s => s.Organizer == null ? null
                : new OrganizerInfo(s.Organizer.Id, s.Organizer.FirstName, s.Organizer.LastName)))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category == null ? null
                : new CategoryInfo(s.Category.Id, s.Category.Name, s.Category.Slug, s.Category.IconName)))
            .ForMember(d => d.Venue, o => o.MapFrom(s => s.Venue == null ? null
                : new VenueInfo(s.Venue.Id, s.Venue.Name, s.Venue.Address, s.Venue.City,
                    s.Venue.State ?? "", s.Venue.Country,
                    s.Venue.Latitude, s.Venue.Longitude, s.Venue.Capacity)))
            .ForMember(d => d.TicketTypes, o => o.MapFrom(s => MapTicketTypes(s.TicketTypes)))
            .ForMember(d => d.Images, o => o.MapFrom(s => MapImages(s.Images)))
            .ForMember(d => d.Tags, o => o.MapFrom(s => MapTags(s.EventTags)))
            .ForMember(d => d.IsFavorited, o => o.Ignore())
            .ForMember(d => d.FavoriteCount, o => o.MapFrom(s => s.EventFavorites == null ? 0 : s.EventFavorites.Count));

        CreateMap<Event, EventListResponse>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category == null ? null : s.Category.Name))
            .ForMember(d => d.City, o => o.MapFrom(s => s.Venue == null ? null : s.Venue.City))
            .ForMember(d => d.MinPrice, o => o.MapFrom(s =>
                s.TicketTypes != null && s.TicketTypes.Count > 0
                    ? (decimal?)s.TicketTypes.Min(t => t.Price)
                    : null))
            .ForMember(d => d.FavoriteCount, o => o.MapFrom(s => s.EventFavorites == null ? 0 : s.EventFavorites.Count));
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
