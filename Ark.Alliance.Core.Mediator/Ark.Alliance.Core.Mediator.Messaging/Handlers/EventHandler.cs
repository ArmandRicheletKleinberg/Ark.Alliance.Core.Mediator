namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Helper base class for synchronous event handlers similar to MediatR's
/// <c>NotificationHandler&lt;T&gt;</c>.
/// </summary>
/// <typeparam name="TEvent">Event type this handler processes.</typeparam>
public abstract class EventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    Task IEventHandler<TEvent>.HandleAsync(TEvent @event, CancellationToken cancellationToken)
    {
        Handle(@event);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override in a derived class to handle the event synchronously.
    /// </summary>
    /// <param name="event">Event instance.</param>
    protected abstract void Handle(TEvent @event);
}
