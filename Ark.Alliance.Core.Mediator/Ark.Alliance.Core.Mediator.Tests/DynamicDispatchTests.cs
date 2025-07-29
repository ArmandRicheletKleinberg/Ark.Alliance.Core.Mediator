using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests dynamic dispatching of commands and stream requests using <see cref="IArkDispatcher"/>.
/// </summary>
public class DynamicDispatchTests
{
    #region Records
    /// <summary>
    /// Example command used to verify dynamic handler resolution.
    /// </summary>
    public record Ping(string Message) : ICommand<Pong>;
    /// <summary>
    /// Response type returned by <see cref="PingHandler"/>.
    /// </summary>
    public record Pong(string Message);

    /// <summary>
    /// Event used to verify dynamic publish.
    /// </summary>
    public record PingEvent(string Message) : IEvent;

    #endregion Records

    #region Handlers
    /// <summary>
    /// Handler that appends " Pong" to the incoming <see cref="Ping"/> message.
    /// </summary>
    public class PingHandler : ICommandHandler<Ping, Pong>
    {
        public Task<Result<Pong>> HandleAsync(Ping command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Pong>.Success.WithData(new Pong(command.Message + " Pong")));
    }

    /// <summary>
    /// Marker struct representing a void result.
    /// </summary>
    public readonly record struct Unit;

    /// <summary>
    /// Command used to test handlers that return <see cref="Unit"/>.
    /// </summary>
    public record VoidPing() : ICommand<Unit>;

    /// <summary>
    /// Handler returning a successful <see cref="Unit"/> result.
    /// </summary>
    public class VoidPingHandler : ICommandHandler<VoidPing, Unit>
    {
        public Task<Result<Unit>> HandleAsync(VoidPing command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Unit>.Success);
    }

    /// <summary>
    /// Handler that sets <see cref="Handled"/> when an event is published.
    /// </summary>
    public class PingEventHandler : IEventHandler<PingEvent>
    {
        public bool Handled { get; private set; }
        public Task HandleAsync(PingEvent @event, CancellationToken ct = default)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }

    #endregion Handlers

    #region Streaming
    /// <summary>
    /// Streaming request that produces <see cref="Pong"/> items.
    /// </summary>
    public class StreamPing : IStreamRequest<Pong>
    {
        public string? Message { get; set; }
    }

    /// <summary>
    /// Handler yielding a single <see cref="Pong"/> value.
    /// </summary>
    public class StreamPingHandler : IStreamRequestHandler<StreamPing, Pong>
    {
        public async IAsyncEnumerable<Pong> HandleAsync(StreamPing request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return await Task.Run(() => new Pong(request.Message + " Pang"), cancellationToken);
        }
    }

    #endregion Streaming

    #region Methods (Tests)
    /// <summary>
    /// Verifies that dynamic dispatch resolves the correct handler for a given command.
    /// </summary>
    [Fact]
    public async Task Should_resolve_handler_via_dynamic_dispatch()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DynamicDispatchTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        object cmd = new Ping("Ping");
        var result = await dispatcher.SendAsync(cmd);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", ((Pong)result.Data!).Message);
    }

    /// <summary>
    /// Ensures dynamic dispatch works for commands returning <see cref="Unit"/>.
    /// </summary>
    [Fact]
    public async Task Should_resolve_void_handler_via_dynamic_dispatch()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DynamicDispatchTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        object cmd = new VoidPing();
        var result = await dispatcher.SendAsync(cmd);
        Assert.Equal(ResultStatus.Success, result.Status);
    }

    /// <summary>
    /// Confirms that streaming requests are dispatched dynamically and produce expected items.
    /// </summary>
    [Fact]
    public async Task Should_resolve_stream_handler_via_dynamic_dispatch()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DynamicDispatchTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        object req = new StreamPing { Message = "Ping" };
        var responses = dispatcher.CreateStream(req);
        int count = 0;
        await foreach (Pong p in responses)
        {
            Assert.Equal("Ping Pang", p.Message);
            count++;
        }
        Assert.Equal(1, count);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> when dispatching a non-command object.
    /// </summary>
    [Fact]
    public async Task Should_throw_when_object_is_not_command()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DynamicDispatchTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        await Assert.ThrowsAsync<ArgumentException>(() => dispatcher.SendAsync(new object()));
    }

    /// <summary>
    /// Confirms events can be published using the object-based overload.
    /// </summary>
    [Fact]
    public async Task Should_publish_event_via_dynamic_dispatch()
    {
        var handler = new PingEventHandler();
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DynamicDispatchTests).Assembly);
        services.AddSingleton<IEventHandler<PingEvent>>(handler);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        object evt = new PingEvent("Ping");
        await dispatcher.PublishAsync(evt);

        Assert.True(handler.Handled);
    }
    #endregion Methods (Tests)
}

