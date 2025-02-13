using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Abstract;

/// <summary>
/// Represents an asynchronous validator that can perform validations requiring I/O operations.
/// </summary>
/// <remarks>
/// EN: This interface is designed for validations that need to access external resources
/// such as databases, APIs, or file systems. It uses ValueTask for better performance
/// with synchronous completions.
///
/// TR: Bu arayüz, veritabanları, API'ler veya dosya sistemleri gibi dış kaynaklara
/// erişmesi gereken doğrulamalar için tasarlanmıştır. Senkron tamamlamalar için
/// daha iyi performans sağlamak üzere ValueTask kullanır.
/// </remarks>
public interface IAsyncValidator<T>
{
    /// <summary>
    /// Asynchronously validates the specified entity.
    /// </summary>
    /// <remarks>
    /// EN: Uses ValueTask for better performance when validation completes synchronously.
    /// The validation logic is compiled once and cached for subsequent validations.
    ///
    /// TR: Doğrulama senkron olarak tamamlandığında daha iyi performans için ValueTask kullanır.
    /// Doğrulama mantığı bir kez derlenir ve sonraki doğrulamalar için önbelleğe alınır.
    /// </remarks>
    ValueTask<ValidationResult> ValidateAsync(T entity, CancellationToken cancellationToken = default);
}