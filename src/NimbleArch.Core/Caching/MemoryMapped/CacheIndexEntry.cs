namespace NimbleArch.Core.Caching.MemoryMapped;

/// <summary>
/// Represents an entry in the cache index.
/// </summary>
/// <remarks>
/// EN: Contains metadata about a cached item including its location, size,
/// expiration settings, and other properties. Used for efficient item lookup
/// and cache management.
///
/// TR: Önbellekteki bir öğenin konumu, boyutu, sona erme ayarları ve diğer
/// özellikleri dahil meta verilerini içerir. Verimli öğe arama ve önbellek
/// yönetimi için kullanılır.
/// </remarks>
public class CacheIndexEntry
{
    /// <summary>
    /// Gets or sets the position in the memory-mapped file.
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// Gets or sets the size of the cached data in bytes.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets when the entry was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the entry was last accessed.
    /// </summary>
    public DateTimeOffset LastAccessed { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration time.
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration interval.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets whether the data is compressed.
    /// </summary>
    public bool IsCompressed { get; set; }

    /// <summary>
    /// Gets or sets the cache priority.
    /// </summary>
    public CachePriority Priority { get; set; }

    /// <summary>
    /// Gets or sets the entry metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; set; }
}
