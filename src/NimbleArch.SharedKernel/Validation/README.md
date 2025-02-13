# NimbleArch Validation System

[English](#english) | [Türkçe](#turkish)

---

## English

### High-Performance Expression Tree Based Validation System

NimbleArch's validation system provides a robust, high-performance, and extensible solution for handling complex validation scenarios. Built using Expression Trees and modern .NET features, it offers superior performance while maintaining code readability and maintainability.

### Key Features

#### 1. Core Validation Engine
- **Expression Tree Based Validation**: Compile-time optimization for validation rules
- **Zero Allocation Design**: Struct-based results and optimized memory usage
- **Async Support**: First-class support for asynchronous validation operations
- **Extensible Architecture**: Easy to add custom validation rules and behaviors

#### 2. Validation Pipeline
The system implements a sophisticated validation pipeline that can handle complex validation scenarios:

```csharp
var pipeline = new ValidationPipeline<Order>()
    .AddStep(new TenantValidationStep<Order>())
    .AddStep(new AuthorizationValidationStep<Order>())
    .AddStep(new BusinessRuleValidationStep<Order>())
    .StopOnFirstFailure(true)
    .ExecuteInParallel(false);
```

#### 3. Context-Aware Validation
Supports rich contextual validation with built-in support for:
- Multi-tenant scenarios
- User authorization
- Environment-specific rules
- Custom business rules

#### 4. Global Configuration
Centralized configuration management:

```csharp
var config = new ValidationConfiguration.Builder()
    .WithErrorMessageTemplate("Required", "{PropertyName} is required.")
    .WithMaxValidationDepth(5)
    .WithStopOnFirstFailure(true)
    .WithParallelValidation(true)
    .WithValidationTimeout(5000)
    .Build();
```

### Getting Started

#### 1. Basic Validation
Create a simple validator:

```csharp
public class UserValidator : ContextualValidator<User>
{
    public UserValidator()
    {
        AddRule(
            u => !string.IsNullOrEmpty(u.Name),
            nameof(User.Name),
            "Name is required");

        AddRule(
            u => u.Age >= 18,
            nameof(User.Age),
            "User must be at least 18 years old");
    }
}
```

#### 2. Using the Pipeline
Implement a validation pipeline:

```csharp
public class OrderValidationPipeline
{
    private readonly ValidationPipeline<Order> _pipeline;

    public OrderValidationPipeline(
        IAuthorizationService authService,
        IBusinessRuleEngine ruleEngine)
    {
        _pipeline = new ValidationPipeline<Order>()
            .AddStep(new TenantValidationStep<Order>())
            .AddStep(new AuthorizationValidationStep<Order>(authService))
            .AddStep(new BusinessRuleValidationStep<Order>(ruleEngine));
    }

    public async Task<ValidationResult> ValidateAsync(
        Order order,
        ValidationContext context)
    {
        return await _pipeline.ExecuteAsync(order, context);
    }
}
```

#### 3. API Integration
Use validation in your API:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    [Validate(typeof(OrderValidator), typeof(CreateOrderRequest))]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        // Validation is automatically handled by middleware
        return Ok();
    }
}
```

### Advanced Features

#### 1. Custom Validation Strategies
Create custom validation strategies:

```csharp
public class DevEnvironmentValidationStrategy : IValidationStrategy
{
    public string StrategyId => "DevEnvironmentValidation";
    public int Priority => 100;

