using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Interfaces;

namespace NimbleArch.SharedKernel.Validation.Pipeline.Steps;

/// <summary>
/// Validates that an entity belongs to the correct tenant.
/// </summary>
/// <remarks>
/// EN: Ensures data isolation in multi-tenant scenarios by validating tenant ownership.
/// This step should typically be one of the first steps in the pipeline to prevent
/// unauthorized cross-tenant data access.
///
/// TR: Çok kiracılı senaryolarda kiracı sahipliğini doğrulayarak veri izolasyonunu sağlar.
/// Bu adım, yetkisiz çapraz kiracı veri erişimini önlemek için genellikle pipeline'daki
/// ilk adımlardan biri olmalıdır.
/// </remarks>
public class TenantValidationStep<T> : IValidationStep<T> where T : IHasTenant
{
    /// <summary>
    /// Executes tenant validation for the given entity.
    /// </summary>
    /// <remarks>
    /// EN: Verifies that the entity's tenant ID matches the current context's tenant ID.
    /// Returns a failure result if there's a tenant mismatch or if tenant information is missing.
    ///
    /// TR: Varlığın kiracı ID'sinin mevcut bağlamın kiracı ID'si ile eşleştiğini doğrular.
    /// Kiracı uyuşmazlığı varsa veya kiracı bilgisi eksikse başarısızlık sonucu döndürür.
    /// </remarks>
    public Task<ValidationStepResult> ExecuteAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(context.TenantId))
        {
            return Task.FromResult(ValidationStepResult.Failure([
                new ValidationError("Tenant", "Validation context must include tenant information")
            ]));
        }

        if (entity.TenantId != context.TenantId)
        {
            return Task.FromResult(ValidationStepResult.Failure([
                new ValidationError("Tenant", "Entity belongs to a different tenant")
            ]));
        }

        return Task.FromResult(ValidationStepResult.Success());
    }
}