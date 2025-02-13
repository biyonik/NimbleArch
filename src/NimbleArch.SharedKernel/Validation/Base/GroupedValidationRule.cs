using System.Linq.Expressions;

namespace NimbleArch.SharedKernel.Validation.Base;

/// <summary>
/// Represents a validation rule that belongs to specific validation groups.
/// </summary>
internal readonly struct GroupedValidationRule<T>(
    Expression<Func<T, bool>> predicate,
    string propertyName,
    string errorMessage,
    IEnumerable<ValidationGroup> groups)
{
    public Expression<Func<T, bool>> Predicate { get; } = predicate;
    public string PropertyName { get; } = propertyName;
    public string ErrorMessage { get; } = errorMessage;
    public HashSet<ValidationGroup> Groups { get; } = [..groups];
}