using NimbleArch.SharedKernel.Validation.Exception;

namespace NimbleArch.SharedKernel.Validation.Result;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
/// <remarks>
/// EN: This struct is designed to be allocation-free by using a value type.
/// It contains validation errors and status using a memory-efficient approach.
///
/// TR: Bu struct, değer tipi kullanarak allocation-free olacak şekilde tasarlanmıştır.
/// Bellek açısından verimli bir yaklaşım kullanarak doğrulama hatalarını ve durumunu içerir.
/// </remarks>
public readonly struct ValidationResult(IReadOnlyList<ValidationError> errors)
{
    /// <summary>
    /// Gets a value indicating whether the validation passed.
    /// </summary>
    /// <remarks>
    /// EN: True if the validation passed, false otherwise.
    /// This property is a computed value based on the presence of errors.
    ///
    /// TR: Doğrulama başarılı ise true, değilse false.
    /// Bu özellik, hataların varlığına göre hesaplanan bir değerdir.
    /// </remarks>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    /// <remarks>
    /// EN: Uses a highly optimized immutable array to store errors.
    /// The array is allocated only when errors exist.
    ///
    /// TR: Hataları saklamak için yüksek düzeyde optimize edilmiş değişmez bir dizi kullanır.
    /// Dizi yalnızca hatalar mevcut olduğunda tahsis edilir.
    /// </remarks>
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}