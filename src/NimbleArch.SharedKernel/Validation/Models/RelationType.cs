namespace NimbleArch.SharedKernel.Validation.Models;

/// <summary>
/// Defines types of entity relationships.
/// </summary>
public enum RelationType
{
    /// <summary>
    /// One-to-one relationship
    /// </summary>
    OneToOne,

    /// <summary>
    /// One-to-many relationship
    /// </summary>
    OneToMany,

    /// <summary>
    /// Many-to-one relationship
    /// </summary>
    ManyToOne,

    /// <summary>
    /// Many-to-many relationship
    /// </summary>
    ManyToMany
}