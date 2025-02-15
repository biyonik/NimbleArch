using NimbleArch.Core.Http.Serialization;

namespace NimbleArch.Core.Http.Transformation.Transformations;

/// <summary>
/// Filters response fields based on client requests.
/// </summary>
public class FieldFilterTransformation<T>(IFastSerializable<T> serializer) : IResponseTransformation<T>
{
    public bool ShouldApply(ResponseTransformationContext context)
    {
        return context.RequestedFields?.Count > 0;
    }

    public Span<byte> Transform(T data, Span<byte> source, ResponseTransformationContext context)
    {
        // Field filtering için dynamic IL emission kullanacağız
        var transformer = CreateFieldTransformer(context.RequestedFields);
        var transformedData = transformer.Transform(data);

        var buffer = context.BufferPool.Rent(serializer.GetRequiredBufferSize(transformedData));
        try
        {
            var written = serializer.SerializeToBytes(transformedData, buffer);
            return new Span<byte>(buffer, 0, written);
        }
        finally
        {
            context.BufferPool.Return(buffer);
        }
    }

    private IFieldTransformer<T> CreateFieldTransformer(HashSet<string> fields)
    {
        // Dynamic IL emission ile field transformer oluştur
        throw new NotImplementedException();
    }
}