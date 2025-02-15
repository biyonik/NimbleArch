using System.Buffers;
using NimbleArch.Core.Caching.MemoryMapped;
using NimbleArch.Core.DependencyInjection;
using NimbleArch.Core.Http.Serialization;

namespace NimbleArch.Core.Http;

/// <summary>
/// High-performance, struct-based API response type.
/// </summary>
/// <remarks>
/// EN: Uses value types and memory pooling for optimal performance.
/// Supports conditional data loading and response transformation.
///
/// TR: Optimum performans için değer tipleri ve bellek havuzlama kullanır.
/// Koşullu veri yükleme ve response dönüşümünü destekler.
/// </remarks>
public readonly struct ApiResponse<T>
{
   private readonly ArrayPool<byte> _arrayPool;
   private readonly byte[] _rentedArray;

   /// <summary>
   /// Gets whether the operation was successful.
   /// </summary>
   public bool IsSuccess { get; }

   /// <summary>
   /// Gets the status code of the response.
   /// </summary>
   public int StatusCode { get; }

   /// <summary>
   /// Gets the data wrapped in a memory efficient structure.
   /// </summary>
   public ReadOnlyMemory<byte> Data { get; }

   /// <summary>
   /// Gets error details if any.
   /// </summary>
   public ApiError? Error { get; }

   private ApiResponse(
       bool isSuccess,
       int statusCode,
       ReadOnlyMemory<byte> data,
       ApiError? error = null)
   {
       _arrayPool = ArrayPool<byte>.Shared;
       _rentedArray = null;

       IsSuccess = isSuccess;
       StatusCode = statusCode;
       Data = data;
       Error = error;
   }

   public static ApiResponse<T> Success(T data, int statusCode = 200)
   {
       var serializedData = SerializeToBytes(data);
       return new ApiResponse<T>(true, statusCode, serializedData);
   }

   public static ApiResponse<T> Failure(ApiError error, int statusCode = 400)
   {
       return new ApiResponse<T>(false, statusCode, ReadOnlyMemory<byte>.Empty, error);
   }

   /// <summary>
    /// Serializes data to bytes using high-performance serialization.
    /// </summary>
    /// <remarks>
    /// EN: Uses Source Generator based serialization for optimal performance.
    /// Falls back to a default serializer if no generated serializer is found.
    ///
    /// TR: Optimal performans için Source Generator tabanlı serileştirme kullanır.
    /// Üretilmiş serileştirici bulunamazsa varsayılan serileştiriciye döner.
    /// </remarks>
    private static ReadOnlyMemory<byte> SerializeToBytes(T data)
    {
        var serializerType = typeof(IFastSerializable<>).MakeGenericType(typeof(T));
        var serializer = ServiceLocator.GetService(serializerType) as IFastSerializable<T>;

        if (serializer != null)
        {
            var bufferSize = serializer.GetRequiredBufferSize(data);
            var rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                var bytesWritten = serializer.SerializeToBytes(data, rentedBuffer);
                var result = new Memory<byte>(new byte[bytesWritten]);
                rentedBuffer.AsSpan(0, bytesWritten).CopyTo(result.Span);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }

        // Fallback serializer kullanımı
        var fallbackBytes = DefaultSerializer.Serialize(data);
        return new ReadOnlyMemory<byte>(fallbackBytes);
    }

    /// <summary>
    /// Gets the data from the response using high-performance deserialization.
    /// </summary>
    /// <remarks>
    /// EN: Uses Source Generator based deserialization for optimal performance.
    /// Falls back to a default deserializer if no generated deserializer is found.
    ///
    /// TR: Optimal performans için Source Generator tabanlı deserileştirme kullanır.
    /// Üretilmiş deserileştirici bulunamazsa varsayılan deserileştiriciye döner.
    /// </remarks>
    public T GetData()
    {
        if (!IsSuccess || Data.IsEmpty)
            throw new InvalidOperationException("Cannot get data from an unsuccessful or empty response");

        var serializerType = typeof(IFastSerializable<>).MakeGenericType(typeof(T));
        var serializer = ServiceLocator.GetService(serializerType) as IFastSerializable<T>;

        if (serializer != null)
        {
            return serializer.DeserializeFromBytes(Data.Span);
        }

        // Fallback deserializer kullanımı
        return DefaultSerializer.Deserialize<T>(Data.ToArray());
    }
}