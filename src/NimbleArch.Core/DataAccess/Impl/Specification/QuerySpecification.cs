using System.Linq.Expressions;
using NimbleArch.Core.DataAccess.Abstract.Specification;

namespace NimbleArch.Core.DataAccess.Impl.Specification;

/// <summary>
/// Base implementation of query specification pattern.
/// </summary>
/// <remarks>
/// EN: Provides a base implementation for building type-safe, reusable queries
/// with support for includes, ordering, grouping and pagination. Uses Expression Trees
/// for optimal query generation.
///
/// TR: Includes, sıralama, gruplama ve sayfalama desteği ile tip güvenli, yeniden
/// kullanılabilir sorgular oluşturmak için temel bir implementasyon sağlar. Optimal
/// sorgu oluşturma için Expression Tree'ler kullanır.
/// </remarks>
public class QuerySpecification<T> : IQuerySpecification<T>
{
   private readonly List<Expression<Func<T, object>>> _includes = new();
   private readonly List<(Expression<Func<T, object>> KeySelector, bool Ascending)> _orderBy = new();

   public Expression<Func<T, bool>>? Criteria { get; private set; }
   public List<Expression<Func<T, object>>> Includes => _includes;
   public List<(Expression<Func<T, object>>, bool)> OrderBy => _orderBy;
   public Expression<Func<T, object>> GroupBy { get; private set; }
   public (int Skip, int Take)? Pagination { get; private set; }

   /// <summary>
   /// Adds a criteria to the query using an expression.
   /// </summary>
   protected void AddCriteria(Expression<Func<T, bool>> criteria)
   {
       if (Criteria == null)
       {
           Criteria = criteria;
       }
       else
       {
           var parameter = Expression.Parameter(typeof(T), "x");
           var combined = Expression.AndAlso(
               Expression.Invoke(Criteria, parameter),
               Expression.Invoke(criteria, parameter));
           Criteria = Expression.Lambda<Func<T, bool>>(combined, parameter);
       }
   }

   /// <summary>
   /// Adds an include expression for eager loading.
   /// </summary>
   protected void AddInclude(Expression<Func<T, object>> includeExpression)
   {
       _includes.Add(includeExpression);
   }

   /// <summary>
   /// Adds an ordering expression.
   /// </summary>
   protected void AddOrderBy(Expression<Func<T, object>> orderByExpression, bool ascending = true)
   {
       _orderBy.Add((orderByExpression, ascending));
   }

   /// <summary>
   /// Sets the group by expression.
   /// </summary>
   protected void SetGroupBy(Expression<Func<T, object>> groupByExpression)
   {
       GroupBy = groupByExpression;
   }

   /// <summary>
   /// Sets the pagination parameters.
   /// </summary>
   protected void SetPagination(int skip, int take)
   {
       Pagination = (skip, take);
   }
}