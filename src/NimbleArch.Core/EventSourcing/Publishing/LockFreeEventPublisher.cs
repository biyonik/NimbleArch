using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.EventSourcing.Events;

namespace NimbleArch.Core.EventSourcing.Publishing;

/// <summary>
/// Lock-free implementation of event publisher.
/// </summary>
public sealed class LockFreeEventPublisher : IEventPublisher, IDisposable
{
   private readonly Channel<EventDescriptor> _channel;
   private readonly ConcurrentDictionary<Type, ConcurrentBag<Func<EventDescriptor, ValueTask>>> _typeHandlers;
   private ConcurrentBag<Func<EventDescriptor, ValueTask>> _globalHandlers;
   private readonly ILogger<LockFreeEventPublisher> _logger;
   private readonly Task _processTask;
   private readonly CancellationTokenSource _cts;
   private readonly int _maxDegreeOfParallelism;

   public LockFreeEventPublisher(
       ILogger<LockFreeEventPublisher> logger,
       int maxDegreeOfParallelism = 4,
       int channelCapacity = 10_000)
   {
       _logger = logger;
       _maxDegreeOfParallelism = maxDegreeOfParallelism;
       _typeHandlers = new ConcurrentDictionary<Type, ConcurrentBag<Func<EventDescriptor, ValueTask>>>();
       _globalHandlers = new ConcurrentBag<Func<EventDescriptor, ValueTask>>();
       _cts = new CancellationTokenSource();

       var options = new BoundedChannelOptions(channelCapacity)
       {
           FullMode = BoundedChannelFullMode.Wait,
           SingleReader = false,
           SingleWriter = false,
           AllowSynchronousContinuations = false
       };

       _channel = Channel.CreateBounded<EventDescriptor>(options);
       _processTask = ProcessEventsAsync(_cts.Token);
   }

   public async ValueTask PublishAsync(
       EventDescriptor eventDescriptor,
       CancellationToken cancellationToken = default)
   {
       try
       {
           // Channel'a back-pressure ile yaz
           await _channel.Writer.WriteAsync(eventDescriptor, cancellationToken);
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error publishing event {EventType}", eventDescriptor.EventType);
           throw;
       }
   }

   public IDisposable Subscribe<TEvent>(Func<EventDescriptor, ValueTask> handler) 
       where TEvent : class
   {
       var handlers = _typeHandlers.GetOrAdd(typeof(TEvent), _ => new ConcurrentBag<Func<EventDescriptor, ValueTask>>());
       handlers.Add(handler);

       return new SubscriptionHandle(() => 
       {
           if (_typeHandlers.TryGetValue(typeof(TEvent), out var bag))
           {
               // ConcurrentBag'den handler'ı kaldır
               var updatedHandlers = bag.Where(h => h != handler).ToList();
               _typeHandlers.TryUpdate(typeof(TEvent), new ConcurrentBag<Func<EventDescriptor, ValueTask>>(updatedHandlers), bag);
           }
       });
   }

   public IDisposable SubscribeAll(Func<EventDescriptor, ValueTask> handler)
   {
       _globalHandlers.Add(handler);
       return new SubscriptionHandle(() =>
       {
           // Global handler'ları güncelle
           var updatedHandlers = _globalHandlers.Where(h => h != handler).ToList();
           _globalHandlers = new ConcurrentBag<Func<EventDescriptor, ValueTask>>(updatedHandlers);
       });
   }

   private async Task ProcessEventsAsync(CancellationToken cancellationToken)
   {
       // Birden fazla consumer başlat
       var consumers = Enumerable.Range(0, _maxDegreeOfParallelism)
           .Select(_ => ConsumeEventsAsync(cancellationToken))
           .ToList();

       await Task.WhenAll(consumers);
   }

   private async Task ConsumeEventsAsync(CancellationToken cancellationToken)
   {
       while (!cancellationToken.IsCancellationRequested)
       {
           try
           {
               // Channel'dan event oku
               var eventDescriptor = await _channel.Reader.ReadAsync(cancellationToken);

               // Global handlers
               var globalTasks = _globalHandlers.Select(h => h(eventDescriptor));
               await Task.WhenAll(globalTasks as IEnumerable<Task>);

               // Type-specific handlers
               var eventType = Type.GetType(eventDescriptor.EventType);
               if (eventType != null && _typeHandlers.TryGetValue(eventType, out var handlers))
               {
                   var tasks = handlers.Select(h => h(eventDescriptor));
                   await Task.WhenAll(tasks as IEnumerable<Task>);
               }
           }
           catch (OperationCanceledException)
           {
               break;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error processing event");
               // Continue processing other events
           }
       }
   }

   public void Dispose()
   {
       _cts.Cancel();
       try
       {
           _processTask.Wait(TimeSpan.FromSeconds(5));
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error disposing event publisher");
       }
       _cts.Dispose();
   }

   private class SubscriptionHandle : IDisposable
   {
       private readonly Action _onDispose;

       public SubscriptionHandle(Action onDispose)
       {
           _onDispose = onDispose;
       }

       public void Dispose()
       {
           _onDispose();
       }
   }
}