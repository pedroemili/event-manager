using EventHub.Application.Common.Interfaces;
using EventHub.Domain.Entities.Events;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Repositories;

public sealed class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(EventHubDbContext context) : base(context) { }

    public async Task<Event?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes)
            .Include(e => e.Images)
            .Include(e => e.EventTags)
                .ThenInclude(et => et.Tag)
            .Include(e => e.EventFavorites)
            .Include(e => e.StaffList)
            .FirstOrDefaultAsync(e => e.Slug == slug, cancellationToken);
    }

    public async Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes)
            .Include(e => e.Images)
            .Include(e => e.EventTags)
                .ThenInclude(et => et.Tag)
            .Include(e => e.EventFavorites)
            .Include(e => e.StaffList)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes)
            .Include(e => e.EventFavorites)
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeEventId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return false;
        var query = Context.Events.AsNoTracking().Where(e => e.Slug == slug);
        if (excludeEventId.HasValue)
            query = query.Where(e => e.Id != excludeEventId.Value);

        return !await query.AnyAsync(cancellationToken);
    }
}
