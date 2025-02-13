using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Configuration;

/// <summary>
/// Defines a custom validation strategy.
/// </summary>
/// <remarks>
/// EN: Interface for defining custom validation strategies that can be
/// plugged into the validation pipeline. Allows for extensible validation
/// behavior.
///
/// TR: Doğrulama pipeline'ına eklenebilen özel doğrulama stratejileri
/// tanımlamak için arayüz. Genişletilebilir doğrulama davranışı sağlar.
/// </remarks>
public interface IValidationStrategy
{
    /// <summary>
    /// Gets the unique identifier of the strategy.
    /// </summary>
    string StrategyId { get; }
    
    /// <summary>
    /// Gets the priority of the strategy.
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Determines if the strategy applies to an entity.
    /// </summary>
    bool AppliesTo<T>(T entity, ValidationContext context);
    
    /// <summary>
    /// Executes the validation strategy.
    /// </summary>
    Task<ValidationResult> ValidateAsync<T>(
        T entity, 
        ValidationContext context,
        CancellationToken cancellationToken = default);
}