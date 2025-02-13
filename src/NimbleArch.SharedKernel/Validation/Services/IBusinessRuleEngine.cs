using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Services;

/// <summary>
/// Defines business rule evaluation operations.
/// </summary>
/// <remarks>
/// EN: Provides methods for evaluating business rules and managing rule execution.
/// Supports complex rule chains and conditional rule execution.
///
/// TR: İş kurallarını değerlendirmek ve kural yürütmesini yönetmek için metodlar sağlar.
/// Karmaşık kural zincirlerini ve koşullu kural yürütmeyi destekler.
/// </remarks>
public interface IBusinessRuleEngine
{
    /// <summary>
    /// Evaluates business rules for an entity.
    /// </summary>
    Task<BusinessRuleResult> EvaluateAsync<T>(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default) where T : IHasBusinessRules;
}