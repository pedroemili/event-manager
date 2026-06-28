using EventHub.Application.Common.Interfaces;
using EventHub.Domain.Entities.Tickets;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Repositories;

public sealed class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(EventHubDbContext context) : base(context) { }

    public async Task<Ticket?> GetByQrDataAsync(string qrData, CancellationToken cancellationToken = default)
    {
        return await Context.Tickets
            .AsNoTracking()
            .Include(t => t.Event)
                .ThenInclude(e => e.Venue)
            .Include(t => t.OrderItem)
            .FirstOrDefaultAsync(t => t.QrCodeData == qrData, cancellationToken);
    }

    /// <summary>
    /// Tracked read for check-in mutations. Caller is responsible for
    /// persisting via UnitOfWork. Use a transaction at the handler level
    /// to make concurrent scan races safe.
    /// </summary>
    public async Task<Ticket?> GetByQrDataForUpdateAsync(string qrData, CancellationToken cancellationToken = default)
    {
        return await Context.Tickets
            .Include(t => t.Event)
                .ThenInclude(e => e.Venue)
            .Include(t => t.OrderItem)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.QrCodeData == qrData, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Tickets
            .AsNoTracking()
            .Include(t => t.Event)
                .ThenInclude(e => e.Venue)
            .Include(t => t.OrderItem)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.Tickets
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .ToListAsync(cancellationToken);
    }
}
