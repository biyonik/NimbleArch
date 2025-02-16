using System.Linq.Expressions;

namespace NimbleArch.Infrastructure.Data.Extensions;

public static class SpecificationExtensions
{
    public static IOrderedQueryable<T> ApplyThenBy<T>(
        this IOrderedQueryable<T> query,
        Expression<Func<T, object>> keySelector,
        bool ascending)
    {
        return ascending
            ? query.ThenBy(keySelector)
            : query.ThenByDescending(keySelector);
    }
}