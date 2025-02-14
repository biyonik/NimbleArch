namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Defines the contract for command dispatching.
/// </summary>
/// <remarks>
/// EN: Provides centralized command dispatching with support for command routing,
/// decorators, and middleware. Handles both single and bulk command operations.
///
/// TR: Komut yönlendirme, dekoratörler ve middleware desteğiyle merkezi komut
/// dağıtımı sağlar. Hem tekil hem de toplu komut işlemlerini yönetir.
/// </remarks>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a single command for execution.
    /// </summary>
    Task<CommandResult> DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default) where TCommand : ICommand;

    /// <summary>
    /// Dispatches multiple commands for bulk execution.
    /// </summary>
    Task<BulkCommandResult> DispatchBulkAsync<TCommand>(
        IEnumerable<TCommand> commands,
        CancellationToken cancellationToken = default) where TCommand : ICommand;
}