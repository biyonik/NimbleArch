namespace NimbleArch.Core.EventSourcing.Events;

/// <summary>
/// Base class for aggregate-related events.
/// </summary>
/// <remarks>
/// EN: Represents events that are specific to an aggregate root.
/// Includes aggregate identification and versioning information.
///
/// TR: Bir aggregate root'a özgü olayları temsil eder.
/// Aggregate tanımlama ve versiyonlama bilgilerini içerir.
/// </remarks>
public abstract class AggregateEvent<TKey> : Event where TKey : struct
{
    public TKey AggregateId { get; }

    protected AggregateEvent(TKey aggregateId, long version) : base(version)
    {
        AggregateId = aggregateId;
    }
}