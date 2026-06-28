using EventHub.Application.Common;
using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Application.Events.Commands;
using EventHub.Application.Events.DTOs;
using EventHub.Application.Events.Queries;
using EventHub.Domain.Entities.Events;
using EventHub.Domain.Enums;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Helpers;
using EventHub.Shared.Responses;
using MediatR;

namespace EventHub.Application.Events.Handlers;

public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, EventDetailResponse>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateEventHandler(IEventRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EventDetailResponse> Handle(CreateEventCommand cmd, CancellationToken ct)
    {
        var organizerId = _currentUser.UserId ?? throw new UnauthorizedException();
        var slug = SlugHelper.Generate(cmd.Title);
        if (!await _repo.IsSlugUniqueAsync(slug, cancellationToken: ct))
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

        var evt = new Event
        {
            OrganizerId = organizerId,
            Title = cmd.Title,
            Slug = slug,
            ShortDescription = cmd.ShortDescription,
            Description = cmd.Description,
            CategoryId = cmd.CategoryId,
            VenueId = cmd.VenueId,
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            Timezone = cmd.Timezone ?? "UTC",
            MaxAttendees = cmd.MaxAttendees,
            AgeRestriction = cmd.AgeRestriction,
            Visibility = cmd.Visibility
        };

        await _repo.AddAsync(evt, ct);
        await _uow.SaveChangesAsync(ct);

        return EventMapper.ToDetail(evt);
    }
}

public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, EventDetailResponse>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;

    public UpdateEventHandler(IEventRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<EventDetailResponse> Handle(UpdateEventCommand cmd, CancellationToken ct)
    {
        var evt = await _repo.GetByIdAsync(cmd.Id, ct);
        if (evt is null) throw new NotFoundException(nameof(Event), cmd.Id);

        evt.Title = cmd.Title;
        evt.ShortDescription = cmd.ShortDescription;
        evt.Description = cmd.Description;
        evt.CategoryId = cmd.CategoryId;
        evt.VenueId = cmd.VenueId;
        evt.StartDate = cmd.StartDate;
        evt.EndDate = cmd.EndDate;
        evt.Timezone = cmd.Timezone ?? evt.Timezone;
        evt.MaxAttendees = cmd.MaxAttendees;
        evt.AgeRestriction = cmd.AgeRestriction;
        evt.Visibility = cmd.Visibility;

        await _repo.UpdateAsync(evt, ct);
        await _uow.SaveChangesAsync(ct);

        return EventMapper.ToDetail(evt);
    }
}

public sealed class PublishEventHandler : IRequestHandler<PublishEventCommand, EventDetailResponse>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;

    public PublishEventHandler(IEventRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<EventDetailResponse> Handle(PublishEventCommand cmd, CancellationToken ct)
    {
        var evt = await _repo.GetByIdWithDetailsAsync(cmd.Id, ct);
        if (evt is null) throw new NotFoundException(nameof(Event), cmd.Id);
        if (evt.TicketTypes.Count == 0)
            throw new BadRequestException("Debes crear al menos un tipo de boleto.");
        if (evt.Status.ToEventStatus() != EventStatus.Draft)
            throw new BadRequestException("Solo se pueden publicar eventos en Draft.");

        evt.Status = EventStatus.Published.Value();
        evt.PublishedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return EventMapper.ToDetail(evt);
    }
}

public sealed class CancelEventHandler : IRequestHandler<CancelEventCommand, EventDetailResponse>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;

    public CancelEventHandler(IEventRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<EventDetailResponse> Handle(CancelEventCommand cmd, CancellationToken ct)
    {
        var evt = await _repo.GetByIdWithDetailsAsync(cmd.Id, ct);
        if (evt is null) throw new NotFoundException(nameof(Event), cmd.Id);

        evt.Status = EventStatus.Cancelled.Value();
        evt.CancelledAt = DateTime.UtcNow;
        evt.CancellationReason = cmd.Reason;

        foreach (var ticket in evt.Tickets.Where(t => t.Status.ToTicketStatus() == TicketStatus.Active))
            ticket.Status = TicketStatus.Cancelled.Value();

        await _uow.SaveChangesAsync(ct);
        return EventMapper.ToDetail(evt);
    }
}

