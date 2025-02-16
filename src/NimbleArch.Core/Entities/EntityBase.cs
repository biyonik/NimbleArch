using NimbleArch.Core.Entities.Base;

namespace NimbleArch.Core.Entities;

/// <summary>
/// Base class for all domain entities with optimized state tracking.
/// </summary>
public abstract class EntityBase<TKey> where TKey : struct
{
    private readonly HashSet<string> _modifiedProperties = new();
    private readonly Queue<IDomainEvent> _domainEvents = new();
    private EntityState _state;
    private long _version;

    public TKey Id { get; private set; }
    public virtual long Version => _version;

    /// <summary>
    /// Gets the current state of the entity.
    /// </summary>
    public EntityState State
    {
        get => _state;
        protected set
        {
            if (_state != value)
            {
                _state = value;
                OnStateChanged();
            }
        }
    }

    /// <summary>
    /// Gets the modified properties since last save.
    /// </summary>
    public IReadOnlySet<string> ModifiedProperties => _modifiedProperties;

    /// <summary>
    /// Gets pending domain events.
    /// </summary>
    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;
    
    protected internal void SetId(TKey id)
    {
        Id = id;
        MarkModified(nameof(Id));
    }

    protected void MarkModified(string propertyName)
    {
        _modifiedProperties.Add(propertyName);
        State = EntityState.Modified;
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Enqueue(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected virtual void OnStateChanged() { }

    public void IncrementVersion()
    {
        Interlocked.Increment(ref _version);
    }

    protected virtual EntitySnapshot<TKey> CreateSnapshot()
    {
        return new EntitySnapshot<TKey>
        {
            Id = Id,
            Version = _version,
            State = _state,
            ModifiedProperties = _modifiedProperties.ToList(),
            Events = _domainEvents.ToList()
        };
    }

    public virtual void RestoreFromSnapshot(EntitySnapshot<TKey> snapshot)
    {
        Id = snapshot.Id;
        _version = snapshot.Version;
        _state = snapshot.State;
        _modifiedProperties.Clear();
        foreach (var prop in snapshot.ModifiedProperties)
        {
            _modifiedProperties.Add(prop);
        }
        _domainEvents.Clear();
        foreach (var @event in snapshot.Events)
        {
            _domainEvents.Enqueue(@event);
        }
    }

    public virtual bool NeedsSnapshot(int eventsCount)
    {
        // Default olarak her 100 event'te bir snapshot alalım
        return eventsCount % 100 == 0;
    }
}