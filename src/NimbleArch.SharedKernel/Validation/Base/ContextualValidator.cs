using System.Collections.Concurrent;
using System.Linq.Expressions;
using NimbleArch.SharedKernel.Validation.Abstract;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Base;

public class ContextualValidator<T> : IValidator<T>
{
    private readonly ConcurrentDictionary<string, Func<T, ValidationContext, ValidationError?>> _compiledRules;
    private readonly List<ContextualValidationRule> _rules;
    
    private readonly struct ContextualValidationRule
    {
        public Expression<Func<T, ValidationContext, bool>> Predicate { get; }
        public string PropertyName { get; }
        public string ErrorMessage { get; }
        public HashSet<ValidationGroup> Groups { get; }

        public ContextualValidationRule(
            Expression<Func<T, ValidationContext, bool>> predicate,
            string propertyName,
            string errorMessage,
            IEnumerable<ValidationGroup> groups)
        {
            Predicate = predicate;
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            Groups = new HashSet<ValidationGroup>(groups);
        }
    }
    
    /// <summary>
    /// Adds a new validation rule with context support to the validator.
    /// </summary>
    /// <remarks>
    /// EN: Adds a new validation rule that has access to the validation context.
    /// The rule will only be executed for the specified validation groups.
    /// The predicate is compiled once and cached for subsequent validations.
    /// Supports complex business rules that require contextual information like:
    /// - Multi-tenant validations
    /// - User-specific rules
    /// - Environment-dependent validations
    /// 
    /// TR: Doğrulama bağlamına erişimi olan yeni bir doğrulama kuralı ekler.
    /// Kural yalnızca belirtilen doğrulama grupları için çalıştırılır.
    /// Predicate bir kez derlenir ve sonraki doğrulamalar için önbelleğe alınır.
    /// Şu gibi bağlamsal bilgi gerektiren karmaşık iş kurallarını destekler:
    /// - Çok kiracılı doğrulamalar
    /// - Kullanıcıya özel kurallar
    /// - Ortama bağlı doğrulamalar
    /// </remarks>
    /// <param name="predicate">
    /// EN: The validation rule expression that includes context information
    /// TR: Bağlam bilgisini içeren doğrulama kuralı ifadesi
    /// </param>
    /// <param name="propertyName">
    /// EN: Name of the property being validated
    /// TR: Doğrulanan özelliğin adı
    /// </param>
    /// <param name="errorMessage">
    /// EN: Error message to display when validation fails
    /// TR: Doğrulama başarısız olduğunda gösterilecek hata mesajı
    /// </param>
    /// <param name="groups">
    /// EN: Validation groups this rule belongs to
    /// TR: Bu kuralın ait olduğu doğrulama grupları
    /// </param>
    public void AddRule(
        Expression<Func<T, ValidationContext, bool>> predicate,
        string propertyName,
        string errorMessage,
        params ValidationGroup[] groups)
    {
        _rules.Add(new ContextualValidationRule(predicate, propertyName, errorMessage, groups));
    }

    /// <summary>
    /// Validates an entity using the specified context and validation group.
    /// </summary>
    /// <remarks>
    /// EN: Performs validation of an entity within a specific context and group.
    /// Only rules belonging to the specified group (or its parent groups) are executed.
    /// The validation is performed using compiled expressions for maximum performance.
    /// Each rule has access to the context information during validation.
    /// 
    /// TR: Belirli bir bağlam ve grup içinde bir varlığın doğrulamasını gerçekleştirir.
    /// Yalnızca belirtilen gruba (veya üst gruplarına) ait kurallar çalıştırılır.
    /// Doğrulama, maksimum performans için derlenmiş ifadeler kullanılarak gerçekleştirilir.
    /// Her kural, doğrulama sırasında bağlam bilgisine erişebilir.
    /// </remarks>
    /// <param name="entity">
    /// EN: The entity to validate
    /// TR: Doğrulanacak varlık
    /// </param>
    /// <param name="context">
    /// EN: Context information for the validation
    /// TR: Doğrulama için bağlam bilgisi
    /// </param>
    /// <param name="group">
    /// EN: The validation group to use
    /// TR: Kullanılacak doğrulama grubu
    /// </param>
    /// <returns>
    /// EN: A ValidationResult containing any validation errors
    /// TR: Doğrulama hatalarını içeren ValidationResult
    /// </returns>
    public ValidationResult Validate(T entity, ValidationContext? context, ValidationGroup? group)
    {
        if (entity == null)
            return new ValidationResult(new[] { new ValidationError("Entity", "Entity cannot be null") });

        var errors = new List<ValidationError>();

        foreach (var rule in _rules.Where(r => r.Groups.Any(g => g.IsInGroup(group))))
        {
            var compiledRule = _compiledRules.GetOrAdd(
                $"{rule.PropertyName}_{group.Name}",
                _ => CompileRule(rule));

            var error = compiledRule(entity, context);
            if (error.HasValue)
                errors.Add(error.Value);
        }

        return new ValidationResult(errors);
    }

    /// <summary>
    /// Compiles a validation rule into an optimized delegate.
    /// </summary>
    /// <remarks>
    /// EN: Transforms a contextual validation rule into compiled IL code.
    /// Uses expression trees to create a highly optimized validation delegate.
    /// The compiled delegate is cached for subsequent validations.
    /// Handles both the validation logic and error creation in a single compiled unit.
    /// 
    /// TR: Bağlamsal bir doğrulama kuralını derlenmiş IL koduna dönüştürür.
    /// Yüksek düzeyde optimize edilmiş bir doğrulama delegate'i oluşturmak için expression tree'ler kullanır.
    /// Derlenen delegate sonraki doğrulamalar için önbelleğe alınır.
    /// Hem doğrulama mantığını hem de hata oluşturmayı tek bir derlenmiş birimde işler.
    /// </remarks>
    /// <param name="rule">
    /// EN: The validation rule to compile
    /// TR: Derlenecek doğrulama kuralı
    /// </param>
    /// <returns>
    /// EN: A compiled delegate that performs the validation
    /// TR: Doğrulamayı gerçekleştiren derlenmiş delegate
    /// </returns>
    private Func<T, ValidationContext, ValidationError?> CompileRule(ContextualValidationRule rule)
    {
        var entityParam = Expression.Parameter(typeof(T), "entity");
        var contextParam = Expression.Parameter(typeof(ValidationContext), "context");
        
        var condition = Expression.Invoke(rule.Predicate, entityParam, contextParam);
        
        var createError = Expression.New(
            typeof(ValidationError).GetConstructor(new[] { typeof(string), typeof(string) }),
            Expression.Constant(rule.PropertyName),
            Expression.Constant(rule.ErrorMessage));

        var nullValue = Expression.Constant(null, typeof(ValidationError?));

        var body = Expression.Condition(
            condition,
            nullValue,
            Expression.Convert(createError, typeof(ValidationError?)));

        return Expression.Lambda<Func<T, ValidationContext, ValidationError?>>(
            body,
            entityParam,
            contextParam).Compile();
    }

    public Task<ValidationResult> ValidateAsync(object entity, ValidationContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Validate((T)entity, context:context, group:null));
    }
    
}