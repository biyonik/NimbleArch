namespace NimbleArch.Core.EventSourcing.Events;

/// <summary>
/// Implementation of event metadata.
/// </summary>
public class EventMetadata : IEventMetadata
{
    public string CorrelationId { get; }
    public string CausationId { get; }
    public string UserId { get; }
    public string TenantId { get; }
    public IDictionary<string, string> AdditionalData { get; }

    public EventMetadata(
        string correlationId,
        string causationId,
        string userId,
        string tenantId,
        IDictionary<string, string> additionalData = null)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
        UserId = userId;
        TenantId = tenantId;
        AdditionalData = additionalData ?? new Dictionary<string, string>();
    }
}