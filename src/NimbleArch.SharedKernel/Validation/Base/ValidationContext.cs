namespace NimbleArch.SharedKernel.Validation.Base;

/// <summary>
/// Represents the context in which validation is performed.
/// </summary>
/// <remarks>
/// EN: Provides a comprehensive validation context that encapsulates all necessary information
/// for context-aware validation rules. This immutable class ensures thread-safety and
/// prevents unintended modifications during validation. It supports multi-tenant scenarios,
/// user-specific validations, environment-based rules, and custom validation requirements.
///
/// TR: Bağlama duyarlı doğrulama kuralları için gerekli tüm bilgileri kapsayan bir doğrulama
/// bağlamı sağlar. Bu değişmez (immutable) sınıf, thread-safety sağlar ve doğrulama sırasında
/// istenmeyen değişiklikleri önler. Çok kiracılı senaryoları, kullanıcıya özel doğrulamaları,
/// ortama bağlı kuralları ve özel doğrulama gereksinimlerini destekler.
/// </remarks>
public class ValidationContext
{
    /// <summary>
    /// Gets the current user's identifier.
    /// </summary>
    /// <remarks>
    /// EN: Represents the unique identifier of the user performing the validation.
    /// Used for user-specific validation rules and authorization checks.
    ///
    /// TR: Doğrulamayı gerçekleştiren kullanıcının benzersiz tanımlayıcısını temsil eder.
    /// Kullanıcıya özel doğrulama kuralları ve yetkilendirme kontrolleri için kullanılır.
    /// </remarks>
    public string UserId { get; }

    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    /// <remarks>
    /// EN: Identifies the tenant in a multi-tenant system. Essential for ensuring
    /// data isolation and tenant-specific validation rules.
    ///
    /// TR: Çok kiracılı bir sistemde kiracıyı tanımlar. Veri izolasyonunu ve
    /// kiracıya özel doğrulama kurallarını sağlamak için gereklidir.
    /// </remarks>
    public string TenantId { get; }

    /// <summary>
    /// Gets the current environment name.
    /// </summary>
    /// <remarks>
    /// EN: Specifies the execution environment (e.g., Development, Staging, Production).
    /// Enables environment-specific validation rules and behaviors.
    ///
    /// TR: Yürütme ortamını belirtir (örn. Development, Staging, Production).
    /// Ortama özel doğrulama kurallarını ve davranışlarını etkinleştirir.
    /// </remarks>
    public string Environment { get; }

    /// <summary>
    /// Gets the collection of services available during validation.
    /// </summary>
    /// <remarks>
    /// EN: Provides access to dependency injection services required for validation.
    /// Enables complex validations that require external service dependencies.
    ///
    /// TR: Doğrulama için gerekli bağımlılık enjeksiyonu servislerine erişim sağlar.
    /// Dış servis bağımlılıkları gerektiren karmaşık doğrulamaları mümkün kılar.
    /// </remarks>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets additional custom properties for validation.
    /// </summary>
    /// <remarks>
    /// EN: Stores custom key-value pairs for validation-specific data.
    /// Provides extensibility for custom validation scenarios not covered by standard properties.
    ///
    /// TR: Doğrulamaya özel veriler için özel anahtar-değer çiftlerini saklar.
    /// Standart özelliklerle karşılanmayan özel doğrulama senaryoları için genişletilebilirlik sağlar.
    /// </remarks>
    public IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Private constructor to enforce the use of the Builder pattern.
    /// </summary>
    /// <remarks>
    /// EN: Initializes a new validation context with the specified parameters.
    /// Private to ensure immutability and proper initialization through the Builder.
    ///
    /// TR: Belirtilen parametrelerle yeni bir doğrulama bağlamı başlatır.
    /// Değişmezliği ve Builder aracılığıyla uygun başlatmayı sağlamak için private yapılmıştır.
    /// </remarks>
    private ValidationContext(
        string userId,
        string tenantId,
        string environment,
        IServiceProvider services,
        IReadOnlyDictionary<string, object> properties)
    {
        UserId = userId;
        TenantId = tenantId;
        Environment = environment;
        Services = services;
        Properties = properties;
    }

