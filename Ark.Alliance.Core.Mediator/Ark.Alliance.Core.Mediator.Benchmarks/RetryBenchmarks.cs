using Ark.Alliance.Core.Mediator.Generators.Hybrid;
using Ark.Alliance.Core.Mediator.Messaging;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

[DotTraceDiagnoser]
public class RetryBenchmarks
{
    private IArkDispatcher _dispatcher = default!;
    private readonly Ping _request = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddSingleton(TextWriter.Null);
        services.AddLogging();
        services.AddArkMessaging(typeof(Ping).Assembly);
        services.AddRetryCommandMiddleware();

        var provider = services.BuildServiceProvider();
        _dispatcher = provider.GetRequiredService<IArkDispatcher>();
        services.AddHybridHandlers(typeof(Ping).Assembly);
    }

    [Benchmark]
    public Task SendingWithRetry() => _dispatcher.SendAsync<Ping, string>(_request);
}
