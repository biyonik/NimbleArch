namespace NimbleArch.Core.Common.ObjectPool;

/// <summary>
/// Defines a contract for object pooling.
/// </summary>
/// <remarks>
/// EN: Provides high-performance object pooling capabilities to reduce GC pressure
/// and improve memory usage patterns.
///
/// TR: GC baskısını azaltmak ve bellek kullanım desenlerini iyileştirmek için
/// yüksek performanslı nesne havuzlama yetenekleri sağlar.
/// </remarks>
public interface IObjectPool<T>
{
    T Get();
    void Return(T item);
    void Clear();
}
