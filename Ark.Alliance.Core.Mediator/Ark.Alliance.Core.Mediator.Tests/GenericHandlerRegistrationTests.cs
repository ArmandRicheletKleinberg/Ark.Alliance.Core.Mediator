using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests open and closed generic handler registrations.
/// </summary>
public class GenericHandlerRegistrationTests
{
    #region Records
    /// <summary>
    /// Response record for ping commands.
    /// </summary>
    public record Pong(string Message)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pong"/> class with an empty message.
        /// Required for the generic handler which constrains <c>T</c> to have a
        /// public parameterless constructor.
        /// </summary>
        public Pong() : this(string.Empty)
        {
        }
    }

    /// <summary>
    /// Generic ping command carrying a message.
    /// </summary>
    public record GenericPing<T>(string Message) : ICommand<T> where T : Pong;
    #endregion Records

    #region Handlers
    /// <summary>
    /// Generic handler returning a pong response.
    /// </summary>
    public class GenericPingHandler<T> : ICommandHandler<GenericPing<T>, T>
        where T : Pong, new()
    {
        public Task<Result<T>> HandleAsync(GenericPing<T> command, CancellationToken cancellationToken = default)
        {
            var result = new T { Message = command.Message + " Pong" };
            return Task.FromResult(Result<T>.Success.WithData(result));
        }
    }

    public record PongExtension() : Pong(string.Empty);

    /// <summary>
    /// Handler overriding the generic version for <see cref="PongExtension"/>.
    /// </summary>
    public class SpecificPingHandler : ICommandHandler<GenericPing<PongExtension>, PongExtension>
    {
        public Task<Result<PongExtension>> HandleAsync(GenericPing<PongExtension> command, CancellationToken cancellationToken = default)
        {
            var result = new PongExtension { Message = command.Message + " Specific" };
            return Task.FromResult(Result<PongExtension>.Success.WithData(result));
        }
    }

    #endregion Handlers

    #region Methods (Tests)

    /// <summary>
    /// Resolves the open generic handler and returns a typed pong.
    /// </summary>
    [Fact]
    public async Task Should_resolve_open_generic_handler()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(new ArkMessagingOptions { HandlerRegistration = HandlerRegistrationMode.Generated });
        services.AddTransient<ICommandHandler<GenericPing<Pong>, Pong>, GenericPingHandler<Pong>>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var cmd = new GenericPing<Pong>("Ping");
        var result = await dispatcher.SendAsync<GenericPing<Pong>, Pong>(cmd);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", result.Data!.Message);
    }

    /// <summary>
    /// Demonstrates that a closed registration overrides the open generic version.
    /// </summary>
    [Fact]
    public async Task Closed_registration_overrides_open_generic()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(new ArkMessagingOptions { HandlerRegistration = HandlerRegistrationMode.Generated });
        services.AddTransient<ICommandHandler<GenericPing<Pong>, Pong>, GenericPingHandler<Pong>>();
        services.AddTransient<ICommandHandler<GenericPing<PongExtension>, PongExtension>, SpecificPingHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var cmd = new GenericPing<PongExtension>("Ping");
        var result = await dispatcher.SendAsync<GenericPing<PongExtension>, PongExtension>(cmd);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Specific", result.Data!.Message);
    }

    #endregion Methods (Tests)
}
