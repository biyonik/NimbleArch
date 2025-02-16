using NimbleArch.Core.Entities;
using NimbleArch.Core.Entities.Base;
using NimbleArch.Core.EventSourcing.Store;

namespace NimbleArch.Core.EventSourcing.Aggregate;

/// <summary>
/// Base class for event-sourced aggregates with high-performance event handling.
/// </summary>
/// <remarks>
/// EN: Provides optimized event sourcing capabilities with snapshot support
/// and efficient event application mechanisms.
///
/// TR: Snapshot desteği ve verimli olay uygulama mekanizmaları ile
/// optimize edilmiş event sourcing yetenekleri sağlar.
/// </remarks>
public abstract class EventSourcedAggregateRoot<TKey> : EntityBase<TKey> 
   where TKey : struct
{
   private readonly RingBuffer<IDomainEvent> _pendingEvents;
   private long _version = -1;
   private readonly HashSet<Type> _eventHandlers;

   protected EventSourcedAggregateRoot()
   {
       _pendingEvents = new RingBuffer<IDomainEvent>(1000);
       _eventHandlers = new HashSet<Type>();
       RegisterEventHandlers();
   }

   /// <summary>
   /// Gets the version of the aggregate.
   /// </summary>
   public override long Version => _version;

   /// <summary>
   /// Gets all uncommitted events.
   /// </summary>
   public IEnumerable<IDomainEvent> GetUncommittedEvents()
   {
       return _pendingEvents.GetAll();
   }

   /// <summary>
   /// Clears all uncommitted events.
   /// </summary>
   public void ClearUncommittedEvents()
   {
       _pendingEvents.Clear();
   }

   /// <summary>
   /// Loads the aggregate from a sequence of events.
   /// </summary>
   public void LoadFromHistory(IEnumerable<IDomainEvent> history)
   {
       foreach (var @event in history)
       {
           ApplyEvent(@event);
           _version = @event.Version;
       }
   }

   /// <summary>
   /// Applies a new event to the aggregate.
   /// </summary>
   protected void ApplyEvent<TEvent>(TEvent @event) where TEvent : IDomainEvent
   {
       var eventType = @event.GetType();

       if (!_eventHandlers.Contains(eventType))
       {
           throw new InvalidOperationException($"No handler registered for event type: {eventType.Name}");
       }

       // Event'i uygula
       ((dynamic)this).Apply((dynamic)@event);

       // Versiyonu güncelle
       _version++;

       // Event'i pending listesine ekle
       if (@event is IDomainEvent domainEvent)
       {
           _pendingEvents.Write(domainEvent);
       }
   }

   /// <summary>
   /// Creates a snapshot of the aggregate's current state.
   /// </summary>
   protected override EntitySnapshot<TKey> CreateSnapshot()
   {
       var snapshot = base.CreateSnapshot();
       // Ek event sourcing bilgilerini ekle
       return snapshot with
       {
           Version = _version,
           Metadata = new Dictionary<string, string>
           {
               ["LastEventTimestamp"] = DateTime.UtcNow.ToString("O")
           }
       };
   }

   /// <summary>
   /// Restores the aggregate's state from a snapshot.
   /// </summary>
   public override void RestoreFromSnapshot(EntitySnapshot<TKey> snapshot)
   {
       base.RestoreFromSnapshot(snapshot);
       _version = snapshot.Version;
   }

   /// <summary>
   /// Registers event handlers for the aggregate.
   /// </summary>
   protected abstract void RegisterEventHandlers();

   /// <summary>
   /// Registers a handler for a specific event type.
   /// </summary>
   protected void RegisterHandler<TEvent>() where TEvent : IDomainEvent
   {
       _eventHandlers.Add(typeof(TEvent));
   }
}