using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Interfaces;
using NimbleArch.SharedKernel.Validation.Services;

namespace NimbleArch.SharedKernel.Validation.Pipeline.Steps;

/// <summary>
/// Validates complex business rules for entities.
/// </summary>
/// <remarks>
/// EN: Executes domain-specific business rules validation. This step is customizable
/// to handle various business requirements and can integrate with domain services.
///
/// TR: Alan-spesifik iş kuralları doğrulamasını yürütür. Bu adım, çeşitli iş
/// gereksinimlerini ele alacak şekilde özelleştirilebilir ve domain servisleriyle
/// entegre çalışabilir.
/// </remarks>
public class BusinessRuleValidationStep<T>(IBusinessRuleEngine ruleEngine) : IValidationStep<T>
    where T : IHasBusinessRules
{
    /// <summary>
    /// Executes business rule validation for the given entity.
    /// </summary>
    /// <remarks>
    /// EN: Evaluates all business rules associated with the entity.
    /// Supports complex validation scenarios involving multiple business rules.
    ///
    /// TR: Varlıkla ilişkili tüm iş kurallarını değerlendirir.
    /// Birden fazla iş kuralı içeren karmaşık doğrulama senaryolarını destekler.
    /// </remarks>
    public async Task<ValidationStepResult> ExecuteAsync(
        T entity,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        var ruleResults = await ruleEngine.EvaluateAsync(
            entity,
            context,
            cancellationToken);

        if (!ruleResults.IsValid)
        {
            return ValidationStepResult.Failure(
                ruleResults.Violations.Select(v => 
                    new ValidationError(v.Rule, v.Message)));
        }

        return ValidationStepResult.Success();
    }
}