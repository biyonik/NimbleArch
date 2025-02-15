namespace NimbleArch.Core.Caching.Exceptions;

/// <summary>
/// Exception thrown when cache capacity is exceeded.
/// </summary>
public class CacheCapacityException(
    string message,
    string operation,
    long requestedSize,
    long availableSize,
    ICacheKey key = null)
    : CacheException(message, operation, key)
{
    public long RequestedSize { get; } = requestedSize;
    public long AvailableSize { get; } = availableSize;
}