namespace NimbleArch.Core.Entities.Base;

/// <summary>
/// Base class for domain events with metadata tracking.
/// </summary>
public abstract class DomainEventBase(long version) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public long Version { get; } = version;
    public string EventType => GetType().Name;
}