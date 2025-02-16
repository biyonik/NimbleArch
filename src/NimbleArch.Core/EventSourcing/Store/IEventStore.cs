using NimbleArch.Core.EventSourcing.Events;

namespace NimbleArch.Core.EventSourcing.Store;

/// <summary>
/// High-performance event storage interface.
/// </summary>
public interface IEventStore
{
    ValueTask AppendEventAsync(EventDescriptor eventDescriptor, CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<EventDescriptor> GetEventsAsync(
        Guid aggregateId, 
        long fromVersion = 0,
        CancellationToken cancellationToken = default);
    
    ValueTask<long> GetLastSequenceAsync(CancellationToken cancellationToken = default);
}