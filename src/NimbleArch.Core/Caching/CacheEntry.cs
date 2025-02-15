namespace NimbleArch.Core.Caching;

/// <summary>
/// Default implementation of ICacheEntry.
/// </summary>
/// <remarks>
/// EN: Provides a basic implementation of cache entry with all required metadata.
/// Uses struct for performance optimization.
///
/// TR: Gerekli tüm meta verilerle temel bir önbellek girdisi implementasyonu sağlar.
/// Performans optimizasyonu için struct kullanır.
/// </remarks>
public readonly struct CacheEntry<T> : ICacheEntry<T>
{
    public T Value { get; init; }
    public DateTimeOffset? AbsoluteExpiration { get; init; }
    public TimeSpan? SlidingExpiration { get; init; }
    public long Size { get; init; }
    public IReadOnlyDictionary<string, object> Metadata { get; init; }
}