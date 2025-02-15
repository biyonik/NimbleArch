using NimbleArch.Core.Http.Serialization;
using NimbleArch.Core.Http.Transformation.Implementations;

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

    public byte[] Transform(T data, ReadOnlySpan<byte> source, ResponseTransformationContext context)
    {
        // Field filtering için dynamic IL emission kullanıyoruz
        var transformer = CreateFieldTransformer(context.RequestedFields);
        var transformedData = transformer.Transform(data);

        var bufferSize = serializer.GetRequiredBufferSize(transformedData);
        var buffer = context.BufferPool.Rent(bufferSize);
        try
        {
            var written = serializer.SerializeToBytes(transformedData, buffer);
            var result = new byte[written];
            buffer.AsSpan(0, written).CopyTo(result);
            return result;
        }
        finally
        {
            context.BufferPool.Return(buffer);
        }
    }

    private IFieldTransformer<T> CreateFieldTransformer(HashSet<string> fields)
    {
        // Dynamic IL emission ile field transformer oluştur
        return new DynamicFieldTransformer<T>(fields);

    }
}