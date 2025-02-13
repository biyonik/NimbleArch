namespace NimbleArch.SharedKernel.Validation.Interfaces;

/// <summary>
/// Defines an entity that requires authorization checks.
/// </summary>
/// <remarks>
/// EN: Indicates that the entity has specific authorization requirements.
/// Used to enforce access control and permission checks.
///
/// TR: Varlığın belirli yetkilendirme gereksinimleri olduğunu belirtir.
/// Erişim kontrolü ve izin kontrollerini uygulamak için kullanılır.
/// </remarks>
public interface IRequiresAuthorization
{
    /// <summary>
    /// Gets the collection of required permissions.
    /// </summary>
    /// <remarks>
    /// EN: The set of permissions required to perform operations on this entity.
    /// TR: Bu varlık üzerinde işlem yapmak için gereken izinler kümesi.
    /// </remarks>
    IReadOnlyCollection<string> RequiredPermissions { get; }
}