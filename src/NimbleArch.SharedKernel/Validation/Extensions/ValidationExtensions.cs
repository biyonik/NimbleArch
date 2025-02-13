using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using NimbleArch.SharedKernel.Validation.Impl;

namespace NimbleArch.SharedKernel.Validation.Extensions;

/// <summary>
/// Provides extension methods for the ExpressionValidator to create fluent validation rules.
/// </summary>
/// <remarks>
/// EN: This class enables a fluent API for defining validation rules, making the validation
/// configuration more readable and maintainable.
///
/// TR: Bu sınıf, doğrulama kurallarını tanımlamak için bir fluent API sağlar, doğrulama
/// yapılandırmasını daha okunabilir ve bakımı kolay hale getirir.
/// </remarks>
public static class ValidatorExtensions
{
    /// <summary>
    /// Adds a rule that ensures a string property is not empty.
    /// </summary>
    public static void NotEmpty<T>(
        this ExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression)
    {
        var memberName = GetMemberName(propertyExpression);
        validator.AddRule(
            entity => !string.IsNullOrEmpty(propertyExpression.Compile()(entity)),
            memberName,
            $"{memberName} cannot be empty");
    }

    
    /// <summary>
    /// Validates that a string property matches a specified regular expression pattern.
    /// </summary>
    /// <remarks>
    /// EN: Uses cached Regex instances for better performance.
    /// The pattern is compiled only once and reused.
    ///
    /// TR: Daha iyi performans için önbelleğe alınmış Regex örnekleri kullanır.
    /// Pattern sadece bir kez derlenir ve tekrar kullanılır.
    /// </remarks>
    public static void MatchesPattern<T>(
        this ExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression,
        string pattern,
        string errorMessage = null)
    {
        var memberName = GetMemberName(propertyExpression);
        var regex = new Regex(pattern, RegexOptions.Compiled);
        var compiledProperty = propertyExpression.Compile();
        
        validator.AddRule(
            Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    typeof(ValidatorExtensions).GetMethod(nameof(IsRegexMatch), BindingFlags.NonPublic | BindingFlags.Static),
                    Expression.Constant(regex),
                    Expression.Invoke(Expression.Constant(compiledProperty), Expression.Parameter(typeof(T), "e"))
                ),
                Expression.Parameter(typeof(T), "e")
            ),
            memberName,
            errorMessage ?? $"{memberName} does not match the required pattern");
    }

    /// <summary>
    /// Validates that a string property represents a valid email address.
    /// </summary>
    /// <remarks>
    /// EN: Uses a specialized high-performance email validation pattern.
    /// Avoids complex regex patterns for better performance.
    ///
    /// TR: Özelleştirilmiş yüksek performanslı e-posta doğrulama pattern'i kullanır.
    /// Daha iyi performans için karmaşık regex pattern'lerinden kaçınır.
    /// </remarks>
    public static void IsEmail<T>(
        this ExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression)
    {
        var memberName = GetMemberName(propertyExpression);
        var compiledProperty = propertyExpression.Compile();

        validator.AddRule(
            Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    typeof(ValidatorExtensions).GetMethod(nameof(IsValidEmail), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException(),
                    Expression.Invoke(Expression.Constant(compiledProperty), Expression.Parameter(typeof(T), "e"))
                ),
                Expression.Parameter(typeof(T), "e")
            ),
            memberName,
            $"{memberName} must be a valid email address");
    }

    /// <summary>
    /// Validates that a numeric property falls within a specified range.
    /// </summary>
    /// <remarks>
    /// EN: Supports all numeric types through generic constraints.
    /// Uses value type comparisons for better performance.
    ///
    /// TR: Generic kısıtlamalar aracılığıyla tüm sayısal tipleri destekler.
    /// Daha iyi performans için değer tipi karşılaştırmaları kullanır.
    /// </remarks>
    public static void InRange<T, TProperty>(
        this ExpressionValidator<T> validator,
        Expression<Func<T, TProperty>> propertyExpression,
        TProperty min,
        TProperty max) 
        where TProperty : IComparable<TProperty>
    {
        var memberName = GetMemberName(propertyExpression);
        var param = Expression.Parameter(typeof(T), "e");
        var property = Expression.Invoke(propertyExpression, param);
        
        var greaterThanMin = Expression.GreaterThanOrEqual(
            Expression.Call(property, typeof(IComparable<TProperty>).GetMethod("CompareTo") ?? throw new InvalidOperationException(), Expression.Constant(min)),
            Expression.Constant(0));
            
        var lessThanMax = Expression.LessThanOrEqual(
            Expression.Call(property, typeof(IComparable<TProperty>).GetMethod("CompareTo") ?? throw new InvalidOperationException(), Expression.Constant(max)),
            Expression.Constant(0));
            
        var combined = Expression.AndAlso(greaterThanMin, lessThanMax);

        validator.AddRule(
            Expression.Lambda<Func<T, bool>>(combined, param),
            memberName,
            $"{memberName} must be between {min} and {max}");
    }

    /// <summary>
    /// Validates that a collection property has a specific number of items.
    /// </summary>
    /// <remarks>
    /// EN: Optimized for collections by checking Count/Length properties directly.
    /// Supports both exact count and range validation.
    ///
    /// TR: Count/Length özelliklerini doğrudan kontrol ederek koleksiyonlar için optimize edilmiştir.
    /// Hem tam sayı hem de aralık doğrulamasını destekler.
    /// </remarks>
    public static void HasCount<T, TCollection>(
        this ExpressionValidator<T> validator,
        Expression<Func<T, TCollection>> propertyExpression,
        int? exactCount = null,
        int? minCount = null,
        int? maxCount = null) 
        where TCollection : IEnumerable
    {
        var memberName = GetMemberName(propertyExpression);
        var compiledProperty = propertyExpression.Compile();

        validator.AddRule(
            Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    typeof(ValidatorExtensions).GetMethod(nameof(CheckCount), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException(),
                    Expression.Invoke(Expression.Constant(compiledProperty), Expression.Parameter(typeof(T), "e")),
                    Expression.Constant(exactCount),
                    Expression.Constant(minCount),
                    Expression.Constant(maxCount)
                ),
                Expression.Parameter(typeof(T), "e")
            ),
            memberName,
            GetCountErrorMessage(memberName, exactCount, minCount, maxCount));
    }

    /// <summary>
    /// Validates that a string property has a specific length.
    /// </summary>
    /// <remarks>
    /// EN: Uses direct string length comparisons for optimal performance.
    /// Supports both exact length and range validation.
    ///
    /// TR: Optimal performans için doğrudan string uzunluğu karşılaştırmaları kullanır.
    /// Hem tam uzunluk hem de aralık doğrulamasını destekler.
    /// </remarks>
    public static void HasLength<T>(
    this ExpressionValidator<T> validator,
    Expression<Func<T, string>> propertyExpression,
    int? exactLength = null,
    int? minLength = null,
    int? maxLength = null)
{
    var memberName = GetMemberName(propertyExpression);
    var param = Expression.Parameter(typeof(T), "e");
    var property = Expression.Invoke(propertyExpression, param);
    
    // Null check
    var nullCheck = Expression.Equal(property, Expression.Constant(null, typeof(string)));
    
    // Length property access
    var lengthProperty = Expression.Property(property, typeof(string).GetProperty("Length"));
    
    Expression validationExpression;
    
    if (exactLength.HasValue)
    {
        // value == null || value.Length == exactLength
        validationExpression = Expression.OrElse(
            nullCheck,
            Expression.Equal(lengthProperty, Expression.Constant(exactLength.Value))
        );
    }
    else
    {
        var checks = new List<Expression>();
        
        if (minLength.HasValue)
        {
            // value.Length >= minLength
            checks.Add(Expression.GreaterThanOrEqual(
                lengthProperty,
                Expression.Constant(minLength.Value)
            ));
        }
        
        if (maxLength.HasValue)
        {
            // value.Length <= maxLength
            checks.Add(Expression.LessThanOrEqual(
                lengthProperty,
                Expression.Constant(maxLength.Value)
            ));
        }
        
        // Combine all checks with AND operations
        var combinedChecks = checks.Aggregate((a, b) => Expression.AndAlso(a, b));
        
        // value == null || (combined length checks)
        validationExpression = Expression.OrElse(nullCheck, combinedChecks);
    }
    
    validator.AddRule(
        Expression.Lambda<Func<T, bool>>(validationExpression, param),
        memberName,
        GetLengthErrorMessage(memberName, exactLength, minLength, maxLength));
}
    
    private static bool IsRegexMatch(Regex regex, string value)
    {
        return value == null || regex.IsMatch(value);
    }
    
    private static bool CheckCount(IEnumerable collection, int? exactCount, int? minCount, int? maxCount)
    {
        if (collection == null) return true;

        var count = collection.Cast<object>().Count();

        if (exactCount.HasValue)
            return count == exactCount.Value;

        var isValid = true;
        if (minCount.HasValue)
            isValid &= count >= minCount.Value;
        if (maxCount.HasValue)
            isValid &= count <= maxCount.Value;

        return isValid;
    }

    // Helper methods...
    private static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new ArgumentException("Expression must be a member expression");
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        // High-performance email validation without complex regex
        int atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1) return false;
        
        int dotIndex = email.IndexOf('.', atIndex);
        return dotIndex > atIndex && dotIndex < email.Length - 1;
    }

    private static string GetCountErrorMessage(string propertyName, int? exact, int? min, int? max)
    {
        if (exact.HasValue)
            return $"{propertyName} must contain exactly {exact.Value} items";
        
        if (min.HasValue && max.HasValue)
            return $"{propertyName} must contain between {min.Value} and {max.Value} items";
        
        if (min.HasValue)
            return $"{propertyName} must contain at least {min.Value} items";
        
        if (max.HasValue)
            return $"{propertyName} must contain no more than {max.Value} items";
        
        return $"{propertyName} has invalid count";
    }

    private static string GetLengthErrorMessage(string propertyName, int? exact, int? min, int? max)
    {
        if (exact.HasValue)
            return $"{propertyName} must be exactly {exact.Value} characters long";
        
        if (min.HasValue && max.HasValue)
            return $"{propertyName} must be between {min.Value} and {max.Value} characters long";
        
        if (min.HasValue)
            return $"{propertyName} must be at least {min.Value} characters long";
        
        if (max.HasValue)
            return $"{propertyName} must not exceed {max.Value} characters";
        
        return $"{propertyName} has invalid length";
    }
}