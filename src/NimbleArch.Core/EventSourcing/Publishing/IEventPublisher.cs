using NimbleArch.Core.EventSourcing.Events;

namespace NimbleArch.Core.EventSourcing.Publishing;

/// <summary>
/// High-performance event publishing interface.
/// </summary>
/// <remarks>
/// EN: Defines the contract for publishing events to subscribers with back-pressure
/// handling and parallel processing capabilities.
///
/// TR: Geri basınç yönetimi ve paralel işleme yetenekleriyle olayları
/// abonelere yayınlamak için sözleşmeyi tanımlar.
/// </remarks>
public interface IEventPublisher
{
    ValueTask PublishAsync(EventDescriptor eventDescriptor, CancellationToken cancellationToken = default);
    IDisposable Subscribe<TEvent>(Func<EventDescriptor, ValueTask> handler) where TEvent : class;
    IDisposable SubscribeAll(Func<EventDescriptor, ValueTask> handler);
}