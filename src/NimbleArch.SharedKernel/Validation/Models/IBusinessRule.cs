namespace NimbleArch.SharedKernel.Validation.Models;

/// <summary>
/// Defines a business rule contract.
/// </summary>
/// <remarks>
/// EN: Base interface for all business rules in the system.
/// Provides a standard way to define and evaluate business rules.
///
/// TR: Sistemdeki tüm iş kuralları için temel arayüz.
/// İş kurallarını tanımlamak ve değerlendirmek için standart bir yol sağlar.
/// </remarks>
public interface IBusinessRule
{
    /// <summary>
    /// Gets the unique identifier of the rule.
    /// </summary>
    string RuleId { get; }

    /// <summary>
    /// Gets the error message when rule is violated.
    /// </summary>
    string ErrorMessage { get; }
}