namespace NimbleArch.Core.Entities.Features;

public interface IHasTenant
{
    string TenantId { get; set; }
}