namespace NimbleArch.Core.Http.Serialization;

/// <summary>
/// Interface implemented by generated serialization code.
/// </summary>
/// <remarks>
/// EN: Provides a contract for high-performance serialization operations.
/// Generated code implements this interface for optimal performance.
///
/// TR: Yüksek performanslı serializasyon operasyonları için bir sözleşme sağlar.
/// Üretilen kod, optimum performans için bu arayüzü uygular.
/// </remarks>
public interface IFastSerializable<T>
{
    /// <summary>
    /// Serializes an object to bytes using a pooled buffer.
    /// </summary>
    int SerializeToBytes(T value, Span<byte> destination);

    /// <summary>
    /// Deserializes an object from bytes.
    /// </summary>
    T DeserializeFromBytes(ReadOnlySpan<byte> source);

    /// <summary>
    /// Gets the required buffer size for serialization.
    /// </summary>
    int GetRequiredBufferSize(T value);
}