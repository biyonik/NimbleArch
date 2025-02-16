using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.DataAccess.Abstract.Specification;
using NimbleArch.Core.Entities;
using NimbleArch.Core.Entities.Caching;
using NimbleArch.Core.Entities.Factory;
using NimbleArch.Core.Entities.Features;
using NimbleArch.Core.MultiTenancy;
using NimbleArch.Infrastructure.Data.Exceptions;
using NimbleArch.Infrastructure.Data.Extensions;

namespace NimbleArch.Infrastructure.Data.Repositories;

/// <summary>
/// High-performance base repository implementation.
/// </summary>
/// <remarks>
/// EN: Provides optimized data access operations using compiled queries,
/// custom caching strategies, and efficient change tracking.
///
/// TR: Derlenmiş sorgular, özel önbellekleme stratejileri ve verimli değişiklik
/// takibi kullanarak optimize edilmiş veri erişim operasyonları sağlar.
/// </remarks>
public abstract class BaseRepository<TEntity, TKey>
    where TEntity : EntityBase<TKey>
    where TKey : struct
{
    private readonly NimbleDbContext _context;
    private readonly IEntityFactory<TEntity, TKey> _entityFactory;
    private readonly EntityCache<TEntity, TKey> _cache;
    private readonly ILogger _logger;

    // Compiled queries for better performance
    private static readonly Func<NimbleDbContext, TKey, CancellationToken, Task<TEntity?>> GetByIdQuery
        = EF.CompileAsyncQuery((NimbleDbContext context, TKey id, CancellationToken ct) =>
            context.Set<TEntity>().FirstOrDefault(e => e.Id.Equals(id)));

    private static readonly Func<NimbleDbContext, IAsyncEnumerable<TEntity>> GetAllQuery
        = EF.CompileAsyncQuery((NimbleDbContext context) =>
            context.Set<TEntity>());

    protected BaseRepository(
        NimbleDbContext context,
        IEntityFactory<TEntity, TKey> entityFactory,
        EntityCache<TEntity, TKey> cache,
        ILogger logger)
    {
        _context = context;
        _entityFactory = entityFactory;
        _cache = cache;
        _logger = logger;
    }

    private async Task<TEntity?> FactoryForGetByIdAsync(TKey key, CancellationToken token)
    {
        return await GetByIdQuery(_context, key, token);
    }

    public virtual async Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get from cache first
            async Task<TEntity?> Factory1(TKey key) => await FactoryForGetByIdAsync(key, cancellationToken);

            var cached = _cache.GetOrCreate(id, Factory1);

            if (cached != null)
            {
                return cached;
            }

            var entity = await GetByIdQuery(_context, id, cancellationToken);
            if (entity != null)
            {
                _cache.Cache(entity);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by id {Id}", id);
            throw new DataAccessException($"Failed to get entity by id {id}", ex);
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = new List<TEntity>();
            await foreach (var entity in GetAllQuery(_context).WithCancellation(cancellationToken))
            {
                entities.Add(entity);
                _cache.Cache(entity);
            }

            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities");
            throw new DataAccessException("Failed to get all entities", ex);
        }
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _cache.Cache(entry.Entity);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity");
            throw new DataAccessException("Failed to add entity", ex);
        }
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _cache.Cache(entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity");
            throw new DataAccessException("Failed to update entity", ex);
        }
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null) return;

            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
                softDeletable.DeletedBy = TenantContext.Current?.Name ?? "System";
                await UpdateAsync(entity, cancellationToken);
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _cache.Invalidate(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity with id {Id}", id);
            throw new DataAccessException($"Failed to delete entity with id {id}", ex);
        }
    }

    protected IQueryable<TEntity> Query()
    {
        return _context.Set<TEntity>().AsNoTracking();
    }

    public virtual async Task<TEntity> FirstOrDefaultAsync(
        IQuerySpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = ApplySpecification(specification);
            var entity = await query.FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                _cache.Cache(entity);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing FirstOrDefault with specification");
            throw new DataAccessException("Failed to execute FirstOrDefault query", ex);
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(
        IQuerySpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = ApplySpecification(specification);
            var entities = await query.ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                _cache.Cache(entity);
            }

            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing List with specification");
            throw new DataAccessException("Failed to execute List query", ex);
        }
    }

    public virtual async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> ListWithPaginationAsync(
        IQuerySpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = ApplySpecification(specification);

            var totalCount = await query
                .TagWith("CountQuery") // Query tag for profiling
                .CountAsync(cancellationToken);

            var entities = await query
                .TagWith("PaginatedQuery") // Query tag for profiling
                .ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                _cache.Cache(entity);
            }

            return (entities, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ListWithPagination with specification");
            throw new DataAccessException("Failed to execute paginated query", ex);
        }
    }

    private IQueryable<TEntity> ApplySpecification(IQuerySpecification<TEntity> specification)
    {
        var query = _context.Set<TEntity>().AsQueryable();

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        var isFirstOrder = true;
        foreach (var (keySelector, ascending) in specification.OrderBy)
        {
            query = isFirstOrder
                ? ascending
                    ? query.OrderBy(keySelector)
                    : query.OrderByDescending(keySelector)
                : ((IOrderedQueryable<TEntity>)query).ApplyThenBy(keySelector, ascending);

            isFirstOrder = false;
        }

        // Apply group by
        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(g => g);
        }

        // Apply pagination
        if (specification.Pagination.HasValue)
        {
            query = query
                .Skip(specification.Pagination.Value.Skip)
                .Take(specification.Pagination.Value.Take);
        }

        return query;
    }
}