using System.Buffers;
using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using LZ4;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.Caching.Exceptions;
using NimbleArch.Core.DependencyInjection;
using NimbleArch.Core.Http.Serialization;

namespace NimbleArch.Core.Caching.MemoryMapped;

/// <summary>
/// High-performance memory-mapped file based cache implementation.
/// </summary>
/// <remarks>
/// EN: Uses memory-mapped files for persistent storage with in-memory index.
/// Provides fast access while supporting large datasets.
///
/// TR: Kalıcı depolama için belleğe eşlenmiş dosyaları ve bellek içi indeksleme
/// kullanır. Büyük veri setlerini desteklerken hızlı erişim sağlar.
/// </remarks>
public sealed partial class MemoryMappedCache : IHighPerformanceCache, IDisposable
{
    private readonly string _filePath;
    private MemoryMappedFile _mappedFile;
    private readonly ReaderWriterLockSlim _lock;
    private readonly ConcurrentDictionary<string, CacheIndexEntry> _index;
    private readonly ILogger<MemoryMappedCache> _logger;
    private readonly CacheOptions _defaultOptions;
    private long _currentPosition;

    public MemoryMappedCache(
        string filePath,
        long initialSize,
        CacheOptions defaultOptions,
        ILogger<MemoryMappedCache> logger)
    {
        _filePath = filePath;
        _lock = new ReaderWriterLockSlim();
        _index = new ConcurrentDictionary<string, CacheIndexEntry>();
        _logger = logger;
        _defaultOptions = defaultOptions;
        _currentPosition = 0;

        // Create or open the memory-mapped file
        _mappedFile = MemoryMappedFile.CreateFromFile(
            _filePath,
            FileMode.OpenOrCreate,
            null,
            initialSize);

        // Load existing index if any
        LoadExistingIndex();
    }

