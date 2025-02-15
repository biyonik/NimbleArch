using System.Text;
using NimbleArch.Core.DependencyInjection;

namespace NimbleArch.Core.Caching;

/// <summary>
/// Implements a structured cache key using struct layout.
/// </summary>
/// <remarks>
/// EN: Provides an efficient cache key implementation using struct layout
/// for optimal memory usage and performance. Supports composite keys.
///
/// TR: Optimal bellek kullanımı ve performans için struct düzeni kullanan
/// verimli bir önbellek anahtarı implementasyonu sağlar. Bileşik anahtarları destekler.
/// </remarks>
public readonly struct StructuredCacheKey : ICacheKey
{
   private readonly byte[] _keySegment;
   private readonly byte[] _partition;
   private readonly long _version;

   public long Version => _version;

   private StructuredCacheKey(byte[] keySegment, byte[] partition, long version)
   {
       _keySegment = keySegment;
       _partition = partition;
       _version = version;
   }

   public ReadOnlySpan<byte> GetKeySegment() => _keySegment;
   public ReadOnlySpan<byte> GetPartition() => _partition;

   /// <summary>
   /// Creates a new structured cache key.
   /// </summary>
   public static StructuredCacheKey Create<T>(T key, string partition = "default", long version = 1)
       where T : notnull
   {
       var keyBytes = key switch
       {
           string s => Encoding.UTF8.GetBytes(s),
           byte[] b => b,
           ISpanFormattable f => FormatSpanFormattable(f),
           _ => SerializeKey(key)
       };

       var partitionBytes = Encoding.UTF8.GetBytes(partition);
       return new StructuredCacheKey(keyBytes, partitionBytes, version);
   }

   private static byte[] FormatSpanFormattable(ISpanFormattable formattable)
   {
       Span<char> charBuffer = stackalloc char[256];
       if (formattable.TryFormat(charBuffer, out var charsWritten, default, null))
       {
           Span<byte> byteBuffer = stackalloc byte[Encoding.UTF8.GetMaxByteCount(charsWritten)];
           var bytesWritten = Encoding.UTF8.GetBytes(charBuffer[..charsWritten], byteBuffer);
           return byteBuffer[..bytesWritten].ToArray();
       }

       return SerializeKey(formattable);
   }

   private static byte[] SerializeKey<T>(T key)
   {
       var serializerType = typeof(ICacheKeySerializer<>).MakeGenericType(typeof(T));
       var serializer = ServiceLocator.GetService(serializerType) as ICacheKeySerializer<T>;
    
       if (serializer == null)
       {
           throw new InvalidOperationException(
               $"No cache key serializer found for type {typeof(T).Name}. " +
               $"Make sure the type is marked with [CacheKeySerializable] attribute.");
       }

       var bufferSize = serializer.GetRequiredBufferSize(key);
       var buffer = new byte[bufferSize];
       var bytesWritten = serializer.SerializeToBytes(key, buffer);
    
       return buffer[..bytesWritten];
   }
}