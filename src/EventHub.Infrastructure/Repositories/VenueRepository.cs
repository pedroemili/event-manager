using EventHub.Application.Common.Interfaces;
using EventHub.Domain.Entities.Venues;
using EventHub.Infrastructure.Persistence;

namespace EventHub.Infrastructure.Repositories;

public sealed class VenueRepository : Repository<Venue>, IVenueRepository
{
    public VenueRepository(EventHubDbContext context) : base(context) { }
}
