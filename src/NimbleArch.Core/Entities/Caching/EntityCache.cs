using System.Collections.Concurrent;
using NimbleArch.Core.Entities.Factory;

namespace NimbleArch.Core.Entities.Caching;

/// <summary>
/// High-performance entity cache with memory optimization.
/// </summary>
public class EntityCache<TEntity, TKey>(IEntityFactory<TEntity, TKey> factory)
    where TEntity : EntityBase<TKey>
    where TKey : struct
{
    private readonly ConcurrentDictionary<TKey, WeakReference<TEntity>> _cache = new();

    public TEntity GetOrCreate(TKey id, Func<TKey, TEntity> factory1 = null)
    {
        if (_cache.TryGetValue(id, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var entity))
            {
                return entity;
            }
        }

        var newEntity = factory1?.Invoke(id) ?? factory.CreateWithId(id);
        _cache[id] = new WeakReference<TEntity>(newEntity);
        return newEntity;
    }

    public void Cache(TEntity entity)
    {
        _cache[entity.Id] = new WeakReference<TEntity>(entity);
    }

    public void Invalidate(TKey id)
    {
        _cache.TryRemove(id, out _);
    }
}