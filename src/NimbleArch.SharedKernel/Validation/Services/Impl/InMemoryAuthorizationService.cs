namespace NimbleArch.SharedKernel.Validation.Services.Impl;

/// <summary>
/// In-memory implementation of the authorization service.
/// </summary>
public class InMemoryAuthorizationService : IAuthorizationService
{
    private readonly Dictionary<string, HashSet<string>> _userPermissions = new();

    public async Task<bool> HasPermissionAsync(
        string userId,
        IEnumerable<string> requiredPermissions,
        CancellationToken cancellationToken = default)
    {
        if (!_userPermissions.TryGetValue(userId, out var permissions))
            return false;

        return requiredPermissions.All(permissions.Contains);
    }

    public bool HasPermission(string userId, IEnumerable<string> requiredPermissions)
    {
        return _userPermissions.TryGetValue(userId, out var permissions) && requiredPermissions.All(permissions.Contains);
    }

    public async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (!_userPermissions.TryGetValue(userId, out var permissions))
            return Array.Empty<string>();

        return permissions.ToList();
    }

    // Helper method to set up test data
    public void AddUserPermissions(string userId, params string[] permissions)
    {
        _userPermissions[userId] = new HashSet<string>(permissions);
    }
}