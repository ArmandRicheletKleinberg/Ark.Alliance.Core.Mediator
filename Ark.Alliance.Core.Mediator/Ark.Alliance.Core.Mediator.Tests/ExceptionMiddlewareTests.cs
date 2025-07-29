using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ark.Alliance.Core;
using Xunit;
using Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Verifies custom exception middleware handling behaviors.
/// </summary>
public class ExceptionMiddlewareTests
{
    #region Records
    /// <summary>
    /// Command that triggers a failure for testing.
    /// </summary>
    public record FailCommand : ICommand<string>
    {
        public string Message { get; set; } = string.Empty;
        public FailCommand(string message) => Message = message;
    }
    #endregion Records

    #region Handlers
    /// <summary>
    /// Handler that throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    public class FailCommandHandler : ICommandHandler<FailCommand, string>
    {
        public Task<Result<string>> HandleAsync(FailCommand command, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(command.Message);
    }

    /// <summary>
    /// Exception handler returning a handled result.
    /// </summary>
    public class FailExceptionHandler : ICommandExceptionHandler<FailCommand, string, InvalidOperationException>
    {
        public Task HandleAsync(FailCommand command, InvalidOperationException exception, CommandExceptionHandlerState<string> state, CancellationToken cancellationToken)
        {
            state.SetHandled(Result<string>.Success.WithData(exception.Message + " Handled"));
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Action capturing the exception and modifying the command.
    /// </summary>
    public class LogExceptionAction : ICommandExceptionAction<FailCommand, InvalidOperationException>
    {
        public bool Executed { get; private set; }
        public Task ExecuteAsync(FailCommand command, InvalidOperationException exception, CancellationToken cancellationToken)
        {
            Executed = true;
            command.Message = exception.Message + " Logged";
            return Task.CompletedTask;
        }
    }
    #endregion Handlers

    #region Methods (Tests)

    /// <summary>
    /// Captures the handler exception and returns the handled result.
    /// </summary>
    [Fact]
    public async Task Handler_exception_is_captured_and_result_returned()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(ExceptionMiddlewareTests).Assembly);
        services.AddTransient<ICommandExceptionHandler<FailCommand, string, InvalidOperationException>, FailExceptionHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<FailCommand, string>(new FailCommand("Boom"));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Boom Handled", result.Data);
    }

    /// <summary>
    /// Executes an action when an exception bubbles up from the handler.
    /// </summary>
    [Fact]
    public async Task Exception_action_executes_when_exception_thrown()
    {
        var action = new LogExceptionAction();
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(ExceptionMiddlewareTests).Assembly);
        services.AddSingleton<ICommandExceptionAction<FailCommand, InvalidOperationException>>(action);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var cmd = new FailCommand("Boom");
        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync<FailCommand, string>(cmd));

        Assert.True(action.Executed);
        Assert.Equal("Boom Logged", cmd.Message);
    }

    #endregion Methods (Tests)
}
