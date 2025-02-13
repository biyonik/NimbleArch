using System.Linq.Expressions;
using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Impl;

namespace NimbleArch.SharedKernel.Validation.Extensions;

public static class GroupedValidatorExtensions
{
    /// <summary>
    /// Adds a NotEmpty rule for Create operations.
    /// </summary>
    public static void NotEmptyOnCreate<T>(
        this GroupedExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression)
    {
        var memberName = GetMemberName(propertyExpression);
        AddNotEmptyRule(validator, propertyExpression, memberName, ValidationGroup.Default.Create);
    }

    /// <summary>
    /// Adds a NotEmpty rule for Update operations.
    /// </summary>
    public static void NotEmptyOnUpdate<T>(
        this GroupedExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression)
    {
        var memberName = GetMemberName(propertyExpression);
        AddNotEmptyRule(validator, propertyExpression, memberName, ValidationGroup.Default.Update);
    }
    
    /// <summary>
    /// Adds a not-empty validation rule for a string property to the specified validation group.
    /// </summary>
    /// <remarks>
    /// EN: Creates an expression tree that checks if a string property is null or empty.
    /// The validation is compiled once and cached for subsequent validations.
    ///
    /// TR: Bir string özelliğinin null veya boş olup olmadığını kontrol eden bir expression tree oluşturur.
    /// Doğrulama bir kez derlenir ve sonraki doğrulamalar için önbelleğe alınır.
    /// </remarks>
    private static void AddNotEmptyRule<T>(
        GroupedExpressionValidator<T> validator,
        Expression<Func<T, string>> propertyExpression,
        string memberName,
        ValidationGroup group)
    {
        // Parameter for the entity
        var parameter = Expression.Parameter(typeof(T), "entity");
        
        // Access the property: entity.Property
        var propertyAccess = Expression.Invoke(propertyExpression, parameter);
        
        // Check if the value is null
        var nullCheck = Expression.ReferenceEqual(propertyAccess, Expression.Constant(null, typeof(string)));
        
        // Check if the value is empty: value == string.Empty
        var emptyCheck = Expression.Equal(
            propertyAccess,
            Expression.Constant(string.Empty, typeof(string)));
        
        // Combine checks: !(value == null || value == string.Empty)
        var notEmptyExpression = Expression.Not(
            Expression.OrElse(nullCheck, emptyCheck));
        
        // Create the final lambda: entity => !(entity.Property == null || entity.Property == string.Empty)
        var lambda = Expression.Lambda<Func<T, bool>>(
            notEmptyExpression,
            parameter);

        // Add the rule to the validator
        validator.AddRule(
            lambda,
            memberName,
            $"{memberName} cannot be empty",
            group);
    }
    
    /// <summary>
    /// Extracts the member name from a property expression.
    /// </summary>
    /// <remarks>
    /// EN: Safely extracts property name from expression tree, handling nested properties.
    /// Throws if the expression is not a valid member access.
    ///
    /// TR: Expression tree'den özellik adını güvenli bir şekilde çıkarır, iç içe özellikleri destekler.
    /// Eğer expression geçerli bir üye erişimi değilse hata fırlatır.
    /// </remarks>
    private static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new ArgumentException("Expression must be a member expression", nameof(expression));
    }
}