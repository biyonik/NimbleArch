namespace NimbleArch.Core.Caching;

/// <summary>
/// Interface for cache key serialization.
/// </summary>
public interface ICacheKeySerializer<T>
{
    /// <summary>
    /// Serializes a key to bytes using a provided buffer.
    /// </summary>
    int SerializeToBytes(T key, Span<byte> destination);

    /// <summary>
    /// Gets the required buffer size for serialization.
    /// </summary>
    int GetRequiredBufferSize(T key);
}