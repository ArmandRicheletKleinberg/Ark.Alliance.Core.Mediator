using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Xunit;

namespace Ark.Alliance.Core.Mediator.Messaging.Tests;

/// <summary>
/// Validates performance characteristics of <see cref="IEventPublisher{TEvent}"/> implementations.
/// </summary>
public class EventPublisherTests
{
    #region Records
    /// <summary>
    /// Sample event used for benchmarking publishers.
    /// </summary>
    private record TestEvent() : IEvent;

    #endregion Records

    #region Handlers
    private class FirstHandler : IEventHandler<TestEvent>
    {
        public async Task HandleAsync(TestEvent @event, CancellationToken ct = default)
            => await Task.Delay(200, ct);
    }

    private class SecondHandler : IEventHandler<TestEvent>
    {
        public async Task HandleAsync(TestEvent @event, CancellationToken ct = default)
            => await Task.Delay(100, ct);
    }

    #endregion Handlers

    #region Methods (Tests)
    /// <summary>
    /// Compares execution times of <see cref="ParallelEventPublisher{TEvent}"/> and <see cref="SequentialEventPublisher{TEvent}"/>.
    /// </summary>
    [Fact]
    public async Task Parallel_publisher_runs_faster_than_sequential()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(EventPublisherTests).Assembly);
        services.AddSingleton<IEventHandler<TestEvent>, FirstHandler>();
        services.AddSingleton<IEventHandler<TestEvent>, SecondHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var sw = Stopwatch.StartNew();
        await dispatcher.PublishAsync(new TestEvent());
        sw.Stop();
        var parallel = sw.ElapsedMilliseconds;

        services = new ServiceCollection();
        services.AddArkMessaging(typeof(EventPublisherTests).Assembly);
        services.AddSingleton<IEventHandler<TestEvent>, FirstHandler>();
        services.AddSingleton<IEventHandler<TestEvent>, SecondHandler>();
        services.AddSingleton(typeof(IEventPublisher<TestEvent>), typeof(SequentialEventPublisher<TestEvent>));
        provider = services.BuildServiceProvider();
        dispatcher = provider.GetRequiredService<IArkDispatcher>();

        sw.Restart();
        await dispatcher.PublishAsync(new TestEvent());
        sw.Stop();
        var sequential = sw.ElapsedMilliseconds;

        Assert.True(sequential > parallel, $"expected sequential > parallel but {sequential} <= {parallel}");
    }
    #endregion Methods (Tests)
}
