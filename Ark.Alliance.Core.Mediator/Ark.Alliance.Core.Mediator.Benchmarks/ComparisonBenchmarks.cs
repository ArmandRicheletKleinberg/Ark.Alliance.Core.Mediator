using Ark.Alliance.Core.Mediator.Generators.Hybrid;
using Ark.Alliance.Core.Mediator.Messaging;
using BenchmarkDotNet.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

[DotTraceDiagnoser]
public class ComparisonBenchmarks
{
    private IArkDispatcher _dispatcher = default!;
    private IMediator _mediator = default!;
    private readonly Ping _request = new();
    private readonly Pinged _notification = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var servicesArk = new ServiceCollection();
        servicesArk.AddSingleton(TextWriter.Null);
        servicesArk.AddLogging();
        servicesArk.AddArkMessaging(typeof(Ping).Assembly);
        servicesArk.AddCommandMiddleware(typeof(GenericCommandMiddleware<,>));
        servicesArk.AddEventMiddleware(typeof(GenericEventMiddleware<>));
        servicesArk.AddHybridHandlers(typeof(Ping).Assembly);
        var providerArk = servicesArk.BuildServiceProvider();
        _dispatcher = providerArk.GetRequiredService<IArkDispatcher>();

        var servicesMed = new ServiceCollection();
        servicesMed.AddSingleton(TextWriter.Null);
        servicesMed.AddLogging();
        servicesMed.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Ping));
            cfg.AddOpenBehavior(typeof(GenericPipelineBehavior<,>));
        });
        var providerMed = servicesMed.BuildServiceProvider();
        _mediator = providerMed.GetRequiredService<IMediator>();
    }

    [Benchmark(Baseline = true)]
    public Task MediatR_Send() => _mediator.Send(_request);

    [Benchmark]
    public Task Ark_Send() => _dispatcher.SendAsync<Ping, string>(_request);

    [Benchmark(Baseline = true)]
    public Task MediatR_Publish() => _mediator.Publish(_notification);

    [Benchmark]
    public Task Ark_Publish() => _dispatcher.PublishAsync(_notification);
}
