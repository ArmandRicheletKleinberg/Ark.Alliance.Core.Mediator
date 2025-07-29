using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Unit tests validating error conditions of <see cref="IArkDispatcher"/>.
/// </summary>
public class DispatcherErrorTests
{
    #region Methods (Tests)
    /// <summary>
    /// Verifies that <see cref="IArkDispatcher.SendAsync{TCommand,TResult}(TCommand,System.Threading.CancellationToken)"/>
    /// throws an <see cref="ArgumentNullException"/> when the command instance is <c>null</c>.
    /// </summary>
    [Fact]
    public async Task SendAsync_throws_when_command_is_null()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DispatcherErrorTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        await Assert.ThrowsAsync<ArgumentNullException>(()
            => dispatcher.SendAsync<NullCommand, string>(null!));
    }

    /// <summary>
    /// Ensures <see cref="IArkDispatcher.CreateStream{T}(IStreamRequest{T},System.Threading.CancellationToken)"/>
    /// throws an <see cref="ArgumentNullException"/> for a <c>null</c> request.
    /// </summary>
    [Fact]
    public async Task CreateStream_throws_when_request_is_null()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DispatcherErrorTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        Assert.Throws<ArgumentNullException>(() => dispatcher.CreateStream<string>(null!));
    }

    /// <summary>
    /// Checks that dispatching a command without a registered handler raises an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task SendAsync_throws_when_handler_missing()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DispatcherErrorTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(()
            => dispatcher.SendAsync<MissingCommand, string>(new MissingCommand()));
    }

    /// <summary>
    /// Confirms that publishing an event with no handlers completes without throwing.
    /// </summary>
    [Fact]
    public async Task PublishAsync_without_handlers_does_not_throw()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(DispatcherErrorTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        await dispatcher.PublishAsync(new TestEvent());
    }

    #endregion Methods (Tests)

    #region Records
    /// <summary>
    /// Marker command used for null argument tests.
    /// </summary>
    public record NullCommand() : ICommand<string>;
    /// <summary>
    /// Command without a corresponding handler registration.
    /// </summary>
    public record MissingCommand() : ICommand<string>;
    /// <summary>
    /// Simple event used in publish tests.
    /// </summary>
    public record TestEvent() : IEvent;
    #endregion Records
}

