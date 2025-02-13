namespace NimbleArch.SharedKernel.Validation.Exception;

/// <summary>
/// Represents a validation error with property and error message information.
/// </summary>
/// <remarks>
/// EN: This record struct provides an immutable, allocation-optimized way to store validation errors.
/// It uses the record struct feature for value-based equality while maintaining value type benefits.
///
/// TR: Bu record struct, doğrulama hatalarını saklamak için değişmez, bellek tahsisi optimize edilmiş bir yol sağlar.
/// Değer tipi avantajlarını korurken değer bazlı eşitlik için record struct özelliğini kullanır.
/// </remarks>
public readonly record struct ValidationError(string PropertyName, string ErrorMessage)
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; } = PropertyName;

    /// <summary>
    /// Gets the error message describing the validation failure.
    /// </summary>
    public string ErrorMessage { get; } = ErrorMessage;
}