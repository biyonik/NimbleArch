namespace NimbleArch.Core.EventSourcing.Events;

/// <summary>
/// Defines metadata capabilities for events.
/// </summary>
public interface IEventMetadata
{
    string CorrelationId { get; }
    string CausationId { get; }
    string UserId { get; }
    string TenantId { get; }
    IDictionary<string, string> AdditionalData { get; }
}