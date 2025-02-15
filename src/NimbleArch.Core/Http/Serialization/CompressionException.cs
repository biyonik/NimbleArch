namespace NimbleArch.Core.Http.Serialization;

/// <summary>
/// Custom exception for compression operations.
/// </summary>
public class CompressionException(
    string message,
    Exception innerException,
    int sourceLength,
    int targetLength)
    : Exception(message, innerException)
{
    public int SourceLength { get; } = sourceLength;
    public int TargetLength { get; } = targetLength;
}
