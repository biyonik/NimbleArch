namespace NimbleArch.Core.Caching;

/// <summary>
/// Defines a strongly-typed cache key.
/// </summary>
/// <remarks>
/// EN: Represents a structured cache key that can be efficiently serialized
/// and used for cache operations. Supports versioning and partitioning.
///
/// TR: Verimli şekilde serileştirilebilen ve önbellek işlemleri için
/// kullanılabilen yapılandırılmış bir önbellek anahtarını temsil eder.
/// Versiyonlama ve bölümlendirmeyi destekler.
/// </remarks>
public interface ICacheKey
{
    /// <summary>
    /// Gets the unique segment of the cache key.
    /// </summary>
    ReadOnlySpan<byte> GetKeySegment();

    /// <summary>
    /// Gets the cache partition identifier.
    /// </summary>
    ReadOnlySpan<byte> GetPartition();

    /// <summary>
    /// Gets the version identifier for cache invalidation.
    /// </summary>
    long Version { get; }
}