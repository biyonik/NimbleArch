using System.Collections.Concurrent;
using NimbleArch.Core.Entities.Features;

namespace NimbleArch.Core.Entities.Base;

/// <summary>
/// High-performance state tracking implementation.
/// </summary>
public sealed class EntityStateTracker<TEntity, TKey>(TEntity entity) : IEntityStateManager<TEntity, TKey>
    where TEntity : EntityBase<TKey> where TKey : struct
{
    private readonly ConcurrentDictionary<string, object> _originalValues = new();
   private bool _isTracking;

   public bool IsTracking => _isTracking;

   public void StartTracking()
   {
       if (_isTracking) return;

       _isTracking = true;
       CaptureOriginalValues();
   }

   public void StopTracking()
   {
       _isTracking = false;
       _originalValues.Clear();
   }

   public IReadOnlyDictionary<string, object> GetChanges()
   {
       if (!_isTracking)
           return new Dictionary<string, object>();

       var changes = new Dictionary<string, object>();
       var properties = typeof(TEntity).GetProperties();

       foreach (var property in properties)
       {
           if (!_originalValues.TryGetValue(property.Name, out var originalValue))
               continue;

           var currentValue = property.GetValue(entity);
           if (!Equals(originalValue, currentValue))
           {
               changes[property.Name] = currentValue;
           }
       }

       return changes;
   }

   public void AcceptChanges()
   {
       if (!_isTracking) return;
       CaptureOriginalValues();
   }

   public void RejectChanges()
   {
       if (!_isTracking) return;

       var properties = typeof(TEntity).GetProperties();
       foreach (var property in properties)
       {
           if (_originalValues.TryGetValue(property.Name, out var originalValue))
           {
               property.SetValue(entity, originalValue);
           }
       }
   }

   private void CaptureOriginalValues()
   {
       _originalValues.Clear();
       var properties = typeof(TEntity).GetProperties();

       foreach (var property in properties)
       {
           var value = property.GetValue(entity);
           _originalValues[property.Name] = value;
       }
   }
}