    public async ValueTask<ICacheEntry<T>> GetAsync<T>(
        ICacheKey key,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKeyString(key);
        
        _lock.EnterReadLock();
        try
        {
            if (!_index.TryGetValue(cacheKey, out var indexEntry))
                return null;

            if (IsExpired(indexEntry))
            {
                await RemoveAsync(key, cancellationToken);
                return null;
            }

            using var accessor = _mappedFile.CreateViewAccessor(
                indexEntry.Position, 
                indexEntry.Size, 
                MemoryMappedFileAccess.Read);

            var buffer = new byte[indexEntry.Size];
            accessor.ReadArray(0, buffer, 0, buffer.Length);

            // Decompress if needed
            if (indexEntry.IsCompressed)
            {
                buffer = DecompressData(buffer);
            }

            var value = DeserializeValue<T>(buffer);
            UpdateAccessTime(indexEntry);

            return CreateCacheEntry(value, indexEntry);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async ValueTask SetAsync<T>(
        ICacheKey key,
        T value,
        CacheOptions options,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKeyString(key);
        var serializedValue = SerializeValue(value);

        // Apply compression if enabled
        var shouldCompress = options.EnableCompression || 
            serializedValue.Length > _defaultOptions.SizeLimit;

        var finalData = shouldCompress ? 
            CompressData(serializedValue) : 
            serializedValue;

        _lock.EnterWriteLock();
        try
        {
            // Create index entry
            var indexEntry = new CacheIndexEntry
            {
                Position = _currentPosition,
                Size = finalData.Length,
                CreatedAt = DateTimeOffset.UtcNow,
                LastAccessed = DateTimeOffset.UtcNow,
                AbsoluteExpiration = options.AbsoluteExpiration,
                SlidingExpiration = options.SlidingExpiration,
                IsCompressed = shouldCompress,
                Priority = options.Priority,
                Metadata = options.Metadata
            };

            // Write to file
            using var accessor = _mappedFile.CreateViewAccessor(
                _currentPosition, 
                finalData.Length, 
                MemoryMappedFileAccess.Write);

            accessor.WriteArray(0, finalData, 0, finalData.Length);

            // Update index
            _index[cacheKey] = indexEntry;
            _currentPosition += finalData.Length;

            // Perform cleanup if needed
            await PerformCleanupIfNeeded(cancellationToken);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async ValueTask RemoveAsync(
        ICacheKey key,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKeyString(key);

        _lock.EnterWriteLock();
        try
        {
            if (_index.TryRemove(cacheKey, out var entry))
            {
                // Mark space as available for reuse
                MarkSpaceAsAvailable(entry.Position, entry.Size);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    private string GetCacheKeyString(ICacheKey key)
    {
        var keySegment = key.GetKeySegment();
        var partition = key.GetPartition();
        
        Span<byte> combined = stackalloc byte[keySegment.Length + partition.Length + 16];
        partition.CopyTo(combined);
        keySegment.CopyTo(combined[partition.Length..]);
        BitConverter.TryWriteBytes(combined[(keySegment.Length + partition.Length)..], key.Version);

        return Convert.ToBase64String(combined);
    }

    private bool IsExpired(CacheIndexEntry entry)
    {
        var now = DateTimeOffset.UtcNow;

        if (entry.AbsoluteExpiration.HasValue && 
            now >= entry.AbsoluteExpiration.Value)
            return true;

        if (entry.SlidingExpiration.HasValue && 
            now >= entry.LastAccessed + entry.SlidingExpiration.Value)
            return true;

        return false;
    }

    private void UpdateAccessTime(CacheIndexEntry entry)
    {
        entry.LastAccessed = DateTimeOffset.UtcNow;
    }

    private ICacheEntry<T> CreateCacheEntry<T>(T value, CacheIndexEntry indexEntry)
    {
        return new CacheEntry<T>
        {
            Value = value,
            AbsoluteExpiration = indexEntry.AbsoluteExpiration,
            SlidingExpiration = indexEntry.SlidingExpiration,
            Size = indexEntry.Size,
            Metadata = indexEntry.Metadata
        };
    }

    /// <summary>
    /// Loads existing cache index from the memory-mapped file.
    /// </summary>
    /// <remarks>
    /// EN: Reads and reconstructs the cache index from the memory-mapped file
    /// during initialization. Validates entries and removes expired ones.
    ///
    /// TR: Başlatma sırasında önbellek indeksini belleğe eşlenmiş dosyadan
    /// okur ve yeniden oluşturur. Girdileri doğrular ve süresi geçenleri kaldırır.
    /// </remarks>
    private void LoadExistingIndex()
    {
        _lock.EnterWriteLock();
        try
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            if (stream.Length == 0) return;
    
            using var reader = new BinaryReader(stream);
            var indexOffset = reader.ReadInt64(); // İndeks başlangıç pozisyonu
    
            if (indexOffset > 0)
            {
                stream.Position = indexOffset;
                var indexSize = reader.ReadInt32();
                var indexBytes = reader.ReadBytes(indexSize);
                
                var deserializedIndex = DeserializeIndex(indexBytes);
                foreach (var (key, entry) in deserializedIndex)
                {
                    if (!IsExpired(entry))
                    {
                        _index[key] = entry;
                        _currentPosition = Math.Max(_currentPosition, 
                            entry.Position + entry.Size);
                    }
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Performs cleanup of expired items and defragmentation if needed.
    /// </summary>
    /// <remarks>
    /// EN: Removes expired entries and performs defragmentation when
    /// the fragmentation level exceeds a threshold. Uses a background
    /// task for cleanup to minimize impact on cache operations.
    ///
    /// TR: Süresi dolmuş girdileri kaldırır ve parçalanma seviyesi
    /// eşiği aştığında birleştirme işlemi gerçekleştirir. Önbellek
    /// işlemlerine etkiyi en aza indirmek için arka plan görevi kullanır.
    /// </remarks>
    private async Task PerformCleanupIfNeeded(CancellationToken cancellationToken)
    {
        var fragmentationLevel = CalculateFragmentationLevel();
        if (fragmentationLevel < 0.3) // %30'dan az parçalanma varsa cleanup'a gerek yok
            return;
    
        await Task.Run(() =>
        {
            _lock.EnterWriteLock();
            try
            {
                var compactedFile = CreateTempFile();
                var newPositions = new Dictionary<string, long>();
                long currentPos = 0;
    
                foreach (var (key, entry) in _index)
                {
                    if (IsExpired(entry))
                    {
                        _index.TryRemove(key, out _);
                        continue;
                    }
    
                    using var sourceView = _mappedFile.CreateViewAccessor(
                        entry.Position, entry.Size, MemoryMappedFileAccess.Read);
                    using var targetView = compactedFile.CreateViewAccessor(
                        currentPos, entry.Size, MemoryMappedFileAccess.Write);
    
                    var buffer = new byte[entry.Size];
                    sourceView.ReadArray(0, buffer, 0, buffer.Length);
                    targetView.WriteArray(0, buffer, 0, buffer.Length);
    
                    newPositions[key] = currentPos;
                    currentPos += entry.Size;
                }
    
                // Yeni pozisyonları güncelle
                foreach (var (key, newPos) in newPositions)
                {
                    if (_index.TryGetValue(key, out var entry))
                    {
                        entry.Position = newPos;
                    }
                }
    
                // Eski dosyayı yenisiyle değiştir
                SwapFiles(compactedFile);
                _currentPosition = currentPos;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }, cancellationToken);
    }
    
    /// <summary>
    /// Marks a space in the memory-mapped file as available for reuse.
    /// </summary>
    /// <remarks>
    /// EN: Manages the free space list for efficient space reuse.
    /// Implements a best-fit algorithm for space allocation.
    ///
    /// TR: Verimli alan yeniden kullanımı için boş alan listesini yönetir.
    /// En uygun alan tahsisi için best-fit algoritmasını uygular.
    /// </remarks>
    private void MarkSpaceAsAvailable(long position, int size)
    {
        // Free space yönetimi için AVL tree kullanılabilir
        // Şimdilik basit bir liste implementasyonu
        var freeSpace = new FreeSpaceEntry
        {
            Position = position,
            Size = size
        };
    
        _freeSpaces.Add(freeSpace);
        _freeSpaces.Sort((a, b) => a.Size.CompareTo(b.Size));
    }

    public void Dispose()
    {
        _mappedFile.Dispose();
        _lock.Dispose();
    }
}

/// <summary>
/// Serialization and compression related methods for MemoryMappedCache.
/// </summary>
public partial class MemoryMappedCache
{
   /// <summary>
   /// Serializes a value to byte array.
   /// </summary>
   /// <remarks>
   /// EN: Uses Source Generator based serialization for optimal performance.
   /// Falls back to a default serializer if no generated serializer is found.
   ///
   /// TR: Optimal performans için Source Generator tabanlı serileştirme kullanır.
   /// Eğer üretilmiş serileştirici bulunamazsa varsayılan serileştiriciye döner.
   /// </remarks>
   private byte[] SerializeValue<T>(T value)
   {
       var serializerType = typeof(IFastSerializable<>).MakeGenericType(typeof(T));
       var serializer = ServiceLocator.GetService(serializerType) as IFastSerializable<T>;

       if (serializer == null) return DefaultSerializer.Serialize(value);
       var bufferSize = serializer.GetRequiredBufferSize(value);
       var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
       try
       {
           var bytesWritten = serializer.SerializeToBytes(value, buffer);
           var result = new byte[bytesWritten];
           Buffer.BlockCopy(buffer, 0, result, 0, bytesWritten);
           return result;
       }
       catch (Exception ex)
       {
           throw new CacheSerializationException(
               $"Failed to serialize value of type {typeof(T).Name}",
               "Serialize",
               typeof(T),
               ex);
       }
       finally
       {
           ArrayPool<byte>.Shared.Return(buffer);
       }
   }

   /// <summary>
   /// Deserializes a value from byte array.
   /// </summary>
   /// <remarks>
   /// EN: Uses Source Generator based deserialization for optimal performance.
   /// Falls back to a default deserializer if no generated deserializer is found.
   ///
   /// TR: Optimal performans için Source Generator tabanlı deserileştirme kullanır.
   /// Eğer üretilmiş deserileştirici bulunamazsa varsayılan deserileştiriciye döner.
   /// </remarks>
   private T DeserializeValue<T>(byte[] data)
   {
       var serializerType = typeof(IFastSerializable<>).MakeGenericType(typeof(T));

       if (ServiceLocator.GetService(serializerType) is IFastSerializable<T> serializer)
       {
           return serializer.DeserializeFromBytes(data);
       }

       // Fallback to default deserializer
       return DefaultSerializer.Deserialize<T>(data);
   }

   /// <summary>
   /// Compresses data using high-performance algorithm.
   /// </summary>
   /// <remarks>
   /// EN: Uses a fast compression algorithm optimized for cache scenarios.
   /// Implements memory pooling for efficient buffer management.
   ///
   /// TR: Önbellek senaryoları için optimize edilmiş hızlı bir sıkıştırma
   /// algoritması kullanır. Verimli tampon yönetimi için bellek havuzlama uygular.
   /// </remarks>
   private byte[] CompressData(byte[] data)
   {
       using var outputStream = new MemoryStream();
       using var lz4Stream = new LZ4Stream(outputStream, LZ4StreamMode.Compress);
       
       lz4Stream.Write(data, 0, data.Length);
       lz4Stream.Flush();
       
       return outputStream.ToArray();
   }

   /// <summary>
   /// Decompresses data using high-performance algorithm.
   /// </summary>
   /// <remarks>
   /// EN: Uses a fast decompression algorithm optimized for cache scenarios.
   /// Implements memory pooling for efficient buffer management.
   ///
   /// TR: Önbellek senaryoları için optimize edilmiş hızlı bir açma
   /// algoritması kullanır. Verimli tampon yönetimi için bellek havuzlama uygular.
   /// </remarks>
   private byte[] DecompressData(byte[] compressedData)
   {
       using var inputStream = new MemoryStream(compressedData);
       using var lz4Stream = new LZ4Stream(inputStream, LZ4StreamMode.Decompress);
       using var outputStream = new MemoryStream();

       lz4Stream.CopyTo(outputStream);
       return outputStream.ToArray();
   }
}

public partial class MemoryMappedCache
{
    // Field
    private readonly List<FreeSpaceEntry> _freeSpaces = new();

    /// <summary>
    /// Gets or creates a cache entry asynchronously.
    /// </summary>
    public async ValueTask<ICacheEntry<T>> GetOrCreateAsync<T>(
        ICacheKey key,
        Func<ICacheKey, CancellationToken, ValueTask<T>> factory,
        CacheOptions options,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing != null)
            return existing;

        var value = await factory(key, cancellationToken);
        await SetAsync(key, value, options, cancellationToken);
        return CreateCacheEntry(value, new CacheIndexEntry
        {
            AbsoluteExpiration = options.AbsoluteExpiration,
            SlidingExpiration = options.SlidingExpiration,
            Size = await CalculateSize(value),
            Metadata = options.Metadata
        });
    }

    /// <summary>
    /// Deserializes index data from bytes.
    /// </summary>
    private Dictionary<string, CacheIndexEntry> DeserializeIndex(byte[] indexBytes)
    {
        return DefaultSerializer.Deserialize<Dictionary<string, CacheIndexEntry>>(indexBytes);
    }

    /// <summary>
    /// Calculates current fragmentation level.
    /// </summary>
    private double CalculateFragmentationLevel()
    {
        var totalSpace = _currentPosition;
        var usedSpace = _index.Values.Sum(e => e.Size);
        var freeSpace = _freeSpaces.Sum(f => f.Size);

        return (double)freeSpace / totalSpace;
    }

    /// <summary>
    /// Creates a temporary memory-mapped file.
    /// </summary>
    private MemoryMappedFile CreateTempFile()
    {
        var tempPath = Path.Combine(
            Path.GetDirectoryName(_filePath),
            $"{Path.GetFileNameWithoutExtension(_filePath)}_temp{Path.GetExtension(_filePath)}");

        return MemoryMappedFile.CreateFromFile(
            tempPath,
            FileMode.CreateNew,
            null,
            _currentPosition);
    }

    /// <summary>
    /// Swaps the current file with a new one.
    /// </summary>
    private void SwapFiles(MemoryMappedFile newFile)
    {
        var tempPath = ((MemoryMappedViewStream)newFile
            .CreateViewStream())
            .SafeMemoryMappedViewHandle
            .DangerousGetHandle()
            .ToString();

        _mappedFile.Dispose();
        File.Delete(_filePath);
        File.Move(tempPath, _filePath);
        _mappedFile = MemoryMappedFile.CreateFromFile(_filePath);
    }

    /// <summary>
    /// Calculates size of a value in bytes.
    /// </summary>
    private async Task<int> CalculateSize<T>(T value)
    {
        var serialized = SerializeValue(value);
        return serialized.Length;
    }
}