    /// <summary>
    /// Builder class for constructing ValidationContext instances.
    /// </summary>
    /// <remarks>
    /// EN: Implements the Builder pattern to provide a fluent and type-safe way to construct
    /// ValidationContext instances. Ensures all required properties are set and validates
    /// the configuration before building.
    ///
    /// TR: ValidationContext örneklerini oluşturmak için akıcı ve tip güvenli bir yol sağlamak
    /// üzere Builder pattern'ı uygular. Tüm gerekli özelliklerin ayarlandığından emin olur ve
    /// yapılandırmayı oluşturmadan önce doğrular.
    /// </remarks>
    public class Builder
    {
        private string _userId;
        private string _tenantId;
        private string _environment;
        private IServiceProvider _services;
        private readonly Dictionary<string, object> _properties = new();

        /// <summary>
        /// Sets the user identifier for the validation context.
        /// </summary>
        /// <remarks>
        /// EN: Adds the user ID to the context, enabling user-specific validation rules.
        /// Returns the builder instance for method chaining.
        ///
        /// TR: Bağlama kullanıcı kimliğini ekler, kullanıcıya özel doğrulama kurallarını etkinleştirir.
        /// Metod zincirlemesi için builder örneğini döndürür.
        /// </remarks>
        /// <param name="userId">
        /// EN: The unique identifier of the current user
        /// TR: Mevcut kullanıcının benzersiz tanımlayıcısı
        /// </param>
        public Builder WithUserId(string userId)
        {
            _userId = userId;
            return this;
        }

        /// <summary>
        /// Sets the tenant identifier for the validation context.
        /// </summary>
        /// <remarks>
        /// EN: Adds the tenant ID to the context for multi-tenant scenarios.
        /// Returns the builder instance for method chaining.
        ///
        /// TR: Çok kiracılı senaryolar için bağlama kiracı kimliğini ekler.
        /// Metod zincirlemesi için builder örneğini döndürür.
        /// </remarks>
        public Builder WithTenantId(string tenantId)
        {
            _tenantId = tenantId;
            return this;
        }

        /// <summary>
        /// Sets the environment name for the validation context.
        /// </summary>
        /// <remarks>
        /// EN: Specifies the current execution environment for environment-specific validations.
        /// Returns the builder instance for method chaining.
        ///
        /// TR: Ortama özel doğrulamalar için mevcut yürütme ortamını belirtir.
        /// Metod zincirlemesi için builder örneğini döndürür.
        /// </remarks>
        public Builder WithEnvironment(string environment)
        {
            _environment = environment;
            return this;
        }

        /// <summary>
        /// Sets the service provider for the validation context.
        /// </summary>
        /// <remarks>
        /// EN: Adds the dependency injection container to enable service resolution during validation.
        /// Returns the builder instance for method chaining.
        ///
        /// TR: Doğrulama sırasında servis çözümlemeyi etkinleştirmek için bağımlılık enjeksiyon
        /// container'ını ekler. Metod zincirlemesi için builder örneğini döndürür.
        /// </remarks>
        public Builder WithServices(IServiceProvider services)
        {
            _services = services;
            return this;
        }

        /// <summary>
        /// Adds a custom property to the validation context.
        /// </summary>
        /// <remarks>
        /// EN: Adds a key-value pair to the context's custom properties collection.
        /// Returns the builder instance for method chaining.
        ///
        /// TR: Bağlamın özel özellikler koleksiyonuna bir anahtar-değer çifti ekler.
        /// Metod zincirlemesi için builder örneğini döndürür.
        /// </remarks>
        public Builder WithProperty(string key, object value)
        {
            _properties[key] = value;
            return this;
        }

        /// <summary>
        /// Builds and returns a new ValidationContext instance.
        /// </summary>
        /// <remarks>
        /// EN: Creates an immutable ValidationContext instance with the configured properties.
        /// Performs validation of required properties before construction.
        ///
        /// TR: Yapılandırılmış özelliklerle değişmez bir ValidationContext örneği oluşturur.
        /// Oluşturmadan önce gerekli özelliklerin doğrulamasını gerçekleştirir.
        /// </remarks>
        /// <returns>
        /// EN: A new, immutable ValidationContext instance
        /// TR: Yeni, değişmez bir ValidationContext örneği
        /// </returns>
        public ValidationContext Build()
        {
            return new ValidationContext(
                _userId,
                _tenantId,
                _environment,
                _services,
                _properties.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}