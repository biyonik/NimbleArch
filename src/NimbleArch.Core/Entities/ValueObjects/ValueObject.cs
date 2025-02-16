namespace NimbleArch.Core.Entities.ValueObjects;

/// <summary>
/// Base struct for value objects with optimized equality comparison.
/// </summary>
public abstract record ValueObject
{
    /// <summary>
    /// Gets the components for equality comparison.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}