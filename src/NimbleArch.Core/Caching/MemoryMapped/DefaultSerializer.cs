using System.Text.Json;
using System.Text.Json.Serialization;

namespace NimbleArch.Core.Caching.MemoryMapped;

/// <summary>
/// Default serializer for fallback scenarios.
/// </summary>
/// <remarks>
/// EN: Provides a default serialization mechanism when Source Generator
/// based serialization is not available. Uses a reliable but slower approach.
///
/// TR: Source Generator tabanlı serileştirme mevcut olmadığında varsayılan
/// bir serileştirme mekanizması sağlar. Güvenilir ama daha yavaş bir yaklaşım kullanır.
/// </remarks>
internal static class DefaultSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static byte[] Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, Options);
    }

    public static T Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data, Options);
    }
}