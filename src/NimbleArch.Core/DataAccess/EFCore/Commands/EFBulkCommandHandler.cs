using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.DataAccess.Commands;

namespace NimbleArch.Core.DataAccess.EFCore.Commands;

/// <summary>
/// Base bulk command handler implementation for Entity Framework Core.
/// </summary>
/// <remarks>
/// EN: Provides high-performance bulk operations using batching and minimal
/// database roundtrips. Includes optimized change tracking and error handling.
///
/// TR: Batch işleme ve minimum veritabanı round-trip kullanarak yüksek performanslı
/// toplu işlemler sağlar. Optimize edilmiş değişiklik takibi ve hata yönetimi içerir.
/// </remarks>
public abstract class EFBulkCommandHandler<TCommand> : IBulkCommandHandler<TCommand>
    where TCommand : ICommand
{
    protected readonly DbContext Context;
    protected readonly ILogger Logger;
    private readonly Activity _activity;
    private readonly int _batchSize;

    protected EFBulkCommandHandler(
        DbContext context,
        ILogger logger,
        int batchSize = 1000)
    {
        Context = context;
        Logger = logger;
        _activity = new Activity(GetType().Name);
        _batchSize = batchSize;
    }

    /// <summary>
    /// Handles bulk commands with optimized batch processing.
    /// </summary>
    public async Task<BulkCommandResult> HandleAsync(
        IEnumerable<TCommand> commands,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activity.Start();
        var commandArray = commands.ToArray();
        activity?.SetTag("commands.count", commandArray.Length);

        var successCount = 0;
        var errors = new Dictionary<Guid, string>();
        var affectedIds = new List<object>();

        try
        {
            // Optimize change tracking
            Context.ChangeTracker.AutoDetectChangesEnabled = false;
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Process in batches
            foreach (var batch in commandArray.Chunk(_batchSize))
            {
                var batchResults = await ExecuteBatchAsync(batch, cancellationToken);
                
                foreach (var result in batchResults)
                {
                    if (result.Result.IsSuccess)
                    {
                        successCount++;
                        affectedIds.AddRange(result.Result.AffectedIds);
                    }
                    else
                    {
                        errors[result.CommandId] = result.Result.ErrorMessage;
                    }
                }

                // Save changes after each batch
                await Context.SaveChangesAsync(cancellationToken);
                Context.ChangeTracker.Clear();
            }

            return BulkCommandResult.Partial(successCount, errors, affectedIds);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing bulk command {CommandType}", typeof(TCommand).Name);
            return BulkCommandResult.Partial(
                successCount: successCount,
                errors: errors,
                affectedIds: affectedIds);
        }
        finally
        {
            Context.ChangeTracker.AutoDetectChangesEnabled = true;
            Context.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Template method for batch execution logic.
    /// </summary>
    protected abstract Task<IEnumerable<(Guid CommandId, CommandResult Result)>> ExecuteBatchAsync(
        TCommand[] batch,
        CancellationToken cancellationToken);
}