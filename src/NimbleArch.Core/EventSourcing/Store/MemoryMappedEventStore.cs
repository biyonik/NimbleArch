using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.EventSourcing.Events;


namespace NimbleArch.Core.EventSourcing.Store;

/// <summary>
/// High-performance memory-mapped event store implementation.
/// </summary>
public sealed class MemoryMappedEventStore : IEventStore, IDisposable
{
   private const int EVENT_HEADER_SIZE = 128; // Fixed size for event header
   private readonly string _filePath;
   private readonly MemoryMappedFile _eventFile;
   private readonly ReaderWriterLockSlim _lock;
   private readonly ILogger<MemoryMappedEventStore> _logger;
   private long _currentPosition;
   
   // Index yapıları
   private readonly ConcurrentDictionary<Guid, List<long>> _aggregateIndex;
   private readonly ConcurrentDictionary<string, List<long>> _eventTypeIndex;
   private readonly RingBuffer<EventIndexEntry> _sequenceIndex;

   public MemoryMappedEventStore(
       string filePath,
       long initialSize,
       ILogger<MemoryMappedEventStore> logger)
   {
       _filePath = filePath;
       _logger = logger;
       _lock = new ReaderWriterLockSlim();
       _aggregateIndex = new ConcurrentDictionary<Guid, List<long>>();
       _eventTypeIndex = new ConcurrentDictionary<string, List<long>>();
       _sequenceIndex = new RingBuffer<EventIndexEntry>(1_000_000); // 1M events in memory

       // Event dosyasını oluştur veya aç
       _eventFile = MemoryMappedFile.CreateFromFile(
           _filePath,
           FileMode.OpenOrCreate,
           null,
           initialSize);

       // Mevcut olayları indexle
       InitializeFromExistingEvents();
   }

   public ValueTask AppendEventAsync(
       EventDescriptor eventDescriptor,
       CancellationToken cancellationToken = default)
   {
       _lock.EnterWriteLock();
       try
       {
           var position = _currentPosition;
           var totalSize = EVENT_HEADER_SIZE + eventDescriptor.Data.Length;

           using var accessor = _eventFile.CreateViewAccessor(position, totalSize);
        
           // Event header'ı yaz
           WriteEventHeader(accessor, 0, eventDescriptor);
        
           // Event verisini yaz
           var buffer = new byte[eventDescriptor.Data.Length];
           eventDescriptor.Data.CopyTo(buffer);
           accessor.WriteArray(EVENT_HEADER_SIZE, buffer, 0, buffer.Length);

           // Indexleri güncelle
           UpdateIndexes(eventDescriptor, position);

           _currentPosition += totalSize;
       }
       finally
       {
           _lock.ExitWriteLock();
       }

       return ValueTask.CompletedTask;
   }

   public IAsyncEnumerable<EventDescriptor> GetEventsAsync(
       Guid aggregateId,
       long fromVersion = 0,
       CancellationToken cancellationToken = default)
   {
       if (!_aggregateIndex.TryGetValue(aggregateId, out var positions))
           return AsyncEnumerable.Empty<EventDescriptor>();

       return GetEventsFromPositionsAsync(positions, fromVersion, cancellationToken);
   }

   public ValueTask<long> GetLastSequenceAsync(CancellationToken cancellationToken = default)
   {
       return new ValueTask<long>(_sequenceIndex.Count);
   }

   private async IAsyncEnumerable<EventDescriptor> GetEventsFromPositionsAsync(
       List<long> positions,
       long fromVersion,
       [EnumeratorCancellation] CancellationToken cancellationToken)
   {
       foreach (var position in positions)
       {
           cancellationToken.ThrowIfCancellationRequested();

           using var accessor = _eventFile.CreateViewAccessor(position, EVENT_HEADER_SIZE);
           var header = ReadEventHeader(accessor, 0);

           if (header.Version < fromVersion)
               continue;

           var data = new byte[header.DataLength];
           accessor.ReadArray(EVENT_HEADER_SIZE, data, 0, data.Length);

           yield return new EventDescriptor(
               header.EventId,
               header.Sequence,
               header.EventType,
               header.Timestamp,
               header.AggregateType,
               header.AggregateId,
               header.Version,
               data,
               header.Metadata);
       }
   }

