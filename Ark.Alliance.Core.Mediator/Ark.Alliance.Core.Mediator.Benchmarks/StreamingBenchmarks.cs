using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Generators.Hybrid;
using System.Runtime.CompilerServices;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

[DotTraceDiagnoser]
public class StreamingBenchmarks
{
    private IArkDispatcher _dispatcher = default!;
    private readonly NumberStreamRequest _request = new(100);

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddSingleton(TextWriter.Null);
        services.AddLogging();
        services.AddArkMessaging(typeof(NumberStreamRequest).Assembly);
        services.AddCommandMiddleware(typeof(GenericCommandMiddleware<,>));
        services.AddHybridHandlers(typeof(NumberStreamRequest).Assembly);

        var provider = services.BuildServiceProvider();
        _dispatcher = provider.GetRequiredService<IArkDispatcher>();
    }

    [Benchmark]
    public async Task StreamingNumbers()
    {
        await foreach (var _ in _dispatcher.CreateStream(_request)) { }
    }
}

public record NumberStreamRequest(int Count) : IStreamRequest<int>;

public class NumberStreamHandler : IStreamRequestHandler<NumberStreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(NumberStreamRequest req, [EnumeratorCancellation] CancellationToken ct)
    {
        for (var i = 0; i < req.Count; i++)
        {
            yield return i;
            await Task.Yield();
        }
    }
}
