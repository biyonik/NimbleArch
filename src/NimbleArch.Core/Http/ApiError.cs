namespace NimbleArch.Core.Http;

/// <summary>
/// Represents a structured API error.
/// </summary>
public readonly struct ApiError
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets additional error details.
    /// </summary>
    public IReadOnlyDictionary<string, object> Details { get; }

    public ApiError(
        string code,
        string message,
        IReadOnlyDictionary<string, object> details = null)
    {
        Code = code;
        Message = message;
        Details = details ?? new Dictionary<string, object>();
    }
}