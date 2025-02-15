namespace NimbleArch.Core.DependencyInjection;

/// <summary>
/// Service locator for resolving dependencies.
/// </summary>
/// <remarks>
/// EN: Provides a centralized service resolution mechanism. Note that this is generally
/// considered an anti-pattern, but we're using it specifically for our generated types
/// and cache implementation.
///
/// TR: Merkezi bir servis çözümleme mekanizması sağlar. Bu genellikle anti-pattern
/// olarak kabul edilir, ancak özellikle üretilen tipler ve önbellek implementasyonu
/// için kullanıyoruz.
/// </remarks>
public static class ServiceLocator
{
    private static IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes the service locator with a service provider.
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? 
                           throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    public static object GetService(Type serviceType)
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException(
                "ServiceLocator has not been initialized. Call Initialize first.");
        }

        return _serviceProvider.GetService(serviceType);
    }

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T));
    }
}