using NimbleArch.Core.Common.ObjectPool;
using NimbleArch.Core.Entities.Base;
using NimbleArch.Core.Entities.Features;

namespace NimbleArch.Core.Entities.Factory;

/// <summary>
/// High-performance entity factory implementation.
/// </summary>
public class EntityFactory<TEntity, TKey> : IEntityFactory<TEntity, TKey> 
    where TEntity : EntityBase<TKey> 
    where TKey : struct
{
    private readonly ObjectPool<TEntity> _entityPool;
    private readonly IEntityStateManager<TEntity, TKey> _stateManager;

    public EntityFactory(ObjectPool<TEntity> entityPool, IEntityStateManager<TEntity, TKey> stateManager)
    {
        _entityPool = entityPool;
        _stateManager = stateManager;
    }

    public TEntity Create()
    {
        var entity = _entityPool.Get();
        _stateManager.StartTracking();
        return entity;
    }

    public TEntity CreateWithId(TKey id)
    {
        var entity = Create();
        entity.SetId(id);
        return entity;
    }
    

    public TEntity CreateFromSnapshot(EntitySnapshot<TKey> snapshot)
    {
        var entity = Create();
        entity.RestoreFromSnapshot(snapshot);
        _stateManager.AcceptChanges();
        return entity;
    }
}