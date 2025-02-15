namespace NimbleArch.Core.Http.Transformation.Transformations;

/// <summary>
/// Applies compression to response data.
/// </summary>
public class CompressionTransformation<T> : IResponseTransformation<T>
{
    public bool ShouldApply(ResponseTransformationContext context)
    {
        return context.PerformanceSettings.EnableCompression;
    }

    public Span<byte> Transform(T data, Span<byte> source, ResponseTransformationContext context)
    {
        if (source.Length < context.PerformanceSettings.CompressionThreshold)
            return source;

        var buffer = context.BufferPool.Rent(source.Length);
        try
        {
            return CompressData(source, buffer);
        }
        finally
        {
            context.BufferPool.Return(buffer);
        }
    }

    private Span<byte> CompressData(Span<byte> source, byte[] buffer)
    {
        // Implement high-performance compression
        throw new NotImplementedException();
    }
}