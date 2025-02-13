namespace NimbleArch.SharedKernel.Validation.Configuration;

/// <summary>
/// Provides access to validation configuration.
/// </summary>
/// <remarks>
/// EN: Singleton provider for accessing validation configuration throughout
/// the application. Ensures consistent validation behavior across different
/// parts of the system.
///
/// TR: Uygulama genelinde doğrulama yapılandırmasına erişim sağlayan
/// singleton sağlayıcı. Sistemin farklı bölümlerinde tutarlı doğrulama
/// davranışı sağlar.
/// </remarks>
public class ValidationConfigurationProvider
{
    private static ValidationConfiguration _current;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the current validation configuration.
    /// </summary>
    public static ValidationConfiguration Current
    {
        get
        {
            if (_current == null)
            {
                lock (_lock)
                {
                    _current ??= GetDefaultConfiguration();
                }
            }
            return _current;
        }
    }

    /// <summary>
    /// Sets the validation configuration.
    /// </summary>
    public static void SetConfiguration(ValidationConfiguration configuration)
    {
        lock (_lock)
        {
            _current = configuration;
        }
    }

    private static ValidationConfiguration GetDefaultConfiguration()
    {
        return new ValidationConfiguration.Builder()
            .WithErrorMessageTemplate("Required", "{PropertyName} is required.")
            .WithErrorMessageTemplate("Length", "{PropertyName} must be between {MinLength} and {MaxLength} characters.")
            .WithErrorMessageTemplate("Range", "{PropertyName} must be between {Min} and {Max}.")
            .WithMaxValidationDepth(10)
            .WithValidationTimeout(30000)
            .Build();
    }
}