namespace NimbleArch.SharedKernel.Validation.Interfaces;

/// <summary>
/// Defines an entity that belongs to a specific tenant.
/// </summary>
/// <remarks>
/// EN: Marks an entity as tenant-aware, requiring tenant validation in multi-tenant scenarios.
/// Ensures proper data isolation between different tenants.
///
/// TR: Bir varlığı kiracı-farkında olarak işaretler, çok kiracılı senaryolarda kiracı
/// doğrulaması gerektirir. Farklı kiracılar arasında uygun veri izolasyonunu sağlar.
/// </remarks>
public interface IHasTenant
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    /// <remarks>
    /// EN: The unique identifier of the tenant that owns this entity.
    /// TR: Bu varlığa sahip olan kiracının benzersiz tanımlayıcısı.
    /// </remarks>
    string TenantId { get; set; }
}