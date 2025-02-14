using System.Linq.Expressions;

namespace NimbleArch.Core.DataAccess.Abstract.Specification;

/// <summary>
/// Defines a specification for querying data.
/// </summary>
/// <remarks>
/// EN: Represents a reusable query specification that encapsulates the query logic.
/// Uses Expression Trees for high-performance query generation.
///
/// TR: Yeniden kullanılabilir sorgu spesifikasyonunu temsil eder.
/// Yüksek performanslı sorgu oluşturma için Expression Tree'ler kullanır.
/// </remarks>
public interface IQuerySpecification<T>
{
    /// <summary>
    /// Gets the criteria for the query.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the include expressions for eager loading.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the ordering expressions.
    /// </summary>
    List<(Expression<Func<T, object>> KeySelector, bool Ascending)> OrderBy { get; }

    /// <summary>
    /// Gets the group by expression.
    /// </summary>
    Expression<Func<T, object>> GroupBy { get; }

    /// <summary>
    /// Gets the pagination settings.
    /// </summary>
    (int Skip, int Take)? Pagination { get; }
}