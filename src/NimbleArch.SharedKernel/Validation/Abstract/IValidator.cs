using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Abstract;

/// <summary>
/// Base interface for all validators.
/// </summary>
/// <remarks>
/// EN: Provides a non-generic base for all validators, enabling type-agnostic validation handling.
/// Used primarily for reflection-based validator instantiation.
///
/// TR: Tüm doğrulayıcılar için generic olmayan bir temel sağlar, tip-bağımsız doğrulama
/// yönetimini mümkün kılar. Öncelikle reflection-tabanlı doğrulayıcı örneklemesi için kullanılır.
/// </remarks>
public interface IValidator
{
    /// <summary>
    /// Validates an object using the provided context.
    /// </summary>
    Task<ValidationResult> ValidateAsync(
        object entity,
        ValidationContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a high-performance validator that uses Expression Trees for compile-time validation.
/// </summary>
/// <remarks>
/// EN: This interface defines a contract for validators that perform validation using Expression Trees,
/// eliminating runtime reflection costs. The validation rules are compiled into IL code at runtime
/// for maximum performance.
///
/// TR: Bu arayüz, Expression Tree'ler kullanarak doğrulama yapan validatörler için bir sözleşme tanımlar.
/// Çalışma zamanı reflection maliyetlerini ortadan kaldırır. Doğrulama kuralları, maksimum performans için
/// çalışma zamanında IL koduna derlenir.
/// </remarks>
/// <typeparam name="T">
/// EN: The type of object to be validated
/// TR: Doğrulanacak nesnenin tipi
/// </typeparam>
public interface IValidator<in T>: IValidator
{
    /// <summary>
    /// Validates the specified entity and returns a validation result.
    /// </summary>
    /// <remarks>
    /// EN: Performs high-performance validation using compiled expressions.
    /// The validation logic is converted to IL code on first use and cached.
    /// 
    /// TR: Derlenmiş expression'lar kullanarak yüksek performanslı doğrulama gerçekleştirir.
    /// Doğrulama mantığı ilk kullanımda IL koduna dönüştürülür ve önbelleğe alınır.
    /// </remarks>
    /// <param name="entity">
    /// EN: The entity to validate
    /// TR: Doğrulanacak varlık
    /// </param>
    /// <param name="context"></param>
    /// <param name="group"></param>
    /// <returns>
    /// EN: A ValidationResult containing validation details
    /// TR: Doğrulama detaylarını içeren ValidationResult
    /// </returns>
    ValidationResult Validate(T entity, ValidationContext? context, ValidationGroup? group);
}