   private void UpdateIndexes(EventDescriptor eventDescriptor, long position)
   {
       // Aggregate index güncelleme
       _aggregateIndex.AddOrUpdate(
           eventDescriptor.AggregateId,
           new List<long> { position },
           (_, list) =>
           {
               list.Add(position);
               return list;
           });

       // Event type index güncelleme
       _eventTypeIndex.AddOrUpdate(
           eventDescriptor.EventType,
           new List<long> { position },
           (_, list) =>
           {
               list.Add(position);
               return list;
           });

       // Sequence index güncelleme
       _sequenceIndex.Write(new EventIndexEntry(
           eventDescriptor.Sequence,
           position));
   }

   private readonly struct EventHeader
   {
       public Guid EventId { get; init; }
       public long Sequence { get; init; }
       public int DataLength { get; init; }
       public string EventType { get; init; }
       public long Timestamp { get; init; }
       public string AggregateType { get; init; }
       public Guid AggregateId { get; init; }
       public long Version { get; init; }
       public IReadOnlyDictionary<string, string> Metadata { get; init; }
   }

   private readonly struct EventIndexEntry(long sequence, long position)
   {
       public long Sequence { get; } = sequence;
       public long Position { get; } = position;
   }

   /// <summary>
   /// Initializes indexes from existing events in the event store file.
   /// </summary>
   /// <remarks>
   /// EN: Reads all existing events from the memory-mapped file and rebuilds indexes.
   /// Uses a buffered approach for efficient reading of large event files.
   ///
   /// TR: Belleğe eşlenmiş dosyadan mevcut tüm olayları okur ve indeksleri yeniden oluşturur.
   /// Büyük olay dosyalarının verimli okunması için tamponlanmış bir yaklaşım kullanır.
   /// </remarks>
   private void InitializeFromExistingEvents()
   {
       try
       {
           long position = 0;
           using var accessor = _eventFile.CreateViewAccessor(0, _eventFile.Capacity);
       
           while (position < accessor.Capacity)
           {
               // Header'ı oku
               var header = ReadEventHeader(accessor, position);
               if (header.DataLength == 0) // Dosyanın sonuna geldik
                   break;

               // Index'leri güncelle
               var descriptor = new EventDescriptor(
                   header.EventId,
                   header.Sequence,
                   header.EventType,
                   header.Timestamp,
                   header.AggregateType,
                   header.AggregateId,
                   header.Version,
                   new byte[0], // Data'yı şu an okumaya gerek yok
                   header.Metadata);

               UpdateIndexes(descriptor, position);

               // Bir sonraki event'e geç
               position += EVENT_HEADER_SIZE + header.DataLength;
               _currentPosition = position;
           }
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error initializing from existing events");
           throw;
       }
   }

