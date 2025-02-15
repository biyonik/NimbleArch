namespace NimbleArch.Core.Http.Transformation;

/// <summary>
/// Defines a contract for field-based data transformation.
/// </summary>
/// <remarks>
/// EN: Allows selective field filtering and transformation of data objects.
/// Uses Expression Trees for high-performance field access and manipulation.
///
/// TR: Veri nesnelerinin seçici alan filtrelemesine ve dönüşümüne olanak tanır.
/// Yüksek performanslı alan erişimi ve manipülasyonu için Expression Tree'ler kullanır.
/// </remarks>
public interface IFieldTransformer<T>
{
    /// <summary>
    /// Transforms an object by including only specified fields.
    /// </summary>
    T Transform(T source);

    /// <summary>
    /// Gets the list of fields included in transformation.
    /// </summary>
    IReadOnlySet<string> IncludedFields { get; }
}
