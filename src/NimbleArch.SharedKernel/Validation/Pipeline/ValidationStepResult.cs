using NimbleArch.SharedKernel.Validation.Exception;

namespace NimbleArch.SharedKernel.Validation.Pipeline;

// NimbleArch.SharedKernel/Validation/Pipeline/ValidationStepResult.cs
/// <summary>
/// Represents the result of a validation step execution.
/// </summary>
/// <remarks>
/// EN: Contains the validation results of a single pipeline step and controls
/// the pipeline flow. This struct is designed to be immutable and lightweight,
/// optimized for passing between pipeline steps.
///
/// TR: Tek bir pipeline adımının doğrulama sonuçlarını içerir ve pipeline
/// akışını kontrol eder. Bu struct, değişmez ve hafif olacak şekilde tasarlanmıştır,
/// pipeline adımları arasında geçiş için optimize edilmiştir.
/// </remarks>
public readonly struct ValidationStepResult(bool continuePipeline, IEnumerable<ValidationError> errors = null)
{
    /// <summary>
    /// Gets whether the validation pipeline should continue to the next step.
    /// </summary>
    /// <remarks>
    /// EN: Indicates if the pipeline should proceed to the next step.
    /// False means the pipeline should be terminated immediately.
    ///
    /// TR: Pipeline'ın bir sonraki adıma geçip geçmemesi gerektiğini belirtir.
    /// False, pipeline'ın hemen sonlandırılması gerektiği anlamına gelir.
    /// </remarks>
    public bool ContinuePipeline { get; } = continuePipeline;

    /// <summary>
    /// Gets the collection of validation errors from this step.
    /// </summary>
    /// <remarks>
    /// EN: Contains any validation errors produced by this step.
    /// Uses an immutable list to prevent modifications after creation.
    ///
    /// TR: Bu adımın ürettiği doğrulama hatalarını içerir.
    /// Oluşturulduktan sonra değişiklikleri önlemek için değişmez liste kullanır.
    /// </remarks>
    public IReadOnlyList<ValidationError> Errors { get; } = (IReadOnlyList<ValidationError>)errors?.ToList() ?? Array.Empty<ValidationError>();

    /// <summary>
    /// Creates a successful result that continues the pipeline.
    /// </summary>
    /// <remarks>
    /// EN: Factory method for creating a success result with no errors.
    ///
    /// TR: Hata içermeyen bir başarı sonucu oluşturmak için fabrika metodu.
    /// </remarks>
    public static ValidationStepResult Success() => 
        new(true, Array.Empty<ValidationError>());

    /// <summary>
    /// Creates a failure result that stops the pipeline.
    /// </summary>
    /// <remarks>
    /// EN: Factory method for creating a failure result with specified errors.
    ///
    /// TR: Belirtilen hatalarla bir başarısızlık sonucu oluşturmak için fabrika metodu.
    /// </remarks>
    public static ValidationStepResult Failure(IEnumerable<ValidationError> errors) => 
        new(false, errors);
}