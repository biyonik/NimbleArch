namespace NimbleArch.Core.Http.Attributes;

/// <summary>
/// Marks a type for fast serialization code generation.
/// </summary>
/// <remarks>
/// EN: Indicates that a type should use compile-time generated serialization
/// code instead of reflection-based serialization. Provides significant
/// performance improvements.
///
/// TR: Bir tipin reflection tabanlı serializasyon yerine derleme zamanında
/// üretilen serializasyon kodunu kullanması gerektiğini belirtir. Önemli
/// performans iyileştirmeleri sağlar.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class FastSerializableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to include null values in serialization.
    /// </summary>
    public bool IncludeNulls { get; set; }

    /// <summary>
    /// Gets or sets the buffer size hint for serialization.
    /// </summary>
    public int BufferSizeHint { get; set; }
}