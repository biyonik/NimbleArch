using NimbleArch.SharedKernel.Validation.Models;

namespace NimbleArch.SharedKernel.Validation.Result;

/// <summary>
/// Represents the result of a data consistency check.
/// </summary>
public readonly struct ConsistencyCheckResult(bool isConsistent, IEnumerable<Inconsistency>? inconsistencies = null)
{
    public bool IsConsistent { get; } = isConsistent;
    public IReadOnlyCollection<Inconsistency> Inconsistencies { get; } = (IReadOnlyCollection<Inconsistency>?)inconsistencies?.ToList() ?? Array.Empty<Inconsistency>();
}