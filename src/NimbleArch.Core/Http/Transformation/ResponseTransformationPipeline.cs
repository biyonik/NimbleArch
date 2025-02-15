using NimbleArch.Core.Http.Serialization;

namespace NimbleArch.Core.Http.Transformation;

/// <summary>
/// Pipeline for response transformations.
/// </summary>
public class ResponseTransformationPipeline<T>(IFastSerializable<T> serializer)
{
    private readonly List<IResponseTransformation<T>> _transformations = new();

    public ResponseTransformationPipeline<T> AddTransformation(IResponseTransformation<T> transformation)
    {
        _transformations.Add(transformation);
        return this;
    }

    public async Task TransformAndWriteAsync(
        T data,
        Stream outputStream,
        ResponseTransformationContext context,
        CancellationToken cancellationToken = default)
    {
        var initialBuffer = context.BufferPool.Rent(serializer.GetRequiredBufferSize(data));
        try
        {
            var currentMemory = GetCurrentMemory(data, initialBuffer);

            foreach (var transformation in _transformations)
            {
                if (transformation.ShouldApply(context))
                {
                    // Span'i Memory'ye dönüştür
                    currentMemory = transformation
                        .Transform(data, currentMemory.Span, context)
                        .ToArray()
                        .AsMemory();
                }
            }

            await outputStream.WriteAsync(currentMemory, cancellationToken);
        }
        finally
        {
            context.BufferPool.Return(initialBuffer);
        }
    }

    private Memory<byte> GetCurrentMemory(T data, byte[] initialBuffer)
    {
        var bytesWritten = serializer.SerializeToBytes(data, initialBuffer);
        return new Memory<byte>(initialBuffer, 0, bytesWritten);
    }
}