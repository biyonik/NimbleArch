namespace NimbleArch.Core.Caching;

/// <summary>
/// Defines cache operation options.
/// </summary>
/// <remarks>
/// EN: Provides configuration options for cache operations including expiration,
/// priority, and size limits. Uses struct for performance optimization.
///
/// TR: Sona erme, öncelik ve boyut limitleri dahil önbellek işlemleri için
/// yapılandırma seçenekleri sağlar. Performans optimizasyonu için struct kullanır.
/// </remarks>
public readonly struct CacheOptions
{
   /// <summary>
   /// Gets the absolute expiration time.
   /// </summary>
   public DateTimeOffset? AbsoluteExpiration { get; init; }

   /// <summary>
   /// Gets the sliding expiration interval.
   /// </summary>
   public TimeSpan? SlidingExpiration { get; init; }

   /// <summary>
   /// Gets the cache priority.
   /// </summary>
   public CachePriority Priority { get; init; }

   /// <summary>
   /// Gets the size limit in bytes.
   /// </summary>
   public long? SizeLimit { get; init; }

   /// <summary>
   /// Gets whether to compress the cached data.
   /// </summary>
   public bool EnableCompression { get; init; }

   /// <summary>
   /// Gets custom metadata for the cache entry.
   /// </summary>
   public IReadOnlyDictionary<string, object> Metadata { get; init; }

   private CacheOptions(
       DateTimeOffset? absoluteExpiration,
       TimeSpan? slidingExpiration,
       CachePriority priority,
       long? sizeLimit,
       bool enableCompression,
       IReadOnlyDictionary<string, object> metadata)
   {
       AbsoluteExpiration = absoluteExpiration;
       SlidingExpiration = slidingExpiration;
       Priority = priority;
       SizeLimit = sizeLimit;
       EnableCompression = enableCompression;
       Metadata = metadata ?? new Dictionary<string, object>();
   }

   /// <summary>
   /// Creates a new instance of cache options.
   /// </summary>
   public static CacheOptions Create(Action<Builder> configure)
   {
       var builder = new Builder();
       configure(builder);
       return builder.Build();
   }

   public class Builder
   {
       private DateTimeOffset? _absoluteExpiration;
       private TimeSpan? _slidingExpiration;
       private CachePriority _priority = CachePriority.Normal;
       private long? _sizeLimit;
       private bool _enableCompression;
       private readonly Dictionary<string, object> _metadata = new();

       public Builder WithAbsoluteExpiration(DateTimeOffset expiration)
       {
           _absoluteExpiration = expiration;
           return this;
       }

       public Builder WithSlidingExpiration(TimeSpan expiration)
       {
           _slidingExpiration = expiration;
           return this;
       }

       public Builder WithPriority(CachePriority priority)
       {
           _priority = priority;
           return this;
       }

       public Builder WithSizeLimit(long sizeLimit)
       {
           _sizeLimit = sizeLimit;
           return this;
       }

       public Builder WithCompression(bool enable = true)
       {
           _enableCompression = enable;
           return this;
       }

       public Builder WithMetadata(string key, object value)
       {
           _metadata[key] = value;
           return this;
       }

       internal CacheOptions Build()
       {
           return new CacheOptions(
               _absoluteExpiration,
               _slidingExpiration,
               _priority,
               _sizeLimit,
               _enableCompression,
               _metadata);
       }
   }
}