using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Services;

/// <summary>
/// Defines data consistency checking operations.
/// </summary>
/// <remarks>
/// EN: Provides methods for checking data consistency and referential integrity.
/// Supports both in-memory and database-level consistency checks.
///
/// TR: Veri tutarlılığını ve referans bütünlüğünü kontrol etmek için metodlar sağlar.
/// Hem bellek içi hem de veritabanı seviyesinde tutarlılık kontrollerini destekler.
/// </remarks>
public interface IDataConsistencyChecker
{
    /// <summary>
    /// Checks data consistency for an entity.
    /// </summary>
    Task<ConsistencyCheckResult> CheckConsistencyAsync<T>(
        T entity,
        CancellationToken cancellationToken = default) where T : IHasRelations;
}