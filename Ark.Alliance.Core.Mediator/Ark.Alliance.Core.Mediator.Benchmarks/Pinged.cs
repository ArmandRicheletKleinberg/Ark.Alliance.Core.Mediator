using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

public record Pinged() : IEvent;

public class PingedHandler : IEventHandler<Pinged>
{
    public Task HandleAsync(Pinged notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
