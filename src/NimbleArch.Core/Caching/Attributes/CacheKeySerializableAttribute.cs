namespace NimbleArch.Core.Caching.Attributes;

/// <summary>
/// Marks a type for cache key serialization code generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CacheKeySerializableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the buffer size hint for serialization.
    /// </summary>
    public int BufferSizeHint { get; set; } = 256;
}