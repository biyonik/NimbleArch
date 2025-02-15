namespace NimbleArch.Core.Http.Transformation;

/// <summary>
/// Performance settings for response handling.
/// </summary>
public readonly struct ResponsePerformanceSettings
{
    /// <summary>
    /// Gets whether to enable compression.
    /// </summary>
    public bool EnableCompression { get; init; }

    /// <summary>
    /// Gets the minimum size for compression.
    /// </summary>
    public int CompressionThreshold { get; init; }

    /// <summary>
    /// Gets buffer size for transformations.
    /// </summary>
    public int BufferSize { get; init; }
}