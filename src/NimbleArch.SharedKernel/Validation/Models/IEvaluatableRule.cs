using NimbleArch.SharedKernel.Validation.Base;

namespace NimbleArch.SharedKernel.Validation.Models;

/// <summary>
/// Defines a business rule that can be evaluated.
/// </summary>
public interface IEvaluatableRule : IBusinessRule
{
    /// <summary>
    /// Evaluates the rule against an entity.
    /// </summary>
    Task<bool> EvaluateAsync<T>(T entity, ValidationContext context, CancellationToken cancellationToken = default);
}