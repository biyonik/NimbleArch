namespace NimbleArch.Core.MultiTenancy;

/// <summary>
/// Manages tenant context for the current execution scope.
/// </summary>
/// <remarks>
/// EN: Provides thread-safe access to tenant information using AsyncLocal storage.
/// Supports tenant isolation in multi-tenant scenarios.
///
/// TR: AsyncLocal depolama kullanarak thread-safe kiracı bilgisi erişimi sağlar.
/// Çok kiracılı senaryolarda kiracı izolasyonunu destekler.
/// </remarks>
public static class TenantContext
{
    private static readonly AsyncLocal<TenantInfo> _currentTenant = new();

    public static TenantInfo Current
    {
        get => _currentTenant.Value;
        set => _currentTenant.Value = value;
    }

    public static string CurrentTenantId => Current?.TenantId;

    public static void Clear()
    {
        Current = null;
    }
}