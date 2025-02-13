namespace NimbleArch.SharedKernel.Validation.Configuration;

/// <summary>
/// Global configuration for validation behavior.
/// </summary>
/// <remarks>
/// EN: Provides centralized configuration for validation behavior across the application.
/// Uses the builder pattern for fluent configuration and ensures thread-safety through
/// immutability after initialization.
///
/// TR: Uygulama genelinde doğrulama davranışı için merkezi yapılandırma sağlar.
/// Akıcı yapılandırma için builder pattern kullanır ve başlatıldıktan sonra
/// değişmezlik yoluyla thread-safety sağlar.
/// </remarks>
public class ValidationConfiguration
{
    /// <summary>
    /// Gets the default error message templates.
    /// </summary>
    public IReadOnlyDictionary<string, string> ErrorMessageTemplates { get; }
    
    /// <summary>
    /// Gets the maximum validation depth for nested objects.
    /// </summary>
    public int MaxValidationDepth { get; }
    
    /// <summary>
    /// Gets whether to stop validation on first failure.
    /// </summary>
    public bool StopOnFirstFailure { get; }
    
    /// <summary>
    /// Gets whether parallel validation is enabled.
    /// </summary>
    public bool EnableParallelValidation { get; }
    
    /// <summary>
    /// Gets the validation timeout in milliseconds.
    /// </summary>
    public int ValidationTimeoutMs { get; }
    
    /// <summary>
    /// Gets the custom validation strategies.
    /// </summary>
    public IReadOnlyCollection<IValidationStrategy> ValidationStrategies { get; }

    private ValidationConfiguration(Builder builder)
    {
        ErrorMessageTemplates = builder._errorMessageTemplates;
        MaxValidationDepth = builder._maxValidationDepth;
        StopOnFirstFailure = builder._stopOnFirstFailure;
        EnableParallelValidation = builder._enableParallelValidation;
        ValidationTimeoutMs = builder._validationTimeoutMs;
        ValidationStrategies = builder._validationStrategies;
    }

    public class Builder
    {
        internal Dictionary<string, string> _errorMessageTemplates = new();
        internal int _maxValidationDepth = 10;
        internal bool _stopOnFirstFailure = false;
        internal bool _enableParallelValidation = false;
        internal int _validationTimeoutMs = 30000; // 30 seconds
        internal List<IValidationStrategy> _validationStrategies = [];

        /// <summary>
        /// Adds or updates an error message template.
        /// </summary>
        public Builder WithErrorMessageTemplate(string key, string template)
        {
            _errorMessageTemplates[key] = template;
            return this;
        }

        /// <summary>
        /// Sets the maximum validation depth.
        /// </summary>
        public Builder WithMaxValidationDepth(int depth)
        {
            _maxValidationDepth = depth;
            return this;
        }

        /// <summary>
        /// Configures whether to stop on first failure.
        /// </summary>
        public Builder WithStopOnFirstFailure(bool stop = true)
        {
            _stopOnFirstFailure = stop;
            return this;
        }

        /// <summary>
        /// Enables or disables parallel validation.
        /// </summary>
        public Builder WithParallelValidation(bool enable = true)
        {
            _enableParallelValidation = enable;
            return this;
        }

        /// <summary>
        /// Sets the validation timeout.
        /// </summary>
        public Builder WithValidationTimeout(int milliseconds)
        {
            _validationTimeoutMs = milliseconds;
            return this;
        }

        /// <summary>
        /// Adds a custom validation strategy.
        /// </summary>
        public Builder WithValidationStrategy(IValidationStrategy strategy)
        {
            _validationStrategies.Add(strategy);
            return this;
        }

        /// <summary>
        /// Builds the configuration.
        /// </summary>
        public ValidationConfiguration Build()
        {
            return new ValidationConfiguration(this);
        }
    }
}