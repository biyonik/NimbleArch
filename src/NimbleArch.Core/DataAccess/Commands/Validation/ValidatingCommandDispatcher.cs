using Microsoft.Extensions.Logging;
using NimbleArch.SharedKernel.Validation.Abstract;
using NimbleArch.SharedKernel.Validation.Base;
using NimbleArch.SharedKernel.Validation.Exception;
using NimbleArch.SharedKernel.Validation.Result;

namespace NimbleArch.Core.DataAccess.Commands.Validation;

/// <summary>
/// Command dispatcher with validation support.
/// </summary>
/// <remarks>
/// EN: Decorator for command dispatcher that adds validation capabilities.
/// Validates commands before dispatching them to their handlers.
///
/// TR: Komut dağıtıcıya doğrulama yetenekleri ekleyen dekoratör.
/// Komutları işleyicilerine göndermeden önce doğrular.
/// </remarks>
public class ValidatingCommandDispatcher(
    ICommandDispatcher inner,
    IServiceProvider serviceProvider,
    ILogger<ValidatingCommandDispatcher> logger)
    : ICommandDispatcher
{
    public async Task<CommandResult> DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        if (command is IValidatableCommand validatable)
        {
            var validationResult = await ValidateCommandAsync(command, validatable.ValidationGroup, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Failure(
                    $"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        return await inner.DispatchAsync(command, cancellationToken);
    }

    public async Task<BulkCommandResult> DispatchBulkAsync<TCommand>(
        IEnumerable<TCommand> commands,
        CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        if (typeof(IValidatableCommand).IsAssignableFrom(typeof(TCommand)))
        {
            var commandArray = commands.ToArray();
            var validationTasks = commandArray
                .Cast<IValidatableCommand>()
                .Select(cmd => ValidateCommandAsync(cmd, cmd.ValidationGroup, cancellationToken));

            var validationResults = await Task.WhenAll(validationTasks);
            var invalidCommands = validationResults
                .Zip(commandArray, (result, cmd) => (cmd.CommandId, Result: result))
                .Where(x => !x.Result.IsValid)
                .ToDictionary(
                    x => x.CommandId,
                    x => string.Join(", ", x.Result.Errors.Select(e => e.ErrorMessage)));

            if (invalidCommands.Any())
            {
                return BulkCommandResult.Partial(
                    0,
                    invalidCommands,
                    Array.Empty<object>()
                );
            }
        }

        return await inner.DispatchBulkAsync(commands, cancellationToken);
    }

    private async Task<ValidationResult> ValidateCommandAsync<TCommand>(
        TCommand command,
        ValidationGroup validationGroup,
        CancellationToken cancellationToken) where TCommand : ICommand
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TCommand));
        var validator = serviceProvider.GetService(validatorType) as IValidator;

        if (validator == null)
        {
            logger.LogWarning("No validator found for command type {CommandType}", typeof(TCommand).Name);
            return new ValidationResult(Array.Empty<ValidationError>());
        }

        var context = new ValidationContext.Builder()
            .WithEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            .WithServices(serviceProvider)
            .Build();

        return await validator.ValidateAsync(command, context, cancellationToken);
    }
}