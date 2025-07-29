namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Publishes an event to the resolved handlers.
/// </summary>
/// <typeparam name="TEvent">Type of event.</typeparam>
public interface IEventPublisher<TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Dispatches the event to all handlers.
    /// </summary>
    /// <param name="handlers">Resolved handlers for the event type.</param>
    /// <param name="@event">Event instance.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync(IEnumerable<IEventHandler<TEvent>> handlers, TEvent @event, CancellationToken ct);
}
