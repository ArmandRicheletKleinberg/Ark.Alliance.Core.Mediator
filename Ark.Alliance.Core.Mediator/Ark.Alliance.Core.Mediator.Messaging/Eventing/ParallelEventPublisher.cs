namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Default <see cref="IEventPublisher{TEvent}"/> that invokes all handlers in parallel.
/// </summary>
public sealed class ParallelEventPublisher<TEvent> : IEventPublisher<TEvent>
    where TEvent : IEvent
{
    /// <inheritdoc />
    public Task PublishAsync(IEnumerable<IEventHandler<TEvent>> handlers, TEvent @event, CancellationToken ct)
    {
        var tasks = handlers.Select(h => h.HandleAsync(@event, ct));
        return Task.WhenAll(tasks);
    }
}
