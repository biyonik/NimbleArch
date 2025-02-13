using System.Collections.Concurrent;
using System.Linq.Expressions;
using NimbleArch.SharedKernel.Validation.Abstract;
using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Impl;

/// <summary>
/// A high-performance validator implementation using Expression Trees and compiled delegates.
/// </summary>
/// <remarks>
/// EN: This class provides a fast validation mechanism by compiling validation rules into IL code.
/// It caches compiled expressions for subsequent validations and uses struct-based storage
/// to minimize heap allocations. The validator supports complex rule chains and custom error messages.
///
/// TR: Bu sınıf, doğrulama kurallarını IL koduna derleyerek hızlı bir doğrulama mekanizması sağlar.
/// Derlenmiş expression'ları sonraki doğrulamalar için önbelleğe alır ve heap tahsislerini en aza
/// indirmek için struct tabanlı depolama kullanır. Doğrulayıcı, karmaşık kural zincirleri ve
/// özel hata mesajlarını destekler.
/// </remarks>
/// <typeparam name="T">The type of object to validate</typeparam>
public class ExpressionValidator<T> : IValidator<T>
{
    private readonly ConcurrentDictionary<string, Func<T, ValidationError?>> _compiledRules = new();
    private readonly List<ValidationRule> _rules = [];

    /// <summary>
    /// Represents a single validation rule with its associated expression and error message.
    /// </summary>
    private readonly struct ValidationRule(
        Expression<Func<T, bool>> predicate,
        string propertyName,
        string errorMessage)
    {
        public Expression<Func<T, bool>> Predicate { get; } = predicate;
        public string PropertyName { get; } = propertyName;
        public string ErrorMessage { get; } = errorMessage;
    }

    /// <summary>
    /// Adds a validation rule for a specific property.
    /// </summary>
    /// <remarks>
    /// EN: Adds a new validation rule using an expression tree. The rule is compiled to IL
    /// code when first used and cached for subsequent validations.
    ///
    /// TR: Expression tree kullanarak yeni bir doğrulama kuralı ekler. Kural ilk kullanıldığında
    /// IL koduna derlenir ve sonraki doğrulamalar için önbelleğe alınır.
    /// </remarks>
    /// <param name="predicate">The validation rule expression</param>
    /// <param name="propertyName">Name of the property being validated</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    public void AddRule(Expression<Func<T, bool>> predicate, string propertyName, string errorMessage)
    {
        _rules.Add(new ValidationRule(predicate, propertyName, errorMessage));
    }

    /// <summary>
    /// Validates an entity against all defined rules.
    /// </summary>
    /// <remarks>
    /// EN: Performs validation using compiled expressions for maximum performance.
    /// Returns a struct-based ValidationResult to minimize allocations.
    ///
    /// TR: Maksimum performans için derlenmiş expression'lar kullanarak doğrulama gerçekleştirir.
    /// Tahsisleri en aza indirmek için struct tabanlı ValidationResult döndürür.
    /// </remarks>
    public ValidationResult Validate(T entity)
    {
        if (entity == null)
            return new ValidationResult(new[] { new ValidationError("Entity", "Entity cannot be null") });

        var errors = new List<ValidationError>();

        foreach (var rule in _rules)
        {
            var compiledRule = _compiledRules.GetOrAdd(
                rule.PropertyName,
                _ => CompileRule(rule));

            var error = compiledRule(entity);
            if (error.HasValue)
                errors.Add(error.Value);
        }

        return new ValidationResult(errors);
    }

    /// <summary>
    /// Compiles a validation rule into a highly optimized delegate.
    /// </summary>
    /// <remarks>
    /// EN: Transforms the validation rule expression into compiled IL code for maximum performance.
    /// Uses expression composition to create a delegate that returns a ValidationError when the rule fails.
    ///
    /// TR: Maksimum performans için doğrulama kuralı expression'ını derlenmiş IL koduna dönüştürür.
    /// Kural başarısız olduğunda ValidationError döndüren bir delegate oluşturmak için expression
    /// kompozisyonu kullanır.
    /// </remarks>
    private Func<T, ValidationError?> CompileRule(ValidationRule rule)
    {
        var parameter = Expression.Parameter(typeof(T), "entity");
        
        var condition = Expression.Invoke(rule.Predicate, parameter);
        
        var createError = Expression.New(
            typeof(ValidationError).GetConstructor([typeof(string), typeof(string)]) ?? throw new InvalidOperationException(),
            Expression.Constant(rule.PropertyName),
            Expression.Constant(rule.ErrorMessage));

        var nullValue = Expression.Constant(null, typeof(ValidationError?));

        var body = Expression.Condition(
            condition,
            nullValue,
            Expression.Convert(createError, typeof(ValidationError?)));

        return Expression.Lambda<Func<T, ValidationError?>>(body, parameter).Compile();
    }

    public ValidationResult Validate(T entity, ValidationContext? context, ValidationGroup? group)
    {
        return Validate(entity);
    }

    public Task<ValidationResult> ValidateAsync(object entity, ValidationContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Validate((T)entity, context, null));
    }
}