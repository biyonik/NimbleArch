using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.DataAccess.Abstract.Executor;
using NimbleArch.Core.DataAccess.Abstract.Specification;
using NimbleArch.Core.DataAccess.EFCore.Extensions;

namespace NimbleArch.Core.DataAccess.Impl.Executor;

/// <summary>
/// High-performance Entity Framework Core query executor implementation.
/// </summary>
/// <remarks>
/// EN: Implements optimized query execution for EF Core using compiled queries,
/// expression optimization, and query result caching. Provides advanced performance
/// monitoring and query optimization capabilities.
///
/// TR: Derlenmiş sorgular, expression optimizasyonu ve sorgu sonucu önbellekleme
/// kullanarak EF Core için optimize edilmiş sorgu yürütme implementasyonu. Gelişmiş
/// performans izleme ve sorgu optimizasyon yetenekleri sağlar.
/// </remarks>
public class EFQueryExecutor(
    DbContext context,
    ILogger<EFQueryExecutor> logger) : IQueryExecutor
{
    private readonly ConcurrentDictionary<string, object> _compiledQueries = new();

   /// <summary>
   /// Creates an optimized queryable based on the specification.
   /// </summary>
   private IQueryable<T> CreateOptimizedQuery<T>(IQuerySpecification<T> spec) where T : class
   {
       var query = context.Set<T>().AsQueryable();

       // Apply criteria
       if (spec.Criteria != null)
       {
           query = query.Where(spec.Criteria);
       }

       // Apply includes
       query = spec.Includes.Aggregate(query, (current, include) => IsCollectionInclude(include) 
           ? current.Include(include) : current.Include(include.AsStringPath()));

       // Apply ordering
       var isFirstOrder = true;
       foreach (var (keySelector, ascending) in spec.OrderBy)
       {
           query = isFirstOrder
               ? ascending 
                   ? query.OrderBy(keySelector)
                   : query.OrderByDescending(keySelector)
               : ascending
                   ? ((IOrderedQueryable<T>)query).ThenBy(keySelector)
                   : ((IOrderedQueryable<T>)query).ThenByDescending(keySelector);
           
           isFirstOrder = false;
       }

       // Apply grouping
       if (spec.GroupBy != null)
       {
           query = query.GroupBy(spec.GroupBy).SelectMany(g => g);
       }

       // Apply pagination
       if (spec.Pagination.HasValue)
       {
           query = query.Skip(spec.Pagination.Value.Skip)
                       .Take(spec.Pagination.Value.Take);
       }

       return query;
   }

   public Task<T?> FirstOrDefaultAsync<T>(IQuerySpecification<T> spec,
       CancellationToken cancellationToken = default) where T : class
   {
       var cacheKey = GetQueryCacheKey(spec, nameof(FirstOrDefaultAsync));
       var compiledQuery = GetOrCreateCompiledQuery(cacheKey, spec, q => 
           EF.CompileQuery((DbContext ctx) => 
               CreateOptimizedQuery(spec).FirstOrDefault()));

       using var activity = StartQueryActivity(nameof(FirstOrDefaultAsync), spec);
       try
       {
           return Task.FromResult(compiledQuery(context));
       }
       catch (Exception ex)
       {
           logger.LogError(ex, "Error executing FirstOrDefaultAsync for {EntityType}", typeof(T).Name);
           throw;
       }
   }

   public Task<List<T>> ToListAsync<T>(
       IQuerySpecification<T> spec, 
       CancellationToken cancellationToken = default) where T : class
   {
       var cacheKey = GetQueryCacheKey(spec, nameof(ToListAsync));
       var compiledQuery = GetOrCreateCompiledQuery(cacheKey, spec, q => 
           EF.CompileQuery((DbContext ctx) => 
               CreateOptimizedQuery(spec).ToList()));

       using var activity = StartQueryActivity(nameof(ToListAsync), spec);
       try
       {
           return Task.FromResult(compiledQuery(context));
       }
       catch (Exception ex)
       {
           logger.LogError(ex, "Error executing ToListAsync for {EntityType}", typeof(T).Name);
           throw;
       }
   }

   public async Task<(List<T> Items, int TotalCount)> ToPaginatedListAsync<T>(
       IQuerySpecification<T> spec,
       CancellationToken cancellationToken = default) where T : class
   {
       // For pagination, we need two queries: one for items and one for total count
       var itemsQuery = CreateOptimizedQuery(spec);
       var countQuery = context.Set<T>().AsQueryable();
       
       if (spec.Criteria != null)
       {
           countQuery = countQuery.Where(spec.Criteria);
       }

       using var activity = StartQueryActivity(nameof(ToPaginatedListAsync), spec);
       try
       {
           var totalCount = await countQuery.CountAsync(cancellationToken);
           var items = await itemsQuery.ToListAsync(cancellationToken);
           
           return (items, totalCount);
       }
       catch (Exception ex)
       {
           logger.LogError(ex, "Error executing ToPaginatedListAsync for {EntityType}", typeof(T).Name);
           throw;
       }
   }

   public Task<bool> AnyAsync<T>(
       IQuerySpecification<T> spec, 
       CancellationToken cancellationToken = default) where T : class
   {
       var cacheKey = GetQueryCacheKey(spec, nameof(AnyAsync));
       var compiledQuery = GetOrCreateCompiledQuery(cacheKey, spec, q => 
           EF.CompileQuery((DbContext ctx) => 
               CreateOptimizedQuery(spec).Any()));

       using var activity = StartQueryActivity(nameof(AnyAsync), spec);
       try
       {
           return Task.FromResult(compiledQuery(context));
       }
       catch (Exception ex)
       {
           logger.LogError(ex, "Error executing AnyAsync for {EntityType}", typeof(T).Name);
           throw;
       }
   }

   public Task<int> CountAsync<T>(
       IQuerySpecification<T> spec, 
       CancellationToken cancellationToken = default) where T : class
   {
       var cacheKey = GetQueryCacheKey(spec, nameof(CountAsync));
       var compiledQuery = GetOrCreateCompiledQuery(cacheKey, spec, q => 
           EF.CompileQuery((DbContext ctx) => 
               CreateOptimizedQuery(spec).Count()));

       using var activity = StartQueryActivity(nameof(CountAsync), spec);
       try
       {
           return Task.FromResult(compiledQuery(context));
       }
       catch (Exception ex)
       {
           logger.LogError(ex, "Error executing CountAsync for {EntityType}", typeof(T).Name);
           throw;
       }
   }

   private Activity StartQueryActivity<T>(string operationName, IQuerySpecification<T> spec)
   {
       var activity = new Activity($"EFQueryExecutor.{operationName}")
           .SetTag("entity.type", typeof(T).Name)
           .SetTag("query.type", operationName)
           .Start();

       return activity;
   }

   private string GetQueryCacheKey<T>(IQuerySpecification<T> spec, string operation)
   {
       return $"{typeof(T).Name}_{operation}_{spec.GetHashCode()}";
   }

   private Func<DbContext, TResult> GetOrCreateCompiledQuery<T, TResult>(
       string cacheKey,
       IQuerySpecification<T> spec,
       Func<IQueryable<T>, Func<DbContext, TResult>> compiler) where T : class
   {
       return (_compiledQueries.GetOrAdd(cacheKey, _ => compiler(CreateOptimizedQuery(spec)))
           as Func<DbContext, TResult>)!;
   }

   private bool IsCollectionInclude<T>(Expression<Func<T, object>> include)
   {
       if (include.Body is not MemberExpression memberExpression) return false;
       
       var propertyType = memberExpression.Type;
       return propertyType.IsGenericType && 
              (propertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
               propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
   }
}