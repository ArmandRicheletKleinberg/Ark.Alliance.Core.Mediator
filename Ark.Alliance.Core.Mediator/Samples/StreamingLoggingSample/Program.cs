using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Generators.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamingLoggingSample;

/// <summary>
/// Sample demonstrating logging and streaming features of Ark.Alliance.Core.Mediator.
/// </summary>

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole());
services.AddArkMessaging(typeof(Program).Assembly);
services.AddHybridHandlers(typeof(Program).Assembly);
services.AddCqrsLogging();
services.AddRetryCommandMiddleware();

var provider = services.BuildServiceProvider();
var dispatcher = provider.GetRequiredService<IArkDispatcher>();

var pingResult = await dispatcher.SendAsync<PingCommand, string>(new PingCommand());
Console.WriteLine(pingResult.Data);

await foreach (var number in dispatcher.CreateStream(new NumberStreamRequest(3)))
{
    Console.WriteLine($"Stream item: {number}");
}

var cts = new CancellationTokenSource();
cts.CancelAfter(200);
try
{
    await foreach (var number in dispatcher.CreateStream(new NumberStreamRequest(5), cts.Token))
    {
        Console.WriteLine($"Stream item with token: {number}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream canceled");
}

namespace StreamingLoggingSample
{
public record PingCommand() : ICommand<string>;

public class PingCommandHandler : ICommandHandler<PingCommand, string>
{
    public Task<Result<string>> HandleAsync(PingCommand command, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Handling PingCommand");
        return Task.FromResult(Result<string>.Success.WithData("Pong"));
    }
}

public record NumberStreamRequest(int Count) : IStreamRequest<int>;

public class NumberStreamHandler : IStreamRequestHandler<NumberStreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(NumberStreamRequest req, [EnumeratorCancellation] CancellationToken ct)
    {
        for (var i = 1; i <= req.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            yield return i;
            await Task.Delay(100, ct);
        }
    }
}

}

