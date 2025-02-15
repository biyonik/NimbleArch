using System.Buffers;

namespace NimbleArch.Core.Http.Transformation;

/// <summary>
/// Context for response transformation operations.
/// </summary>
/// <remarks>
/// EN: Contains contextual information for response transformations, including
/// client preferences, environment settings, and performance metrics.
///
/// TR: Response dönüşümleri için bağlamsal bilgileri içerir. İstemci tercihleri,
/// ortam ayarları ve performans metrikleri dahildir.
/// </remarks>
public class ResponseTransformationContext(
    HashSet<string> requestedFields,
    IReadOnlyDictionary<string, string> clientPreferences,
    ResponsePerformanceSettings performanceSettings)
{
    /// <summary>
    /// Gets the requested fields to include.
    /// </summary>
    public HashSet<string> RequestedFields { get; } = requestedFields;

    /// <summary>
    /// Gets client-specific preferences.
    /// </summary>
    public IReadOnlyDictionary<string, string> ClientPreferences { get; } = clientPreferences;

    /// <summary>
    /// Gets performance settings.
    /// </summary>
    public ResponsePerformanceSettings PerformanceSettings { get; } = performanceSettings;

    /// <summary>
    /// Gets memory pool for buffer operations.
    /// </summary>
    public ArrayPool<byte> BufferPool { get; } = ArrayPool<byte>.Shared;
}