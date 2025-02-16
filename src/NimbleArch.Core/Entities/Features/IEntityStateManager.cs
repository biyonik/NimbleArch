namespace NimbleArch.Core.Entities.Features;

/// <summary>
/// Manages entity state transitions and tracking.
/// </summary>
public interface IEntityStateManager<TEntity, TKey> 
    where TEntity : EntityBase<TKey>
    where TKey: struct
{
    bool IsTracking { get; }
    void StartTracking();
    void StopTracking();
    IReadOnlyDictionary<string, object> GetChanges();
    void AcceptChanges();
    void RejectChanges();
}