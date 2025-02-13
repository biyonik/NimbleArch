namespace NimbleArch.SharedKernel.Validation.Services;

/// <summary>
/// Defines authorization service operations.
/// </summary>
/// <remarks>
/// EN: Provides methods for checking user permissions and performing authorization checks.
/// Supports both synchronous and asynchronous permission validation.
///
/// TR: Kullanıcı izinlerini kontrol etmek ve yetkilendirme kontrolleri yapmak için
/// metodlar sağlar. Hem senkron hem de asenkron izin doğrulamasını destekler.
/// </remarks>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if a user has the required permissions.
    /// </summary>
    Task<bool> HasPermissionAsync(
        string userId, 
        IEnumerable<string> requiredPermissions,
        CancellationToken cancellationToken = default);

    bool HasPermission(
        string userId,
        IEnumerable<string> requiredPermissions);

    /// <summary>
    /// Gets user's effective permissions.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}