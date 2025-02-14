using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Default implementation of command dispatcher.
/// </summary>
/// <remarks>
/// EN: Implements command routing and execution with support for middleware pipeline,
/// logging, and performance monitoring. Handles command handler resolution and lifecycle.
///
/// TR: Middleware pipeline, loglama ve performans izleme desteğiyle komut yönlendirme
/// ve yürütmeyi implement eder. Komut işleyici çözümleme ve yaşam döngüsünü yönetir.
/// </remarks>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandDispatcher> _logger;
    private readonly Activity _activity;

    public CommandDispatcher(
        IServiceProvider serviceProvider,
        ILogger<CommandDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _activity = new Activity(nameof(CommandDispatcher));
    }

    public async Task<CommandResult> DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        using var activity = _activity.Start();
        activity?.SetTag("command.type", typeof(TCommand).Name);
        activity?.SetTag("command.id", command.CommandId);

        try
        {
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
            if (handler == null)
            {
                var error = $"No handler found for command type {typeof(TCommand).Name}";
                _logger.LogError(error);
                return CommandResult.Failure(error);
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            LogCommandResult(command, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching command {CommandType}", typeof(TCommand).Name);
            return CommandResult.Failure(ex.Message);
        }
    }

    public async Task<BulkCommandResult> DispatchBulkAsync<TCommand>(
        IEnumerable<TCommand> commands,
        CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        using var activity = _activity.Start();
        var commandArray = commands.ToArray();
        activity?.SetTag("command.type", typeof(TCommand).Name);
        activity?.SetTag("commands.count", commandArray.Length);

        try
        {
            var handler = _serviceProvider.GetService<IBulkCommandHandler<TCommand>>();
            if (handler == null)
            {
                // Fall back to single command handler if bulk handler is not available
                return await ExecuteIndividually(commandArray, cancellationToken);
            }

            var result = await handler.HandleAsync(commandArray, cancellationToken);
            LogBulkCommandResult(typeof(TCommand).Name, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching bulk command {CommandType}", typeof(TCommand).Name);
            
            return BulkCommandResult.Partial(
                0,
                commandArray.ToDictionary(c => c.CommandId, _ => ex.Message), Array.Empty<object>());
        }
    }

    private async Task<BulkCommandResult> ExecuteIndividually<TCommand>(
        TCommand[] commands,
        CancellationToken cancellationToken) where TCommand : ICommand
    {
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
        if (handler == null)
        {
            var error = $"No handler found for command type {typeof(TCommand).Name}";
            _logger.LogError(error);

            return BulkCommandResult.Partial(0, commands.ToDictionary(c => c.CommandId, _ => error),
                Array.Empty<object>());
        }

        var successCount = 0;
        var errors = new Dictionary<Guid, string>();
        var affectedIds = new List<object>();

        foreach (var command in commands)
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                successCount++;
                affectedIds.AddRange(result.AffectedIds);
            }
            else
            {
                errors[command.CommandId] = result.ErrorMessage;
            }
        }

        return BulkCommandResult.Partial(successCount: successCount, errors, affectedIds);
    }

    private void LogCommandResult<TCommand>(TCommand command, CommandResult result)
        where TCommand : ICommand
    {
        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Command {CommandType} ({CommandId}) executed successfully. Affected IDs: {@AffectedIds}",
                typeof(TCommand).Name,
                command.CommandId,
                result.AffectedIds);
        }
        else
        {
            _logger.LogWarning(
                "Command {CommandType} ({CommandId}) failed: {Error}",
                typeof(TCommand).Name,
                command.CommandId,
                result.ErrorMessage);
        }
    }

    private void LogBulkCommandResult(string commandType, BulkCommandResult result)
    {
        _logger.LogInformation(
            "Bulk command {CommandType} executed. Success: {SuccessCount}, Failed: {FailureCount}, Affected IDs: {@AffectedIds}",
            commandType,
            result.SuccessCount,
            result.FailureCount,
            result.AffectedIds);

        if (result.FailureCount > 0)
        {
            _logger.LogWarning(
                "Bulk command {CommandType} had failures: {@Errors}",
                commandType,
                result.Errors);
        }
    }
}