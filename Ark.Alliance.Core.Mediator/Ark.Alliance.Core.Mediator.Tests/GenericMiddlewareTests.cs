using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Exercises middleware invocation order including generic constraints.
/// </summary>
public class GenericMiddlewareTests
{
    public record Ping(string Message) : ICommand<Pong>;
    public record Pong(string Message);

    private class PingHandler : ICommandHandler<Ping, Pong>
    {
        private readonly IList<string> _log;
        public PingHandler(IList<string> log) => _log = log;
        public Task<Result<Pong>> HandleAsync(Ping command, CancellationToken cancellationToken = default)
        {
            _log.Add("Handler");
            return Task.FromResult(Result<Pong>.Success.WithData(new Pong(command.Message + " Pong")));
        }
    }

    private class OuterPingMiddleware : ICommandMiddleware<Ping, Pong>
    {
        private readonly IList<string> _log;
        public OuterPingMiddleware(IList<string> log) => _log = log;
        public async Task<Result<Pong>> HandleAsync(Ping command, CommandHandlerDelegate<Pong> next, CancellationToken cancellationToken = default)
        {
            _log.Add("Outer before");
            var result = await next();
            _log.Add("Outer after");
            return result;
        }
    }

    private class InnerPingMiddleware : ICommandMiddleware<Ping, Pong>
    {
        private readonly IList<string> _log;
        public InnerPingMiddleware(IList<string> log) => _log = log;
        public async Task<Result<Pong>> HandleAsync(Ping command, CommandHandlerDelegate<Pong> next, CancellationToken cancellationToken = default)
        {
            _log.Add("Inner before");
            var result = await next();
            _log.Add("Inner after");
            return result;
        }
    }

    private class OuterMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        private readonly IList<string> _log;
        public OuterMiddleware(IList<string> log) => _log = log;
        public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
        {
            _log.Add("Outer generic before");
            var result = await next();
            _log.Add("Outer generic after");
            return result;
        }
    }

    private class InnerMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        private readonly IList<string> _log;
        public InnerMiddleware(IList<string> log) => _log = log;
        public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
        {
            _log.Add("Inner generic before");
            var result = await next();
            _log.Add("Inner generic after");
            return result;
        }
    }

    private class ConstrainedMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
        where TCommand : Ping, ICommand<TResult>
        where TResult : Pong
    {
        private readonly IList<string> _log;
        public ConstrainedMiddleware(IList<string> log) => _log = log;
        public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
        {
            _log.Add("Constrained before");
            var result = await next();
            _log.Add("Constrained after");
            return result;
        }
    }

    #region Methods (Tests)

    /// <summary>
    /// Verifies middleware order when two explicit middlewares wrap the handler.
    /// </summary>
    [Fact]
    public async Task Should_wrap_with_middleware()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IList<string>>(log);
        services.AddArkMessaging(typeof(GenericMiddlewareTests).Assembly);
        services.AddTransient<ICommandHandler<Ping, Pong>, PingHandler>();
        services.AddCommandMiddleware<OuterPingMiddleware>();
        services.AddCommandMiddleware<InnerPingMiddleware>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<Ping, Pong>(new Ping("Ping"));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", result.Data!.Message);
        Assert.Equal(new[] { "Outer before", "Inner before", "Handler", "Inner after", "Outer after" }, log);
    }

    /// <summary>
    /// Checks generic middleware types execute around the handler in order.
    /// </summary>
    [Fact]
    public async Task Should_wrap_generics_with_middleware()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IList<string>>(log);
        services.AddArkMessaging(typeof(GenericMiddlewareTests).Assembly);
        services.AddTransient<ICommandHandler<Ping, Pong>, PingHandler>();
        services.AddCommandMiddleware(typeof(OuterMiddleware<,>));
        services.AddCommandMiddleware(typeof(InnerMiddleware<,>));
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<Ping, Pong>(new Ping("Ping"));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", result.Data!.Message);
        Assert.Equal(new[] { "Outer generic before", "Inner generic before", "Handler", "Inner generic after", "Outer generic after" }, log);
    }

    /// <summary>
    /// Ensures middleware with type constraints runs correctly with generic commands.
    /// </summary>
    [Fact]
    public async Task Should_handle_constrained_generics()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IList<string>>(log);
        services.AddArkMessaging(typeof(GenericMiddlewareTests).Assembly);
        services.AddTransient<ICommandHandler<Ping, Pong>, PingHandler>();
        services.AddCommandMiddleware(typeof(OuterMiddleware<,>));
        services.AddCommandMiddleware(typeof(InnerMiddleware<,>));
        services.AddCommandMiddleware(typeof(ConstrainedMiddleware<,>));
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<Ping, Pong>(new Ping("Ping"));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", result.Data!.Message);
        Assert.Equal(new[] { "Outer generic before", "Inner generic before", "Constrained before", "Handler", "Constrained after", "Inner generic after", "Outer generic after" }, log);
    }

    #endregion Methods (Tests)
}
