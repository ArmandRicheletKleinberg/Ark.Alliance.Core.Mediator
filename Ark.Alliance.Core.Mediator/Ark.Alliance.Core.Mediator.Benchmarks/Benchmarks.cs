using Ark.Alliance.Core.Mediator.Generators.Hybrid;
using Ark.Alliance.Core.Mediator.Messaging;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

[DotTraceDiagnoser]
public class Benchmarks
{
    private IArkDispatcher _dispatcher = default!;
    private readonly Ping _request = new();
    private readonly Pinged _notification = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();

        services.AddSingleton(TextWriter.Null);
        services.AddLogging();
        services.AddArkMessaging(typeof(Ping).Assembly);
        services.AddCommandMiddleware(typeof(GenericCommandMiddleware<,>));
        services.AddHybridHandlers(typeof(Ping).Assembly);
        services.AddEventMiddleware(typeof(GenericEventMiddleware<>));

        var provider = services.BuildServiceProvider();
        _dispatcher = provider.GetRequiredService<IArkDispatcher>();
    }

    [Benchmark]
    public Task SendingRequests() => _dispatcher.SendAsync<Ping, string>(_request);

    [Benchmark]
    public Task PublishingNotifications() => _dispatcher.PublishAsync(_notification);
}
