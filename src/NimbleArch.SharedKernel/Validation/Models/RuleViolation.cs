namespace NimbleArch.SharedKernel.Validation.Models;

/// <summary>
/// Represents a business rule violation.
/// </summary>
public class RuleViolation(string rule, string message)
{
    /// <summary>
    /// Gets the rule that was violated.
    /// </summary>
    public string Rule { get; } = rule;

    /// <summary>
    /// Gets the violation message.
    /// </summary>
    public string Message { get; } = message;
}