using EventHub.Domain.Entities.Events;

namespace EventHub.Application.Common.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
}