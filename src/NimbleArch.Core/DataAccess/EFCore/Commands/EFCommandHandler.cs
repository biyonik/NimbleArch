using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.DataAccess.Commands;

namespace NimbleArch.Core.DataAccess.EFCore.Commands;

/// <summary>
/// Base command handler implementation for Entity Framework Core.
/// </summary>
/// <remarks>
/// EN: Provides optimized command handling with EF Core, including change tracking
/// optimization, batch operations, and performance monitoring.
///
/// TR: EF Core ile optimize edilmiş komut işleme sağlar. Değişiklik takibi
/// optimizasyonu, toplu işlemler ve performans izleme içerir.
/// </remarks>
public abstract class EFCommandHandler<TCommand> : ICommandHandler<TCommand> 
    where TCommand : ICommand
{
    protected readonly DbContext Context;
    protected readonly ILogger Logger;
    private readonly Activity _activity;

    protected EFCommandHandler(
        DbContext context,
        ILogger logger)
    {
        Context = context;
        Logger = logger;
        _activity = new Activity(GetType().Name);
    }

    /// <summary>
    /// Handles the command with optimized EF Core operations.
    /// </summary>
    public async Task<CommandResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activity.Start();
        activity?.SetTag("command.id", command.CommandId);
        activity?.SetTag("command.type", typeof(TCommand).Name);

        try
        {
            // Optimize change tracking
            Context.ChangeTracker.AutoDetectChangesEnabled = false;
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var result = await ExecuteCommandAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                await Context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing command {CommandType}", typeof(TCommand).Name);
            return CommandResult.Failure(ex.Message);
        }
        finally
        {
            Context.ChangeTracker.AutoDetectChangesEnabled = true;
            Context.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Template method for command execution logic.
    /// </summary>
    protected abstract Task<CommandResult> ExecuteCommandAsync(
        TCommand command,
        CancellationToken cancellationToken);
}