using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Models;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Services.Impl;

/// <summary>
/// Simple implementation of the business rule engine.
/// </summary>
public class SimpleBusinessRuleEngine : IBusinessRuleEngine
{
    public async Task<BusinessRuleResult> EvaluateAsync<T>(T entity, Base.ValidationContext context, CancellationToken cancellationToken = default) where T : IHasBusinessRules
    {
        var violations = new List<RuleViolation>();

        foreach (var rule in entity.BusinessRules)
        {
            // In a real implementation, we would evaluate each rule here
            // For now, we'll just collect the rules that are marked as violated
            if (rule is IEvaluatableRule evaluatable)
            {
                var isValid = await evaluatable.EvaluateAsync(entity, context, cancellationToken);
                if (!isValid)
                {
                    violations.Add(new RuleViolation(rule.RuleId, rule.ErrorMessage));
                }
            }
        }

        return new BusinessRuleResult(violations.Count == 0, violations);
    }
}