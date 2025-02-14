namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Defines contract for handling bulk operations.
/// </summary>
/// <remarks>
/// EN: Interface for handlers that process multiple entities in an optimized way.
/// Uses batching and minimal database roundtrips for maximum performance.
///
/// TR: Birden çok varlığı optimize edilmiş şekilde işleyen işleyiciler için arayüz.
/// Maksimum performans için batch işleme ve minimum veritabanı round-trip kullanır.
/// </remarks>
public interface IBulkCommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles bulk command execution.
    /// </summary>
    Task<BulkCommandResult> HandleAsync(
        IEnumerable<TCommand> commands,
        CancellationToken cancellationToken = default);
}