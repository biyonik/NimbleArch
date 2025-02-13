using System.Collections.Concurrent;
using System.Linq.Expressions;
using NimbleArch.SharedKernel.Validation.Abstract;
using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Impl;

/// <summary>
/// A validator that supports validation groups for conditional rule application.
/// </summary>
/// <remarks>
/// EN: Extends the base validator with group support, allowing rules to be applied
/// selectively based on the operation being performed. Maintains the high-performance
/// characteristics of the base validator.
///
/// TR: Temel doğrulayıcıyı grup desteği ile genişletir, kuralların yapılan işleme
/// göre seçici olarak uygulanmasına olanak tanır. Temel doğrulayıcının yüksek
/// performans özelliklerini korur.
/// </remarks>
public class GroupedExpressionValidator<T> : IValidator<T>
{
    private readonly ConcurrentDictionary<string, Func<T, ValidationError?>> _compiledRules = new();
    private readonly List<GroupedValidationRule<T>> _rules = new();

    /// <summary>
    /// Adds a validation rule that applies to specific groups.
    /// </summary>
    public void AddRule(
        Expression<Func<T, bool>> predicate,
        string propertyName,
        string errorMessage,
        params ValidationGroup[] groups)
    {
        _rules.Add(new GroupedValidationRule<T>(predicate, propertyName, errorMessage, groups));
    }

    /// <summary>
    /// Validates the entity against rules in the specified group.
    /// </summary>
    public ValidationResult Validate(T entity, ValidationGroup group)
    {
        if (entity == null)
            return new ValidationResult(new[] { new ValidationError("Entity", "Entity cannot be null") });

        var errors = new List<ValidationError>();

        foreach (var rule in _rules.Where(r => r.Groups.Any(g => g.IsInGroup(group))))
        {
            var compiledRule = _compiledRules.GetOrAdd(
                $"{rule.PropertyName}_{group.Name}",
                _ => CompileRule(rule));

            var error = compiledRule(entity);
            if (error.HasValue)
                errors.Add(error.Value);
        }

        return new ValidationResult(errors);
    }

    /// <summary>
    /// Validates the entity against all rules (ignoring groups).
    /// </summary>
    public ValidationResult Validate(T entity) => Validate(entity, ValidationGroup.Default.Create);

    /// <summary>
    /// Compiles a validation rule into a highly optimized delegate.
    /// </summary>
    /// <remarks>
    /// EN: Transforms the validation rule expression into compiled IL code for maximum performance.
    /// Uses expression composition to create a delegate that returns a ValidationError when the rule fails.
    ///
    /// TR: Doğrulama kuralı expression'ını maksimum performans için derlenmiş IL koduna dönüştürür.
    /// Kural başarısız olduğunda ValidationError döndüren bir delegate oluşturmak için expression
    /// kompozisyonu kullanır.
    /// </remarks>
    private Func<T, ValidationError?> CompileRule(GroupedValidationRule<T> rule)
    {
        var parameter = Expression.Parameter(typeof(T), "entity");
    
        // Invoke the rule's predicate with the parameter
        var condition = Expression.Invoke(rule.Predicate, parameter);
    
        // Create a new ValidationError when the condition fails
        var createError = Expression.New(
            typeof(ValidationError).GetConstructor(new[] { typeof(string), typeof(string) }) ?? throw new InvalidOperationException(),
            Expression.Constant(rule.PropertyName),
            Expression.Constant(rule.ErrorMessage));

        // Create a null value of type ValidationError?
        var nullValue = Expression.Constant(null, typeof(ValidationError?));

        // Create a conditional expression: if (condition) null else new ValidationError(...)
        var body = Expression.Condition(
            condition,
            nullValue,
            Expression.Convert(createError, typeof(ValidationError?)));

        // Compile the expression into a delegate
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