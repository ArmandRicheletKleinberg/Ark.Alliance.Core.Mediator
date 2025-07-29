using Ark.Alliance.Core.Mediator.Messaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

public class GenericEventMiddleware<TEvent> : IEventMiddleware<TEvent>
    where TEvent : IEvent
{
    private readonly TextWriter _writer;

    public GenericEventMiddleware(TextWriter writer) => _writer = writer;

    public async Task HandleAsync(TEvent @event, EventHandlerDelegate next, CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync("-- Handling Event");
        await next();
        await _writer.WriteLineAsync("-- Finished Event");
    }
}
