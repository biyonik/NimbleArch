namespace NimbleArch.Core.Caching.Exceptions;

/// <summary>
/// Exception thrown when memory-mapped file operations fail.
/// </summary>
public class CacheFileException(
    string message,
    string operation,
    string filePath,
    Exception innerException = null,
    ICacheKey key = null)
    : CacheException(message, operation, innerException, key)
{
    public string FilePath { get; } = filePath;
}