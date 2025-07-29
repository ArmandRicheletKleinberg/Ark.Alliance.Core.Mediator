using Ark.Alliance.Core.Mediator.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

public record Ping() : ICommand<string>;

public class PingHandler : ICommandHandler<Ping, string>
{
    public Task<Result<string>> HandleAsync(Ping command, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<string>.Success.WithData("Pong"));
}
