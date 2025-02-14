using NimbleArch.Core.DataAccess.Abstract.Specification;

namespace NimbleArch.Core.DataAccess.Abstract.Executor;

/// <summary>
/// Defines contract for executing queries based on specifications.
/// </summary>
/// <remarks>
/// EN: High-performance query executor that optimizes query execution using
/// compiled expressions and efficient query plans. Supports both synchronous
/// and asynchronous operations.
///
/// TR: Derlenmiş expression'lar ve verimli sorgu planları kullanarak sorgu
/// yürütmeyi optimize eden yüksek performanslı sorgu yürütücüsü. Hem senkron
/// hem de asenkron operasyonları destekler.
/// </remarks>
public interface IQueryExecutor
{
    /// <summary>
    /// Executes a query and returns a single result.
    /// </summary>
    Task<T?> FirstOrDefaultAsync<T>(IQuerySpecification<T> spec, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Executes a query and returns all matching results.
    /// </summary>
    Task<List<T>> ToListAsync<T>(IQuerySpecification<T> spec, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Executes a query and returns a paginated result.
    /// </summary>
    Task<(List<T> Items, int TotalCount)> ToPaginatedListAsync<T>(
        IQuerySpecification<T> spec,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    Task<bool> AnyAsync<T>(IQuerySpecification<T> spec, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Counts entities matching the specification.
    /// </summary>
    Task<int> CountAsync<T>(IQuerySpecification<T> spec, CancellationToken cancellationToken = default) where T : class;
}