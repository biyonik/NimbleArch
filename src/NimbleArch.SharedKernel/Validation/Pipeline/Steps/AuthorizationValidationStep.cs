using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Services;

namespace NimbleArch.SharedKernel.Validation.Pipeline.Steps;

/// <summary>
/// Validates user permissions for entity operations.
/// </summary>
/// <remarks>
/// EN: Performs authorization checks to ensure the current user has necessary permissions.
/// Integrates with the application's authorization system to enforce access control rules.
///
/// TR: Mevcut kullanıcının gerekli izinlere sahip olduğundan emin olmak için yetkilendirme
/// kontrollerini gerçekleştirir. Erişim kontrolü kurallarını uygulamak için uygulamanın
/// yetkilendirme sistemi ile entegre çalışır.
/// </remarks>
public class AuthorizationValidationStep<T>(IAuthorizationService authService) : IValidationStep<T>
    where T : IRequiresAuthorization
{
    /// <summary>
    /// Executes authorization validation for the given entity.
    /// </summary>
    /// <remarks>
    /// EN: Checks if the current user has the required permissions for the operation.
    /// Uses the authorization service to validate user permissions against entity requirements.
    ///
    /// TR: Mevcut kullanıcının işlem için gerekli izinlere sahip olup olmadığını kontrol eder.
    /// Kullanıcı izinlerini varlık gereksinimleriyle karşılaştırmak için yetkilendirme
    /// servisini kullanır.
    /// </remarks>
    public async Task<ValidationStepResult> ExecuteAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(context.UserId))
        {
            return ValidationStepResult.Failure(new[]
            {
                new ValidationError("Authorization", "User context is required for authorization")
            });
        }

        var hasPermission = await authService.HasPermissionAsync(
            context.UserId,
            entity.RequiredPermissions,
            cancellationToken);

        if (!hasPermission)
        {
            return ValidationStepResult.Failure(new[]
            {
                new ValidationError("Authorization", "User does not have required permissions")
            });
        }

        return ValidationStepResult.Success();
    }
}