namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Represents the result of a bulk command execution.
/// </summary>
public readonly struct BulkCommandResult
{
    /// <summary>
    /// Gets successful operations count.
    /// </summary>
    public int SuccessCount { get; }

    /// <summary>
    /// Gets failed operations count.
    /// </summary>
    public int FailureCount { get; }

    /// <summary>
    /// Gets the collection of errors if any operations failed.
    /// </summary>
    public IReadOnlyDictionary<Guid, string> Errors { get; }

    /// <summary>
    /// Gets the affected entities' identifiers.
    /// </summary>
    public IReadOnlyList<object> AffectedIds { get; }

    private BulkCommandResult(
        int successCount,
        int failureCount,
        IReadOnlyDictionary<Guid, string> errors,
        IReadOnlyList<object> affectedIds)
    {
        SuccessCount = successCount;
        FailureCount = failureCount;
        Errors = errors;
        AffectedIds = affectedIds;
    }

    public static BulkCommandResult Success(int count, IReadOnlyList<object> affectedIds) =>
        new(count, 0, new Dictionary<Guid, string>(), affectedIds);

    public static BulkCommandResult Partial(
        int successCount,
        IDictionary<Guid, string> errors,
        IReadOnlyList<object> affectedIds) =>
        new(successCount, errors.Count, errors as IReadOnlyDictionary<Guid, string>, affectedIds);
}