namespace NimbleArch.Core.Entities.Features;

/// <summary>
/// Interface for entities that need concurrency control.
/// </summary>
public interface IConcurrencyAware
{
    byte[] RowVersion { get; set; }
    void UpdateConcurrencyToken();
}