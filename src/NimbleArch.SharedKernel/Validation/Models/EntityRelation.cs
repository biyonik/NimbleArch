namespace NimbleArch.SharedKernel.Validation.Models;

/// <summary>
/// Represents a relationship between entities.
/// </summary>
/// <remarks>
/// EN: Describes a relationship between two entities, including the type
/// of relationship and any constraints that must be maintained.
///
/// TR: İki varlık arasındaki ilişkiyi, ilişki türünü ve korunması
/// gereken kısıtlamaları da içerecek şekilde tanımlar.
/// </remarks>
public class EntityRelation(
    Type sourceType,
    Type relatedType,
    RelationType relationType,
    string foreignKeyProperty)
{
    /// <summary>
    /// Gets the source entity type.
    /// </summary>
    public Type SourceType { get; } = sourceType;

    /// <summary>
    /// Gets the related entity type.
    /// </summary>
    public Type RelatedType { get; } = relatedType;

    /// <summary>
    /// Gets the type of relationship.
    /// </summary>
    public RelationType RelationType { get; } = relationType;

    /// <summary>
    /// Gets the foreign key property name.
    /// </summary>
    public string ForeignKeyProperty { get; } = foreignKeyProperty;
}