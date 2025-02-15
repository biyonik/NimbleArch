namespace NimbleArch.Core.Caching.MemoryMapped;

/// <summary>
/// Represents a free space entry in the memory-mapped file.
/// </summary>
/// <remarks>
/// EN: Used for tracking available space in the memory-mapped file.
/// Supports efficient space allocation and defragmentation.
///
/// TR: Belleğe eşlenmiş dosyada kullanılabilir alanı takip etmek için kullanılır.
/// Verimli alan tahsisi ve birleştirmeyi destekler.
/// </remarks>
public class FreeSpaceEntry
{
    /// <summary>
    /// Gets or sets the position of free space.
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// Gets or sets the size of free space.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the next entry in the free space list.
    /// </summary>
    public FreeSpaceEntry Next { get; set; }
}