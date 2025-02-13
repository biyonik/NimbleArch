namespace NimbleArch.SharedKernel.Validation.Base;

/// <summary>
/// Represents a group of validation rules that can be applied conditionally.
/// </summary>
/// <remarks>
/// EN: Enables scenario-based validation by grouping rules under specific operations
/// like Create, Update, or Delete. Rules can belong to multiple groups.
///
/// TR: Create, Update veya Delete gibi belirli operasyonlar için kuralları
/// gruplandırarak senaryo bazlı doğrulamayı mümkün kılar. Kurallar birden fazla
/// gruba ait olabilir.
/// </remarks>
public sealed class ValidationGroup
{
    /// <summary>
    /// Gets the name of the validation group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the parent group if this is a sub-group.
    /// </summary>
    public ValidationGroup Parent { get; }

    private ValidationGroup(string name, ValidationGroup parent = null)
    {
        Name = name;
        Parent = parent;
    }

    /// <summary>
    /// Predefined validation groups for common operations.
    /// </summary>
    public static class Default
    {
        public static readonly ValidationGroup Create = new("Create");
        public static readonly ValidationGroup Update = new("Update");
        public static readonly ValidationGroup Delete = new("Delete");
        public static readonly ValidationGroup Query = new("Query");
    }

    /// <summary>
    /// Creates a new validation group.
    /// </summary>
    public static ValidationGroup New(string name) => new(name);

    /// <summary>
    /// Creates a sub-group under this group.
    /// </summary>
    public ValidationGroup SubGroup(string name) => new(name, this);

    /// <summary>
    /// Checks if this group is or inherits from the specified group.
    /// </summary>
    public bool IsInGroup(ValidationGroup group)
    {
        var current = this;
        while (current is not null)
        {
            if (current.Name == group.Name)
                return true;
            current = current.Parent;
        }
        return false;
    }
}