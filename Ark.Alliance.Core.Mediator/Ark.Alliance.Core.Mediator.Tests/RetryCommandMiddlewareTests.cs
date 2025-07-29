using System;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests for <see cref="RetryCommandMiddleware{TCommand, TResult}"/>.
/// </summary>
public class RetryCommandMiddlewareTests
{
    /// <summary>
    /// Retries the command handler until it succeeds based on configured count.
    /// </summary>
    [Fact]
    public async Task Middleware_retries_until_success()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkMessaging(typeof(RetryCommandMiddlewareTests).Assembly);
        services.AddRetryCommandMiddleware(o => o.RetryCount = 2);

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<FlakyCommand, string>(new FlakyCommand());

        Assert.Equal(ResultStatus.Success, result.Status);
    }

    public record FlakyCommand() : ICommand<string>;

    public class FlakyCommandHandler : ICommandHandler<FlakyCommand, string>
    {
        private int _attempts;
        public Task<Result<string>> HandleAsync(FlakyCommand command, CancellationToken cancellationToken = default)
        {
            if (_attempts++ < 2)
                throw new InvalidOperationException("fail");
            return Task.FromResult(Result<string>.Success.WithData("ok"));
        }
    }
}

