namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Represents the result of a command execution.
/// </summary>
public readonly struct CommandResult
{
    /// <summary>
    /// Gets whether the command was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if the command failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the affected entities' identifiers.
    /// </summary>
    public IReadOnlyList<object> AffectedIds { get; }

    private CommandResult(bool isSuccess, string? errorMessage, IReadOnlyList<object> affectedIds)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        AffectedIds = affectedIds;
    }

    public static CommandResult Success(params object[] affectedIds) =>
        new(true, null, affectedIds);

    public static CommandResult Failure(string? error) =>
        new(false, error, Array.Empty<object>());
}