using System.Linq.Expressions;
using EventHub.Application.Common.Interfaces;
using EventHub.Infrastructure.Persistence;
using EventHub.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly EventHubDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(EventHubDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = DbSet.AsNoTracking();
        if (filter != null)
            query = query.Where(filter);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));
        return await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        if (predicate != null)
            query = query.Where(predicate);
        return await query.CountAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}
