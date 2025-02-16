namespace NimbleArch.Core.EventSourcing.Events;

/// <summary>
/// High-performance event metadata container.
/// </summary>
public readonly struct EventDescriptor(
    Guid eventId,
    long sequence,
    string eventType,
    long timestamp,
    string aggregateType,
    Guid aggregateId,
    long version,
    ReadOnlyMemory<byte> data,
    IReadOnlyDictionary<string, string> metadata)
{
    public Guid EventId { get; } = eventId;
    public long Sequence { get; } = sequence;
    public string EventType { get; } = eventType;
    public long Timestamp { get; } = timestamp;
    public string AggregateType { get; } = aggregateType;
    public Guid AggregateId { get; } = aggregateId;
    public long Version { get; } = version;
    public ReadOnlyMemory<byte> Data { get; } = data;
    public IReadOnlyDictionary<string, string> Metadata { get; } = metadata;
}