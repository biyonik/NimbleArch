namespace NimbleArch.Core.MultiTenancy;

/// <summary>
/// Represents tenant information.
/// </summary>
public class TenantInfo(
    string tenantId,
    string name,
    TenantStatus status = TenantStatus.Active,
    IDictionary<string, object>? properties = null)
{
    public string TenantId { get; } = tenantId;
    public string Name { get; } = name;
    public TenantStatus Status { get; } = status;
    public IDictionary<string, object>? Properties { get; } = properties;
}