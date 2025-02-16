using NimbleArch.Core.Entities.Base;

namespace NimbleArch.Core.Entities.Features;

/// <summary>
/// Interface for entities that support versioning.
/// </summary>
public interface IVersionedEntity
{
    long Version { get; }
    IReadOnlyCollection<IDomainEvent> GetUncommittedEvents();
    void ClearUncommittedEvents();
    void ApplyEvent(IDomainEvent @event);
}