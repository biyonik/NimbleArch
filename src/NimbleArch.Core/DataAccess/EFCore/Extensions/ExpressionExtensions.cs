using System.Linq.Expressions;

namespace NimbleArch.Core.DataAccess.EFCore.Extensions;

/// <summary>
/// Extension methods for Expression manipulation.
/// </summary>
/// <remarks>
/// EN: Provides utility methods for working with expressions, particularly
/// for converting expressions to string paths for EF Core includes.
///
/// TR: Expression'larla çalışmak için yardımcı metodlar sağlar, özellikle
/// EF Core include'ları için expression'ları string path'lere dönüştürür.
/// </remarks>
public static class ExpressionExtensions
{
    /// <summary>
    /// Converts an include expression to its string path representation.
    /// </summary>
    public static string AsStringPath<T, TProperty>(
        this Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            var pathParts = new List<string>();
            do
            {
                pathParts.Add(memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }
            while (memberExpression != null);

            pathParts.Reverse();
            return string.Join(".", pathParts);
        }

        throw new ArgumentException("Expression must be a member expression");
    }
}