namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Represents a command operation.
/// </summary>
/// <remarks>
/// EN: Base interface for all command operations. Commands represent state-changing
/// operations and are optimized for write performance.
///
/// TR: Tüm komut operasyonları için temel arayüz. Komutlar durum değiştiren
/// operasyonları temsil eder ve yazma performansı için optimize edilmiştir.
/// </remarks>
public interface ICommand
{
    /// <summary>
    /// Gets the unique identifier for the command.
    /// </summary>
    Guid CommandId { get; }

    /// <summary>
    /// Gets the timestamp when the command was created.
    /// </summary>
    DateTime Timestamp { get; }
}