using System.Buffers;
using System.Buffers.Binary;
using K4os.Compression.LZ4;
using NimbleArch.Core.Http.Serialization;

namespace NimbleArch.Core.Http.Transformation.Transformations;

/// <summary>
/// Applies compression to response data.
/// </summary>
public class CompressionTransformation<T> : IResponseTransformation<T>
{
    public bool ShouldApply(ResponseTransformationContext context)
    {
        return context.PerformanceSettings.EnableCompression;
    }

    public byte[] Transform(T data, ReadOnlySpan<byte> source, ResponseTransformationContext context)
    {
        if (source.Length < context.PerformanceSettings.CompressionThreshold)
            return source.ToArray();

        var buffer = context.BufferPool.Rent(source.Length);
        try
        {
            // LZ4 için gereken maksimum buffer boyutunu hesapla
            int maxCompressedLength = source.Length + (source.Length / 255) + 16;
           
            // Buffer yeterli değilse yeni bir buffer al
            if (buffer.Length < maxCompressedLength + 4) // +4 boyut bilgisi için
            {
                // Buffer havuzundan yeterli boyutta buffer al
                buffer = context.BufferPool.Rent(maxCompressedLength + 4);
            }

            // Sıkıştırma işlemini gerçekleştir
            int compressedLength = LZ4Codec.Encode(
                source,
                buffer.AsSpan(4, maxCompressedLength), // İlk 4 byte için yer bırak
                LZ4Level.L00_FAST
            );

            // Sıkıştırılmış verinin boyutunu başa yaz
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(0, 4), compressedLength);

            // Sadece kullanılan kısmı kopyala ve döndür
            var result = new byte[compressedLength + 4];
            buffer.AsSpan(0, compressedLength + 4).CopyTo(result);
            return result;
        }
        finally
        {
            context.BufferPool.Return(buffer);
        }
    }

    /// <summary>
    /// Compresses data using high-performance LZ4 algorithm.
    /// </summary>
    /// <remarks>
    /// EN: Uses LZ4 compression algorithm optimized for speed and compression ratio.
    /// Implements buffer pooling for efficient memory usage.
    ///
    /// TR: Hız ve sıkıştırma oranı için optimize edilmiş LZ4 sıkıştırma algoritmasını kullanır.
    /// Verimli bellek kullanımı için buffer havuzlama uygular.
    /// </remarks>
    private Span<byte> CompressData(Span<byte> source, byte[] buffer)
    {
        // LZ4 için gereken maksimum buffer boyutunu hesapla
        int maxCompressedLength = source.Length + (source.Length / 255) + 16;
    
        // Buffer yeterli değilse yeni bir buffer al
        if (buffer.Length < maxCompressedLength + 4)
        {
            // Buffer havuzundan yeterli boyutta buffer al
            buffer = ArrayPool<byte>.Shared.Rent(maxCompressedLength + 4);
        }

        try
        {
            // Sıkıştırma işlemini gerçekleştir
            int compressedLength = LZ4Codec.Encode(
                source, 
                buffer.AsSpan(4, maxCompressedLength) // Maksimum sıkıştırma seviyesi
            );

            // Sıkıştırılmış verinin boyutunu başa yaz
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(0, 4), compressedLength);

            // Sadece kullanılan kısmı döndür
            return buffer.AsSpan(0, compressedLength + 4);
        }
        catch (Exception ex)
        {
            throw new CompressionException(
                "Failed to compress data",
                ex,
                source.Length,
                maxCompressedLength);
        }
    }
}