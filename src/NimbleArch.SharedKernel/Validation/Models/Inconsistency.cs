namespace NimbleArch.SharedKernel.Validation.Models;

/// <summary>
/// Represents a data inconsistency.
/// </summary>
public class Inconsistency(string entity, string message)
{
    /// <summary>
    /// Gets the entity type where inconsistency was found.
    /// </summary>
    public string Entity { get; } = entity;

    /// <summary>
    /// Gets the inconsistency message.
    /// </summary>
    public string Message { get; } = message;
}