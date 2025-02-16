namespace NimbleArch.Core.Entities.LazyLoading;

/// <summary>
/// Optimized lazy loading implementation for entity relationships.
/// </summary>
public class LazyLoader<TEntity, TProperty>
{
    private readonly Func<Task<TProperty>> _valueFactory;
    private readonly Action<TEntity, TProperty> _valueUpdater;
    private volatile bool _isLoaded;
    private TProperty _value;

    public LazyLoader(
        Func<Task<TProperty>> valueFactory,
        Action<TEntity, TProperty> valueUpdater)
    {
        _valueFactory = valueFactory;
        _valueUpdater = valueUpdater;
    }

    public async Task<TProperty> LoadAsync()
    {
        if (_isLoaded) return _value;
        var value = await _valueFactory();
        _value = value;
        _isLoaded = true;
        return _value;
    }

    public bool IsLoaded => _isLoaded;

    public void Invalidate()
    {
        _isLoaded = false;
        _value = default;
    }
}