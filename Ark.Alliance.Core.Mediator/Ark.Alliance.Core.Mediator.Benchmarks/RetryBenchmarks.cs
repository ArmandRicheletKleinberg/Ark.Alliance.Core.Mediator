using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Generators.Hybrid;

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
