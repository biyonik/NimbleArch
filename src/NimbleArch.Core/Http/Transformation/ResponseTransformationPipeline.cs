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

    private Span<byte> GetCurrentSpan(T data, byte[] initialBuffer)
    {
        return new Span<byte>(initialBuffer, 0, 
            serializer.SerializeToBytes(data, initialBuffer));
    }

    public async Task TransformAndWriteAsync(
        T data,
        Stream outputStream,
        ResponseTransformationContext context,
        CancellationToken cancellationToken = default)
    {
        var initialBuffer = context.BufferPool.Rent(serializer.GetRequiredBufferSize(data));
        var currentSpan = GetCurrentSpan(data: data, initialBuffer);
        try
        {
            foreach (var transformation in _transformations)
            {
                if (transformation.ShouldApply(context))
                {
                    currentSpan = transformation.Transform(data, currentSpan, context);
                }
            }

            await outputStream.WriteAsync(currentSpan.ToArray(), cancellationToken);
        }
        finally
        {
            context.BufferPool.Return(initialBuffer);
        }
    }
}