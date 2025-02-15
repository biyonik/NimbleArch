namespace NimbleArch.Core.Http.Transformation;

public interface IResponseTransformation<T>
{
    /// <summary>
    /// Transform'un çalışıp çalışmayacağına karar verir.
    /// </summary>
    bool ShouldApply(ResponseTransformationContext context);

    /// <summary>
    /// Response transformation'ı gerçekleştirir.
    /// </summary>
    Span<byte> Transform(T data, Span<byte> source, ResponseTransformationContext context);
}