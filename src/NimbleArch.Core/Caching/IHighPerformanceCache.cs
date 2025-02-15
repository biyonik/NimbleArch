namespace NimbleArch.Core.Caching;

/// <summary>
/// Defines a high-performance cache store.
/// </summary>
/// <remarks>
/// EN: Provides optimized cache operations with minimal allocations and
/// efficient memory usage. Supports both sync and async operations.
///
/// TR: Minimum bellek tahsisi ve verimli bellek kullanımı ile optimize edilmiş
/// önbellek işlemleri sağlar. Hem senkron hem de asenkron işlemleri destekler.
/// </remarks>
public interface IHighPerformanceCache
{
    /// <summary>
    /// Gets a value from cache using a strongly-typed key.
    /// </summary>
    ValueTask<ICacheEntry<T>> GetAsync<T>(ICacheKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with the specified options.
    /// </summary>
    ValueTask SetAsync<T>(ICacheKey key, T value, CacheOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    ValueTask RemoveAsync(ICacheKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache or creates it if it doesn't exist.
    /// </summary>
    ValueTask<ICacheEntry<T>> GetOrCreateAsync<T>(
        ICacheKey key,
        Func<ICacheKey, CancellationToken, ValueTask<T>> factory,
        CacheOptions options,
        CancellationToken cancellationToken = default);
}