    public bool AppliesTo<T>(T entity, ValidationContext context)
    {
        return context.Environment
            .Equals("Development", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ValidationResult> ValidateAsync<T>(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        // Development environment specific validation
        return ValidationResult.Success();
    }
}
```

#### 2. Async Validation
Perform asynchronous validations:

```csharp
public class UserValidator : ContextualValidator<User>
{
    private readonly IUserService _userService;

    public UserValidator(IUserService userService)
    {
        _userService = userService;

        AddAsyncRule(
            async (user, context) => 
                await _userService.IsEmailUniqueAsync(user.Email),
            nameof(User.Email),
            "Email must be unique");
    }
}
```

#### 3. Conditional Validation
Apply conditional validation rules:

```csharp
public class OrderValidator : ContextualValidator<Order>
{
    public OrderValidator()
    {
        AddRule(
            (order, context) => 
                context.TenantId == order.TenantId,
            nameof(Order.TenantId),
            "Order must belong to current tenant");

        When(
            order => order.Type == OrderType.International,
            () => {
                AddRule(
                    o => !string.IsNullOrEmpty(o.CustomsDeclaration),
                    nameof(Order.CustomsDeclaration),
                    "Customs declaration is required for international orders");
            });
    }
}
```

### Performance Considerations

1. **Expression Trees**: Rules are compiled once and cached
2. **Struct-Based Results**: Minimizes heap allocations
3. **Parallel Validation**: Optional parallel rule execution
4. **Validation Timeout**: Configurable timeout for validation operations

### Best Practices

1. **Rule Organization**
    - Group related rules together
    - Use meaningful rule names
    - Keep rules focused and single-purpose

2. **Error Messages**
    - Use clear and actionable error messages
    - Include all relevant information
    - Support localization

3. **Performance**
    - Use parallel validation when appropriate
    - Configure proper timeout values
    - Monitor validation performance

4. **Security**
    - Always validate tenant context
    - Implement proper authorization checks
    - Sanitize error messages

---

## Turkish

### Yüksek Performanslı Expression Tree Tabanlı Doğrulama Sistemi

### Yüksek Performanslı Expression Tree Tabanlı Doğrulama Sistemi

NimbleArch'ın doğrulama sistemi, karmaşık doğrulama senaryolarını yönetmek için güçlü, yüksek performanslı ve genişletilebilir bir çözüm sunar. Expression Tree'ler ve modern .NET özellikleri kullanılarak oluşturulan bu sistem, kod okunabilirliği ve bakım kolaylığını korurken üstün performans sağlar.

### Temel Özellikler

#### 1. Çekirdek Doğrulama Motoru
- **Expression Tree Tabanlı Doğrulama**: Doğrulama kuralları için derleme zamanı optimizasyonu
- **Sıfır Bellek Tahsisi**: Struct tabanlı sonuçlar ve optimize edilmiş bellek kullanımı
- **Asenkron Destek**: Asenkron doğrulama işlemleri için birinci sınıf destek
- **Genişletilebilir Mimari**: Özel doğrulama kuralları ve davranışları kolayca eklenebilir

#### 2. Doğrulama Pipeline'ı
Sistem, karmaşık doğrulama senaryolarını yönetebilen gelişmiş bir doğrulama pipeline'ı uygular:

```csharp
var pipeline = new ValidationPipeline<Order>()
    .AddStep(new TenantValidationStep<Order>())
    .AddStep(new AuthorizationValidationStep<Order>())
    .AddStep(new BusinessRuleValidationStep<Order>())
    .StopOnFirstFailure(true)
    .ExecuteInParallel(false);
```

#### 3. Bağlam-Duyarlı Doğrulama
Zengin bağlamsal doğrulama için yerleşik destek:
- Çok kiracılı senaryolar
- Kullanıcı yetkilendirme
- Ortama özel kurallar
- Özel iş kuralları

#### 4. Global Yapılandırma
Merkezi yapılandırma yönetimi:

```csharp
var config = new ValidationConfiguration.Builder()
    .WithErrorMessageTemplate("Required", "{PropertyName} zorunludur.")
    .WithMaxValidationDepth(5)
    .WithStopOnFirstFailure(true)
    .WithParallelValidation(true)
    .WithValidationTimeout(5000)
    .Build();
```

### Başlangıç

#### 1. Temel Doğrulama
Basit bir doğrulayıcı oluşturma:

```csharp
public class UserValidator : ContextualValidator<User>
{
    public UserValidator()
    {
        AddRule(
            u => !string.IsNullOrEmpty(u.Name),
            nameof(User.Name),
            "İsim zorunludur");

        AddRule(
            u => u.Age >= 18,
            nameof(User.Age),
            "Kullanıcı en az 18 yaşında olmalıdır");
    }
}
```

#### 2. Pipeline Kullanımı
Doğrulama pipeline'ı implementasyonu:

```csharp
public class OrderValidationPipeline
{
    private readonly ValidationPipeline<Order> _pipeline;

    public OrderValidationPipeline(
        IAuthorizationService authService,
        IBusinessRuleEngine ruleEngine)
    {
        _pipeline = new ValidationPipeline<Order>()
            .AddStep(new TenantValidationStep<Order>())
            .AddStep(new AuthorizationValidationStep<Order>(authService))
            .AddStep(new BusinessRuleValidationStep<Order>(ruleEngine));
    }

    public async Task<ValidationResult> ValidateAsync(
        Order order,
        ValidationContext context)
    {
        return await _pipeline.ExecuteAsync(order, context);
    }
}
```

#### 3. API Entegrasyonu
API'nizde doğrulama kullanımı:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    [Validate(typeof(OrderValidator), typeof(CreateOrderRequest))]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        // Doğrulama middleware tarafından otomatik olarak yapılır
        return Ok();
    }
}
```

### Gelişmiş Özellikler

#### 1. Özel Doğrulama Stratejileri
Özel doğrulama stratejileri oluşturma:

```csharp
public class DevEnvironmentValidationStrategy : IValidationStrategy
{
    public string StrategyId => "DevEnvironmentValidation";
    public int Priority => 100;

    public bool AppliesTo<T>(T entity, ValidationContext context)
    {
        return context.Environment
            .Equals("Development", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ValidationResult> ValidateAsync<T>(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        // Geliştirme ortamına özel doğrulama
        return ValidationResult.Success();
    }
}
```

#### 2. Asenkron Doğrulama
Asenkron doğrulamalar gerçekleştirme:

```csharp
public class UserValidator : ContextualValidator<User>
{
    private readonly IUserService _userService;

    public UserValidator(IUserService userService)
    {
        _userService = userService;

        AddAsyncRule(
            async (user, context) => 
                await _userService.IsEmailUniqueAsync(user.Email),
            nameof(User.Email),
            "Email adresi benzersiz olmalıdır");
    }
}
```

#### 3. Koşullu Doğrulama
Koşullu doğrulama kuralları uygulama:

```csharp
public class OrderValidator : ContextualValidator<Order>
{
    public OrderValidator()
    {
        AddRule(
            (order, context) => 
                context.TenantId == order.TenantId,
            nameof(Order.TenantId),
            "Sipariş mevcut kiracıya ait olmalıdır");

        When(
            order => order.Type == OrderType.International,
            () => {
                AddRule(
                    o => !string.IsNullOrEmpty(o.CustomsDeclaration),
                    nameof(Order.CustomsDeclaration),
                    "Uluslararası siparişler için gümrük beyanı zorunludur");
            });
    }
}
```

### Performans Konuları

1. **Expression Tree'ler**: Kurallar bir kez derlenir ve önbelleğe alınır
2. **Struct Tabanlı Sonuçlar**: Heap tahsislerini minimize eder
3. **Paralel Doğrulama**: İsteğe bağlı paralel kural yürütme
4. **Doğrulama Zaman Aşımı**: Doğrulama işlemleri için yapılandırılabilir zaman aşımı

### En İyi Uygulamalar

1. **Kural Organizasyonu**
    - İlgili kuralları gruplandırın
    - Anlamlı kural isimleri kullanın
    - Kuralları odaklı ve tek amaçlı tutun

2. **Hata Mesajları**
    - Net ve eyleme geçirilebilir hata mesajları kullanın
    - Tüm ilgili bilgileri dahil edin
    - Lokalizasyon desteği sağlayın

3. **Performans**
    - Uygun olduğunda paralel doğrulama kullanın
    - Uygun zaman aşımı değerlerini yapılandırın
    - Doğrulama performansını izleyin

4. **Güvenlik**
    - Her zaman kiracı bağlamını doğrulayın
    - Uygun yetkilendirme kontrolleri uygulayın
    - Hata mesajlarını sanitize edin

### Önemli Notlar

1. **Expression Tree Optimizasyonları**
    - Derleme maliyeti ilk kullanımda gerçekleşir
    - Sonraki kullanımlar için önbellek kullanılır
    - Yüksek performans için kuralları önceden derleyin

2. **Memory Yönetimi**
    - Value type'lar tercih edilir
    - Gereksiz object allocation'lardan kaçınılır
    - Struct result pattern kullanılır

3. **Thread Safety**
    - Tüm validator'lar thread-safe
    - Concurrent dictionary kullanımı
    - Immutable result nesneleri

4. **Hata Yönetimi**
    - Detaylı validation hataları
    - İç içe validation desteği
    - Özelleştirilebilir hata mesajları