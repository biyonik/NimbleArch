using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using NimbleArch.SharedKernel.Validation.Abstract;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Impl;

/// <summary>
/// Implements high-performance asynchronous validation using Expression Trees.
/// </summary>
/// <remarks>
/// EN: This class provides an asynchronous validation mechanism optimized for
/// external resource access. It supports caching of compiled expressions and
/// concurrent validation execution.
///
/// TR: Bu sınıf, dış kaynak erişimi için optimize edilmiş asenkron bir doğrulama
/// mekanizması sağlar. Derlenmiş ifadelerin önbelleğe alınmasını ve eşzamanlı
/// doğrulama yürütmeyi destekler.
/// </remarks>
public class AsyncExpressionValidator<T> : IAsyncValidator<T>
{
    private readonly ConcurrentDictionary<string, Func<T, CancellationToken, ValueTask<ValidationError?>>> _compiledRules;
    private readonly List<AsyncValidationRule> _rules;

    /// <summary>
    /// Represents a single asynchronous validation rule.
    /// </summary>
    private readonly struct AsyncValidationRule
    {
        public Expression<Func<T, CancellationToken, ValueTask<bool>>> Predicate { get; }
        public string PropertyName { get; }
        public string ErrorMessage { get; }

        public AsyncValidationRule(
            Expression<Func<T, CancellationToken, ValueTask<bool>>> predicate,
            string propertyName,
            string errorMessage)
        {
            Predicate = predicate;
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }

    public AsyncExpressionValidator()
    {
        _compiledRules = new ConcurrentDictionary<string, Func<T, CancellationToken, ValueTask<ValidationError?>>>();
        _rules = new List<AsyncValidationRule>();
    }

    /// <summary>
    /// Adds an asynchronous validation rule.
    /// </summary>
    /// <remarks>
    /// EN: The rule is compiled once when added and cached for subsequent validations.
    /// Supports cancellation for long-running validations.
    ///
    /// TR: Kural eklendiğinde bir kez derlenir ve sonraki doğrulamalar için önbelleğe alınır.
    /// Uzun süren doğrulamalar için iptal desteği sağlar.
    /// </remarks>
    public void AddRule(
        Expression<Func<T, CancellationToken, ValueTask<bool>>> predicate,
        string propertyName,
        string errorMessage)
    {
        _rules.Add(new AsyncValidationRule(predicate, propertyName, errorMessage));
    }

    /// <summary>
    /// Validates the entity asynchronously against all defined rules.
    /// </summary>
    public async ValueTask<ValidationResult> ValidateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            return new ValidationResult(new[] { new ValidationError("Entity", "Entity cannot be null") });

        var errors = new List<ValidationError>();

        foreach (var rule in _rules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var compiledRule = _compiledRules.GetOrAdd(
                rule.PropertyName,
                _ => CompileRule(rule));

            var error = await compiledRule(entity, cancellationToken);
            if (error.HasValue)
                errors.Add(error.Value);
        }

        return new ValidationResult(errors);
    }

    private Func<T, CancellationToken, ValueTask<ValidationError?>> CompileRule(AsyncValidationRule rule)
    {
        var parameter = Expression.Parameter(typeof(T), "entity");
        var cancelToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
        
        var predicateCall = Expression.Invoke(rule.Predicate, parameter, cancelToken);
        
        var createError = Expression.New(
            typeof(ValidationError).GetConstructor(new[] { typeof(string), typeof(string) }) ?? throw new InvalidOperationException(),
            Expression.Constant(rule.PropertyName),
            Expression.Constant(rule.ErrorMessage));

        // Create method to convert bool to ValidationError?
        var resultConversion = Expression.Call(
            typeof(AsyncExpressionValidator<T>).GetMethod(nameof(ConvertToValidationError), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException(),
            predicateCall,
            createError);

        return Expression.Lambda<Func<T, CancellationToken, ValueTask<ValidationError?>>>(
            resultConversion,
            parameter,
            cancelToken).Compile();
    }

    private static async ValueTask<ValidationError?> ConvertToValidationError(ValueTask<bool> predicate, ValidationError error)
    {
        return await predicate ? null : error;
    }
}