   /// <summary>
/// Reads event header from the memory-mapped file.
/// </summary>
/// <remarks>
/// EN: Deserializes event header information from the specified position.
/// Uses a fixed-size format for efficient reading and writing.
///
/// TR: Belirtilen pozisyondan olay başlık bilgilerini deserialize eder.
/// Verimli okuma ve yazma için sabit boyutlu format kullanır.
/// </remarks>
private static EventHeader ReadEventHeader(MemoryMappedViewAccessor accessor, long offset)
{
   var buffer = new byte[EVENT_HEADER_SIZE];
   accessor.ReadArray(offset, buffer, 0, EVENT_HEADER_SIZE);

   var position = 0;

   // Event ID (16 bytes)
   var eventId = new Guid(buffer.AsSpan(position, 16));
   position += 16;

   // Sequence (8 bytes)
   var sequence = BitConverter.ToInt64(buffer, position);
   position += 8;

   // Data Length (4 bytes)
   var dataLength = BitConverter.ToInt32(buffer, position);
   position += 4;

   // Event Type Length (4 bytes) ve Event Type
   var eventTypeLength = BitConverter.ToInt32(buffer, position);
   position += 4;
   var eventType = Encoding.UTF8.GetString(buffer, position, eventTypeLength);
   position += eventTypeLength;

   // Timestamp (8 bytes)
   var timestamp = BitConverter.ToInt64(buffer, position);
   position += 8;

   // Aggregate Type Length (4 bytes) ve Aggregate Type
   var aggTypeLength = BitConverter.ToInt32(buffer, position);
   position += 4;
   var aggregateType = Encoding.UTF8.GetString(buffer, position, aggTypeLength);
   position += aggTypeLength;

   // Aggregate ID (16 bytes)
   var aggregateId = new Guid(buffer.AsSpan(position, 16));
   position += 16;

   // Version (8 bytes)
   var version = BitConverter.ToInt64(buffer, position);
   position += 8;

   // Metadata
   var metadata = ReadMetadata(buffer, ref position);

   return new EventHeader
   {
       EventId = eventId,
       Sequence = sequence,
       DataLength = dataLength,
       EventType = eventType,
       Timestamp = timestamp,
       AggregateType = aggregateType,
       AggregateId = aggregateId,
       Version = version,
       Metadata = metadata
   };
}

/// <summary>
/// Writes event header to the memory-mapped file.
/// </summary>
/// <remarks>
/// EN: Serializes event header information to the specified position.
/// Uses fixed-size format and efficient byte array operations.
///
/// TR: Belirtilen pozisyona olay başlık bilgilerini serialize eder.
/// Sabit boyutlu format ve verimli byte dizisi işlemleri kullanır.
/// </remarks>
private static void WriteEventHeader(MemoryMappedViewAccessor accessor, long offset, EventDescriptor descriptor)
{
   var buffer = new byte[EVENT_HEADER_SIZE];
   var position = 0;

   // Event ID (16 bytes)
   descriptor.EventId.ToByteArray().CopyTo(buffer, position);
   position += 16;

   // Sequence (8 bytes)
   BitConverter.GetBytes(descriptor.Sequence).CopyTo(buffer, position);
   position += 8;

   // Data Length (4 bytes)
   BitConverter.GetBytes(descriptor.Data.Length).CopyTo(buffer, position);
   position += 4;

   // Event Type
   var eventTypeBytes = Encoding.UTF8.GetBytes(descriptor.EventType);
   BitConverter.GetBytes(eventTypeBytes.Length).CopyTo(buffer, position);
   position += 4;
   eventTypeBytes.CopyTo(buffer, position);
   position += eventTypeBytes.Length;

   // Timestamp (8 bytes)
   BitConverter.GetBytes(descriptor.Timestamp).CopyTo(buffer, position);
   position += 8;

   // Aggregate Type
   var aggTypeBytes = Encoding.UTF8.GetBytes(descriptor.AggregateType);
   BitConverter.GetBytes(aggTypeBytes.Length).CopyTo(buffer, position);
   position += 4;
   aggTypeBytes.CopyTo(buffer, position);
   position += aggTypeBytes.Length;

   // Aggregate ID (16 bytes)
   descriptor.AggregateId.ToByteArray().CopyTo(buffer, position);
   position += 16;

   // Version (8 bytes)
   BitConverter.GetBytes(descriptor.Version).CopyTo(buffer, position);
   position += 8;

   // Metadata
   WriteMetadata(buffer, ref position, descriptor.Metadata);

   // Write to file
   accessor.WriteArray(offset, buffer, 0, EVENT_HEADER_SIZE);
}

/// <summary>
/// Helper method to read metadata from buffer.
/// </summary>
private static IReadOnlyDictionary<string, string> ReadMetadata(byte[] buffer, ref int position)
{
   var count = BitConverter.ToInt32(buffer, position);
   position += 4;

   var metadata = new Dictionary<string, string>(count);
   for (var i = 0; i < count; i++)
   {
       var keyLength = BitConverter.ToInt32(buffer, position);
       position += 4;
       var key = Encoding.UTF8.GetString(buffer, position, keyLength);
       position += keyLength;

       var valueLength = BitConverter.ToInt32(buffer, position);
       position += 4;
       var value = Encoding.UTF8.GetString(buffer, position, valueLength);
       position += valueLength;

       metadata[key] = value;
   }

   return metadata;
}

/// <summary>
/// Helper method to write metadata to buffer.
/// </summary>
private static void WriteMetadata(byte[] buffer, ref int position, IReadOnlyDictionary<string, string> metadata)
{
   BitConverter.GetBytes(metadata.Count).CopyTo(buffer, position);
   position += 4;

   foreach (var (key, value) in metadata)
   {
       var keyBytes = Encoding.UTF8.GetBytes(key);
       BitConverter.GetBytes(keyBytes.Length).CopyTo(buffer, position);
       position += 4;
       keyBytes.CopyTo(buffer, position);
       position += keyBytes.Length;

       var valueBytes = Encoding.UTF8.GetBytes(value);
       BitConverter.GetBytes(valueBytes.Length).CopyTo(buffer, position);
       position += 4;
       valueBytes.CopyTo(buffer, position);
       position += valueBytes.Length;
   }
}

   public void Dispose()
   {
       _lock.Dispose();
       _eventFile.Dispose();
   }
}