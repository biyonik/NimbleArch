using NimbleArch.SharedKernel.Validation.Models;

namespace NimbleArch.SharedKernel.Validation.Interfaces;

/// <summary>
/// Defines an entity that has relationships with other entities.
/// </summary>
/// <remarks>
/// EN: Indicates that the entity has relationships that need to be validated
/// for consistency and referential integrity.
///
/// TR: Varlığın tutarlılık ve referans bütünlüğü için doğrulanması gereken
/// ilişkileri olduğunu belirtir.
/// </remarks>
public interface IHasRelations
{
    /// <summary>
    /// Gets the collection of entity relations.
    /// </summary>
    /// <remarks>
    /// EN: The relationships that this entity has with other entities.
    /// TR: Bu varlığın diğer varlıklarla olan ilişkileri.
    /// </remarks>
    IReadOnlyCollection<EntityRelation> Relations { get; }
}