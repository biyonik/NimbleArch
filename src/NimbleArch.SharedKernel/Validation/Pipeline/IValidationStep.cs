using NimbleArch.SharedKernel.Validation.Base;

namespace NimbleArch.SharedKernel.Validation.Pipeline;

/// <summary>
/// Represents a step in the validation pipeline.
/// </summary>
/// <remarks>
/// EN: Defines a contract for validation steps that can be chained together in a pipeline.
/// Each step can perform its own validation and decide whether to continue the pipeline.
/// Supports both synchronous and asynchronous validation operations.
///
/// TR: Bir doğrulama pipeline'ında zincirleme şekilde birleştirilebilen doğrulama
/// adımları için bir sözleşme tanımlar. Her adım kendi doğrulamasını gerçekleştirebilir
/// ve pipeline'ın devam edip etmeyeceğine karar verebilir. Hem senkron hem de asenkron
/// doğrulama işlemlerini destekler.
/// </remarks>
/// <typeparam name="T">
/// EN: The type of entity being validated
/// TR: Doğrulanan varlığın tipi
/// </typeparam>
public interface IValidationStep<T>
{
    /// <summary>
    /// Executes the validation step.
    /// </summary>
    /// <remarks>
    /// EN: Performs the validation logic specific to this step. Can access both the entity
    /// and the validation context. Returns a ValidationStepResult that indicates whether
    /// the pipeline should continue and contains any validation errors.
    ///
    /// TR: Bu adıma özgü doğrulama mantığını gerçekleştirir. Hem varlığa hem de doğrulama
    /// bağlamına erişebilir. Pipeline'ın devam edip etmemesi gerektiğini belirten ve
    /// varsa doğrulama hatalarını içeren bir ValidationStepResult döndürür.
    /// </remarks>
    /// <param name="entity">
    /// EN: The entity to validate
    /// TR: Doğrulanacak varlık
    /// </param>
    /// <param name="context">
    /// EN: The validation context
    /// TR: Doğrulama bağlamı
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancellation token for cancelling the operation
    /// TR: İşlemi iptal etmek için iptal token'ı
    /// </param>
    Task<ValidationStepResult> ExecuteAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default);
}