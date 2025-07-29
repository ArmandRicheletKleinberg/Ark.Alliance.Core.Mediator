using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Validates event publisher selection through configuration.
/// </summary>
public class EventPublisherConfigurationTests
{
    /// <summary>
    /// Configures the sequential publisher and checks that it is resolved.
    /// </summary>
    [Fact]
    public void Sequential_publisher_is_registered_when_selected()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("Ark:Messaging:EventPublisher", "Sequential")
            })
            .Build();

        var services = new ServiceCollection();
        services.AddArkCqrs(config, typeof(EventPublisherConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var publisher = provider.GetRequiredService<IEventPublisher<TestEvent>>();
        Assert.IsType<SequentialEventPublisher<TestEvent>>(publisher);
    }

    /// <summary>
    /// Uses default settings and verifies the parallel publisher is chosen.
    /// </summary>
    [Fact]
    public void Parallel_publisher_is_default()
    {
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddArkCqrs(config, typeof(EventPublisherConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var publisher = provider.GetRequiredService<IEventPublisher<TestEvent>>();
        Assert.IsType<ParallelEventPublisher<TestEvent>>(publisher);
    }

    private record TestEvent() : IEvent;

    private class Handler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent @event, CancellationToken ct = default) => Task.CompletedTask;
    }
}
