namespace NimbleArch.Core.Caching.Exceptions;

/// <summary>
/// Exception thrown when serialization/deserialization fails.
/// </summary>
public class CacheSerializationException(
    string message,
    string operation,
    Type valueType,
    Exception innerException = null,
    ICacheKey key = null)
    : CacheException(message, operation, innerException, key)
{
    public Type ValueType { get; } = valueType;
}