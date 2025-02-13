using NimbleArch.SharedKernel.Validation.Models;

namespace NimbleArch.SharedKernel.Validation.Interfaces;

/// <summary>
/// Defines an entity that has associated business rules.
/// </summary>
/// <remarks>
/// EN: Marks an entity as having domain-specific business rules that must be validated.
/// Enables complex validation scenarios involving multiple business rules.
///
/// TR: Bir varlığı, doğrulanması gereken alana özgü iş kurallarına sahip olarak işaretler.
/// Birden fazla iş kuralı içeren karmaşık doğrulama senaryolarını mümkün kılar.
/// </remarks>
public interface IHasBusinessRules
{
    /// <summary>
    /// Gets the collection of business rules.
    /// </summary>
    /// <remarks>
    /// EN: The set of business rules that must be satisfied by this entity.
    /// TR: Bu varlık tarafından karşılanması gereken iş kuralları kümesi.
    /// </remarks>
    IReadOnlyCollection<IBusinessRule> BusinessRules { get; }
}