public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;

    public DeleteEventHandler(IEventRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task Handle(DeleteEventCommand cmd, CancellationToken ct)
    {
        var evt = await _repo.GetByIdAsync(cmd.Id, ct);
        if (evt is null) throw new NotFoundException(nameof(Event), cmd.Id);

        evt.DeletedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
}

public sealed class ToggleFavoriteHandler : IRequestHandler<ToggleFavoriteCommand, bool>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ToggleFavoriteHandler(IEventRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(ToggleFavoriteCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var evt = await _repo.GetByIdWithDetailsAsync(cmd.EventId, ct);
        if (evt is null) throw new NotFoundException(nameof(Event), cmd.EventId);

        var existing = evt.EventFavorites.FirstOrDefault(f => f.UserId == userId);
        if (existing is not null)
        {
            evt.EventFavorites.Remove(existing);
            await _uow.SaveChangesAsync(ct);
            return false;
        }

        evt.EventFavorites.Add(new EventFavorite { UserId = userId, EventId = cmd.EventId });
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetEventBySlugHandler : IRequestHandler<GetEventBySlugQuery, EventDetailResponse?>
{
    private readonly IEventRepository _repo;
    private readonly IUnitOfWork _uow;

    public GetEventBySlugHandler(IEventRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<EventDetailResponse?> Handle(GetEventBySlugQuery q, CancellationToken ct)
    {
        var evt = await _repo.GetBySlugWithDetailsAsync(q.Slug, ct);
        if (evt is null || evt.Status.ToEventStatus() != EventStatus.Published) return null;

        evt.ViewCount++;
        await _uow.SaveChangesAsync(ct);
        return EventMapper.ToDetail(evt);
    }
}

public sealed class GetMyEventsHandler : IRequestHandler<GetMyEventsQuery, IReadOnlyList<EventListResponse>>
{
    private readonly IEventRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetMyEventsHandler(IEventRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<EventListResponse>> Handle(GetMyEventsQuery q, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var allEvents = await _repo.GetByOrganizerAsync(userId, ct);
        return allEvents.Select(EventMapper.ToList).ToList();
    }
}

public sealed class GetPublishedEventsHandler : IRequestHandler<GetPublishedEventsQuery, PagedResult<EventListResponse>>
{
    private readonly IEventRepository _repo;

    public GetPublishedEventsHandler(IEventRepository repo) => _repo = repo;

    public async Task<PagedResult<EventListResponse>> Handle(GetPublishedEventsQuery q, CancellationToken ct)
    {
        var result = await _repo.GetPagedAsync(
            q.Page,
            q.PageSize,
            filter: e => e.Status == EventStatus.Published.Value(),
            cancellationToken: ct);

        return new PagedResult<EventListResponse>
        {
            Items = result.Items.Select(EventMapper.ToList).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}

/// <summary>
/// Centralizes the Entity -> DTO mapping for Events. Removes 30+ duplicate
/// mapping lines that previously lived in Create/Update/Publish/etc handlers.
/// </summary>
internal static class EventMapper
{
    public static EventDetailResponse ToDetail(Event e) => new(
        e.Id, e.OrganizerId, e.Title, e.Slug, e.ShortDescription, e.Description,
        e.Visibility, e.StartDate, e.EndDate, e.Timezone, e.Status,
        e.MainImageUrl, e.ThumbnailUrl, e.CardImageUrl, e.HeroImageUrl,
        e.MaxAttendees, e.AgeRestriction, e.IsFeatured, e.ViewCount, e.ExternalUrl,
        e.PublishedAt, e.CancelledAt, e.CancellationReason, e.RejectionReason, e.CreatedAt,
        e.Organizer is not null ? new OrganizerInfo(e.Organizer.Id, e.Organizer.FirstName, e.Organizer.LastName) : null,
        e.Category is not null ? new CategoryInfo(e.Category.Id, e.Category.Name, e.Category.Slug, e.Category.IconName) : null,
        e.Venue is not null ? new VenueInfo(e.Venue.Id, e.Venue.Name, e.Venue.Address, e.Venue.City, e.Venue.State ?? "", e.Venue.Country, e.Venue.Latitude, e.Venue.Longitude, e.Venue.Capacity) : null,
        e.TicketTypes?.Select(tt => new TicketTypeInfo(tt.Id, tt.Name, tt.Description, tt.Price, tt.Currency, tt.TotalQuantity, tt.SoldQuantity, tt.MinPerOrder, tt.MaxPerOrder, tt.Type, tt.SalesStartAt, tt.SalesEndAt, tt.IsActive, tt.DisplayOrder)).ToList().AsReadOnly() ?? new List<TicketTypeInfo>().AsReadOnly(),
        e.Images?.Select(i => new EventImageInfo(i.Id, i.ImageUrl, i.OrderIndex, i.AltText)).ToList().AsReadOnly() ?? new List<EventImageInfo>().AsReadOnly(),
        e.EventTags?.Select(et => new TagInfo(et.Tag.Id, et.Tag.Name, et.Tag.Slug)).ToList().AsReadOnly() ?? new List<TagInfo>().AsReadOnly(),
        false,
        e.EventFavorites?.Count ?? 0);

    public static EventListResponse ToList(Event e) => new(
        e.Id, e.Title, e.Slug, e.ShortDescription, e.MainImageUrl, e.CardImageUrl,
        e.StartDate, e.EndDate, e.Status, e.IsFeatured,
        e.Category?.Name, e.Venue?.City,
        e.TicketTypes is { Count: > 0 } ? e.TicketTypes.Min(t => t.Price) : null,
        e.EventFavorites?.Count ?? 0,
        e.ViewCount);
}
