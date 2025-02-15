namespace NimbleArch.Core.Caching;

/// <summary>
/// Represents a cache entry with metadata.
/// </summary>
/// <remarks>
/// EN: Provides cache entry information including expiration, size metrics,
/// and custom metadata. Optimized for memory efficiency.
///
/// TR: Sona erme, boyut metrikleri ve özel metadatalar dahil önbellek giriş
/// bilgilerini sağlar. Bellek verimliliği için optimize edilmiştir.
/// </remarks>
public interface ICacheEntry<T>
{
    /// <summary>
    /// Gets the cached value.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Gets the absolute expiration time.
    /// </summary>
    DateTimeOffset? AbsoluteExpiration { get; }

    /// <summary>
    /// Gets the sliding expiration interval.
    /// </summary>
    TimeSpan? SlidingExpiration { get; }

    /// <summary>
    /// Gets the size of the entry in bytes.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Gets the entry metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}