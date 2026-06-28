using EventHub.Domain.Entities.Events;

namespace EventHub.Application.Common.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeEventId = null, CancellationToken cancellationToken = default);
}