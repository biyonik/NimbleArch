using System.Linq.Expressions;
using NimbleArch.SharedKernel.Validation.Impl;

namespace NimbleArch.SharedKernel.Validation.Extensions;

/// <summary>
/// Provides extension methods for AsyncExpressionValidator.
/// </summary>
public static class AsyncValidatorExtensions
{
    /// <summary>
    /// Validates that a string property is unique in a data store.
    /// </summary>
    public static void IsUnique<T>(
        this AsyncExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression,
        Func<string, CancellationToken, ValueTask<bool>> uniqueCheckFunc)
    {
        var memberName = GetMemberName(propertyExpression);
        var param = Expression.Parameter(typeof(T), "e");
        var cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
        
        var property = Expression.Invoke(propertyExpression, param);
        
        var checkCall = Expression.Invoke(
            Expression.Constant(uniqueCheckFunc),
            property,
            cancelToken);

        validator.AddRule(
            Expression.Lambda<Func<T, CancellationToken, ValueTask<bool>>>(
                checkCall,
                param,
                cancelToken),
            memberName,
            $"{memberName} must be unique");
    }

    /// <summary>
    /// Validates that a reference exists in a data store.
    /// </summary>
    public static void Exists<T, TKey>(
        this AsyncExpressionValidator<T> validator,
        Expression<Func<T, TKey>> propertyExpression,
        Func<TKey, CancellationToken, ValueTask<bool>> existsCheckFunc)
    {
        var memberName = GetMemberName(propertyExpression);
        var param = Expression.Parameter(typeof(T), "e");
        var cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
        
        var property = Expression.Invoke(propertyExpression, param);
        
        var checkCall = Expression.Invoke(
            Expression.Constant(existsCheckFunc),
            property,
            cancelToken);

        validator.AddRule(
            Expression.Lambda<Func<T, CancellationToken, ValueTask<bool>>>(
                checkCall,
                param,
                cancelToken),
            memberName,
            $"{memberName} must exist");
    }

    private static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new ArgumentException("Expression must be a member expression");
    }
}