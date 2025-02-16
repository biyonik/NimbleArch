using System.Collections.Concurrent;

namespace NimbleArch.Core.Common.ObjectPool;

/// <summary>
/// High-performance object pool implementation.
/// </summary>
public class ObjectPool<T>(
    Func<T> objectGenerator,
    Action<T> resetAction = null,
    int maxSize = 1000)
    : IObjectPool<T>
{
    private readonly ConcurrentBag<T> _objects = new();

    public T Get()
    {
        if (_objects.TryTake(out T item))
        {
            return item;
        }

        return objectGenerator();
    }

    public void Return(T item)
    {
        if (_objects.Count >= maxSize)
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
            return;
        }

        resetAction?.Invoke(item);
        _objects.Add(item);
    }

    public void Clear()
    {
        while (_objects.TryTake(out T item))
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}