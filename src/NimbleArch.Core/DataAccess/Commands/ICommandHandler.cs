namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Defines contract for handling commands.
/// </summary>
/// <remarks>
/// EN: Interface for command handlers that process state-changing operations.
/// Supports optimistic concurrency and batch operations.
///
/// TR: Durum değiştiren operasyonları işleyen komut işleyicileri için arayüz.
/// İyimser eşzamanlılık ve toplu işlemleri destekler.
/// </remarks>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles the command execution.
    /// </summary>
    Task<CommandResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}