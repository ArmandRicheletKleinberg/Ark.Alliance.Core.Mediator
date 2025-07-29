namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// <see cref="IEventPublisher{TEvent}"/> that invokes handlers sequentially in the order they are resolved.
/// </summary>
public sealed class SequentialEventPublisher<TEvent> : IEventPublisher<TEvent>
    where TEvent : IEvent
{
    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<IEventHandler<TEvent>> handlers, TEvent @event, CancellationToken ct)
    {
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, ct).ConfigureAwait(false);
        }
    }
}
