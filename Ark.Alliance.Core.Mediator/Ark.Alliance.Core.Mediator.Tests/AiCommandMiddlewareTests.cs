using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Demonstrates how <see cref="AiCommandMiddleware{TCommand, TResult}"/> can short-circuit or permit
/// command execution based on a decision service.
/// </summary>
public class AiCommandMiddlewareTests
{
    /// <summary>
    /// Ensures that a command is blocked when the decision service denies execution.
    /// </summary>
    [Fact]
    public async Task Command_is_short_circuited_by_decision_service()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkMessaging(typeof(AiCommandMiddlewareTests).Assembly);
        services.AddCommandMiddleware<AiCommandMiddleware<TestCommand, string>>();
        services.AddSingleton<ICommandDecisionService, DenyDecisionService>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<TestCommand, string>(new TestCommand());

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Verifies that a command runs normally when allowed by the decision service.
    /// </summary>
    [Fact]
    public async Task Command_is_executed_when_decision_allows()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkMessaging(typeof(AiCommandMiddlewareTests).Assembly);
        services.AddCommandMiddleware<AiCommandMiddleware<TestCommand, string>>();
        services.AddSingleton<ICommandDecisionService, AllowDecisionService>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<TestCommand, string>(new TestCommand());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Handled", result.Data);
    }

    /// <summary>
    /// Decision service that always denies command execution for test purposes.
    /// </summary>
    private class DenyDecisionService : ICommandDecisionService
    {
        public Task<Result> EvaluateAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
            => Task.FromResult(Result.Failure.WithReason("Denied"));
    }

    /// <summary>
    /// Decision service that always allows command execution.
    /// </summary>
    private class AllowDecisionService : ICommandDecisionService
    {
        public Task<Result> EvaluateAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
            => Task.FromResult(Result.Success);
    }

    /// <summary>
    /// Dummy command used for testing middleware behaviour.
    /// </summary>
    public record TestCommand() : ICommand<string>;

    /// <summary>
    /// Simple handler returning a constant result for <see cref="TestCommand"/>.
    /// </summary>
    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("Handled"));
    }
}
