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
    byte[] Transform(T data, ReadOnlySpan<byte> source, ResponseTransformationContext context);

}