using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.SharedKernel.Validation.Pipeline;

/// <summary>
/// Represents a pipeline of validation steps.
/// </summary>
/// <remarks>
/// EN: Implements a validation pipeline that executes multiple validation steps
/// in sequence or parallel. Provides a fluent API for building the pipeline and
/// configuring its behavior. Thread-safe and optimized for performance.
///
/// TR: Birden çok doğrulama adımını sıralı veya paralel olarak yürüten bir
/// doğrulama pipeline'ı uygular. Pipeline'ı oluşturmak ve davranışını yapılandırmak
/// için akıcı bir API sağlar. Thread-safe ve performans için optimize edilmiştir.
/// </remarks>
public class ValidationPipeline<T>
{
    private readonly List<IValidationStep<T>> _steps = [];
    private bool _stopOnFirstFailure = true;
    private bool _executeInParallel = false;

    /// <summary>
    /// Adds a validation step to the pipeline.
    /// </summary>
    /// <remarks>
    /// EN: Adds a new validation step to be executed as part of this pipeline.
    /// Steps are executed in the order they are added unless parallel execution is enabled.
    ///
    /// TR: Bu pipeline'ın bir parçası olarak yürütülecek yeni bir doğrulama adımı ekler.
    /// Paralel yürütme etkinleştirilmedikçe adımlar eklendikleri sırayla yürütülür.
    /// </remarks>
    public ValidationPipeline<T> AddStep(IValidationStep<T> step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Configures whether to stop on first validation failure.
    /// </summary>
    /// <remarks>
    /// EN: When enabled, the pipeline stops at the first step that produces errors.
    /// When disabled, all steps are executed regardless of previous failures.
    ///
    /// TR: Etkinleştirildiğinde, pipeline hata üreten ilk adımda durur.
    /// Devre dışı bırakıldığında, önceki başarısızlıklardan bağımsız olarak
    /// tüm adımlar yürütülür.
    /// </remarks>
    public ValidationPipeline<T> StopOnFirstFailure(bool stop = true)
    {
        _stopOnFirstFailure = stop;
        return this;
    }

    /// <summary>
    /// Configures whether to execute steps in parallel.
    /// </summary>
    /// <remarks>
    /// EN: When enabled, all steps are executed concurrently using Task.WhenAll.
    /// This can improve performance but may use more system resources.
    ///
    /// TR: Etkinleştirildiğinde, tüm adımlar Task.WhenAll kullanılarak eşzamanlı
    /// olarak yürütülür. Bu performansı artırabilir ancak daha fazla sistem
    /// kaynağı kullanabilir.
    /// </remarks>
    public ValidationPipeline<T> ExecuteInParallel(bool parallel = true)
    {
        _executeInParallel = parallel;
        return this;
    }

    /// <summary>
    /// Executes the validation pipeline.
    /// </summary>
    /// <remarks>
    /// EN: Executes all validation steps according to the configured behavior.
    /// Collects and combines results from all steps into a final ValidationResult.
    ///
    /// TR: Yapılandırılmış davranışa göre tüm doğrulama adımlarını yürütür.
    /// Tüm adımlardan gelen sonuçları toplayıp nihai bir ValidationResult'a birleştirir.
    /// </remarks>
    public async Task<ValidationResult> ExecuteAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (_executeInParallel)
            return await ExecuteParallelAsync(entity, context, cancellationToken);
        
        return await ExecuteSequentialAsync(entity, context, cancellationToken);
    }

    private async Task<ValidationResult> ExecuteSequentialAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        foreach (var step in _steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await step.ExecuteAsync(entity, context, cancellationToken);
            errors.AddRange(result.Errors);

            if (!result.ContinuePipeline && _stopOnFirstFailure)
                break;
        }

        return new ValidationResult(errors);
    }

    private async Task<ValidationResult> ExecuteParallelAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken)
    {
        var tasks = _steps.Select(step => 
            step.ExecuteAsync(entity, context, cancellationToken));

        var results = await Task.WhenAll(tasks);
        var errors = results.SelectMany(r => r.Errors).ToList();

        return new ValidationResult(errors);
    }
}