namespace NimbleArch.Core.Caching.Exceptions;

/// <summary>
/// Base exception for all cache-related errors.
/// </summary>
/// <remarks>
/// EN: Serves as the base class for all cache-specific exceptions.
/// Provides additional context and cache operation details.
///
/// TR: Tüm önbellek-spesifik istisnalar için temel sınıf görevi görür.
/// Ek bağlam ve önbellek işlem detayları sağlar.
/// </remarks>
public class CacheException : Exception
{
    public string CacheOperation { get; }
    public ICacheKey CacheKey { get; }

    public CacheException(string message, string operation, ICacheKey key = null) 
        : base(message)
    {
        CacheOperation = operation;
        CacheKey = key;
    }

    public CacheException(string message, string operation, Exception innerException, ICacheKey key = null) 
        : base(message, innerException)
    {
        CacheOperation = operation;
        CacheKey = key;
    }
}