using NimbleArch.Core.Entities.Base;

namespace NimbleArch.Core.EventSourcing.Events;

/// <summary>
/// Base class for all events in the system.
/// </summary>
/// <remarks>
/// EN: Provides common event properties and metadata handling.
/// Designed for efficient serialization and event sourcing.
///
/// TR: Tüm olaylar için ortak özellikler ve metadata yönetimi sağlar.
/// Verimli serileştirme ve event sourcing için tasarlanmıştır.
/// </remarks>
public abstract class Event : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public long Version { get; }
    public string EventType => GetType().Name;
    
    protected Event(long version)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        Version = version;
    }
}