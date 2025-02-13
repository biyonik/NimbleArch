using NimbleArch.SharedKernel.Validation.Models;

namespace NimbleArch.SharedKernel.Validation.Result;

/// <summary>
/// Represents the result of business rule evaluation.
/// </summary>
public readonly struct BusinessRuleResult(bool isValid, IEnumerable<RuleViolation> violations = null)
{
    public bool IsValid { get; } = isValid;
    public IReadOnlyCollection<RuleViolation> Violations { get; } = (IReadOnlyCollection<RuleViolation>?)violations?.ToList() ?? Array.Empty<RuleViolation>();
}