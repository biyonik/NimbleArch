namespace NimbleArch.Core.EventSourcing.Store;

/// <summary>
/// Lock-free ring buffer implementation for high-performance event caching.
/// </summary>
/// <remarks>
/// EN: Provides a fixed-size, lock-free circular buffer for caching recent events.
/// Uses memory fencing and atomic operations for thread safety.
///
/// TR: Yakın zamandaki olayları önbelleğe almak için sabit boyutlu, kilit gerektirmeyen
/// dairesel bir tampon sağlar. Thread güvenliği için bellek bariyerleri ve atomik
/// işlemler kullanır.
/// </remarks>
public class RingBuffer<T>
{
   private readonly T[] _buffer;
   private readonly int _mask;
   private long _writePosition;
   private long _readPosition;

   /// <summary>
   /// Gets the number of items currently in the buffer.
   /// </summary>
   public long Count => _writePosition - _readPosition;

   /// <summary>
   /// Gets the total capacity of the buffer.
   /// </summary>
   public int Capacity => _buffer.Length;

   public RingBuffer(int size)
   {
       // Size'ı 2'nin katına yuvarla
       var capacity = RoundUpToPowerOf2(size);
       _buffer = new T[capacity];
       _mask = capacity - 1;
       _writePosition = 0;
       _readPosition = 0;
   }

   /// <summary>
   /// Writes an item to the buffer.
   /// </summary>
   /// <remarks>
   /// EN: Writes an item to the next available position. If the buffer is full,
   /// overwrites the oldest item. Uses memory barriers for thread safety.
   ///
   /// TR: Bir öğeyi kullanılabilir bir sonraki konuma yazar. Tampon doluysa,
   /// en eski öğenin üzerine yazar. Thread güvenliği için bellek bariyerleri kullanır.
   /// </remarks>
   public void Write(T item)
   {
       var currentWrite = Interlocked.Increment(ref _writePosition) - 1;
       _buffer[currentWrite & _mask] = item;
       
       // Eğer buffer doluysa, read position'ı ilerlet
       while (_writePosition - _readPosition > _buffer.Length)
       {
           Interlocked.Increment(ref _readPosition);
       }
   }

   /// <summary>
   /// Attempts to read an item from the buffer.
   /// </summary>
   /// <remarks>
   /// EN: Tries to read the next available item without removing it.
   /// Returns false if no items are available.
   ///
   /// TR: Bir sonraki kullanılabilir öğeyi kaldırmadan okumaya çalışır.
   /// Öğe yoksa false döner.
   /// </remarks>
   public bool TryPeek(long sequence, out T item)
   {
       item = default;
       
       if (sequence < _readPosition || sequence >= _writePosition)
           return false;

       item = _buffer[sequence & _mask];
       return true;
   }

   /// <summary>
   /// Gets a range of items from the buffer.
   /// </summary>
   /// <remarks>
   /// EN: Returns a sequence of items from the specified position.
   /// Skips unavailable items.
   ///
   /// TR: Belirtilen pozisyondan başlayan öğe dizisini döndürür.
   /// Kullanılamayan öğeleri atlar.
   /// </remarks>
   public IEnumerable<T> GetRange(long fromSequence, int maxItems)
   {
       var count = 0;
       var currentSequence = Math.Max(fromSequence, _readPosition);
       
       while (count < maxItems && currentSequence < _writePosition)
       {
           if (TryPeek(currentSequence, out var item))
           {
               yield return item;
               count++;
           }
           currentSequence++;
       }
   }

   /// <summary>
   /// Gets all available items from the buffer.
   /// </summary>
   /// <remarks>
   /// EN: Returns all items currently in the buffer without removing them.
   ///
   /// TR: Tamponda bulunan tüm öğeleri kaldırmadan döndürür.
   /// </remarks>
   public IEnumerable<T> GetAll()
   {
       return GetRange(_readPosition, _buffer.Length);
   }

   /// <summary>
   /// Clears all items from the buffer.
   /// </summary>
   /// <remarks>
   /// EN: Resets the buffer to its initial empty state.
   ///
   /// TR: Tamponu başlangıç boş durumuna sıfırlar.
   /// </remarks>
   public void Clear()
   {
       _writePosition = 0;
       _readPosition = 0;
       Array.Clear(_buffer, 0, _buffer.Length);
   }

   private static int RoundUpToPowerOf2(int value)
   {
       var result = 1;
       while (result < value)
       {
           result *= 2;
       }
       return result;
   }
}