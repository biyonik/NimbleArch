using NimbleArch.Core.Entities.Base;
using NimbleArch.Core.Entities.Features;

namespace NimbleArch.Core.Entities.Factory;

/// <summary>
/// Factory interface for creating optimized entity instances.
/// </summary>
public interface IEntityFactory<out TEntity, TKey> where TEntity : EntityBase<TKey> where TKey : struct
{
    TEntity Create();
    TEntity CreateWithId(TKey id);
    TEntity CreateFromSnapshot(EntitySnapshot<TKey> snapshot);
}