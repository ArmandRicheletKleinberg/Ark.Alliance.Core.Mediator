using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

/// <summary>
/// Tests for <see cref="LoggingCommandMiddleware{TCommand, TResult}"/>.
/// </summary>
public class LoggingCommandMiddlewareTests
{
    /// <summary>
    /// Confirms that middleware writes log entries and still calls the handler.
    /// </summary>
    [Fact]
    public async Task Middleware_logs_and_invokes_handler()
    {
        var loggerProvider = new ListLoggerProvider();
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(loggerProvider));
        services.AddArkMessaging(typeof(LoggingCommandMiddlewareTests).Assembly);
        services.AddCqrsLogging();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<LogTestCommand, string>(new LogTestCommand());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Contains(loggerProvider.Logs, l => l.Contains("Executing command"));
        Assert.Contains(loggerProvider.Logs, l => l.Contains("succeeded"));
    }

    private sealed class ListLoggerProvider : ILoggerProvider
    {
        public List<string> Logs { get; } = new();
        public ILogger CreateLogger(string categoryName) => new ListLogger(Logs);
        public void Dispose() { }

        private sealed class ListLogger : ILogger
        {
            private readonly List<string> _logs;
            public ListLogger(List<string> logs) => _logs = logs;
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => _logs.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    public record LogTestCommand() : ICommand<string>;

    public class LogTestCommandHandler : ICommandHandler<LogTestCommand, string>
    {
        public Task<Result<string>> HandleAsync(LogTestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("ok"));
    }
}
