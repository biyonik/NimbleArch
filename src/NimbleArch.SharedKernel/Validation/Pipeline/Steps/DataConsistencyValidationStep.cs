using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Services;

namespace NimbleArch.SharedKernel.Validation.Pipeline.Steps;

/// <summary>
/// Validates data consistency and integrity.
/// </summary>
/// <remarks>
/// EN: Ensures data consistency across related entities and validates referential integrity.
/// This step can perform database checks and verify entity relationships.
///
/// TR: İlişkili varlıklar arasında veri tutarlılığını sağlar ve referans bütünlüğünü
/// doğrular. Bu adım, veritabanı kontrolleri yapabilir ve varlık ilişkilerini
/// doğrulayabilir.
/// </remarks>
public class DataConsistencyValidationStep<T>(IDataConsistencyChecker consistencyChecker) : IValidationStep<T>
    where T : IHasRelations
{
    /// <summary>
    /// Executes data consistency validation for the given entity.
    /// </summary>
    /// <remarks>
    /// EN: Checks for data consistency issues such as broken references or invalid relationships.
    /// Can perform both in-memory and database-level consistency checks.
    ///
    /// TR: Kırık referanslar veya geçersiz ilişkiler gibi veri tutarsızlığı sorunlarını
    /// kontrol eder. Hem bellek içi hem de veritabanı seviyesinde tutarlılık kontrollerini
    /// gerçekleştirebilir.
    /// </remarks>
    public async Task<ValidationStepResult> ExecuteAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        var consistencyResult = await consistencyChecker.CheckConsistencyAsync(
            entity,
            cancellationToken);

        if (!consistencyResult.IsConsistent)
        {
            return ValidationStepResult.Failure(
                consistencyResult.Inconsistencies.Select(i =>
                    new ValidationError(i.Entity, i.Message)));
        }

        return ValidationStepResult.Success();
    }
}