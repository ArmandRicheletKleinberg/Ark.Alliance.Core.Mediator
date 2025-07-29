using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Microsoft.Extensions.Resilience;
using Xunit;

/// <summary>
/// Validates retry behavior provided by Polly and the resilience pipeline.
/// </summary>
public class ResilienceMiddlewareTests
{
    /// <summary>
    /// Uses a Polly policy to retry until the command handler succeeds.
    /// </summary>
    [Fact]
    public async Task Polly_middleware_retries_until_success()
    {
        var policy = Policy.Handle<InvalidOperationException>().RetryAsync(2);
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(ResilienceMiddlewareTests).Assembly);
        services.AddPollyResilience(policy);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<FlakyCommand, string>(new FlakyCommand());
        Assert.Equal(ResultStatus.Success, result.Status);
    }

    /// <summary>
    /// Confirms the resilience pipeline retries and eventually returns success.
    /// </summary>
    [Fact]
    public async Task Pipeline_middleware_retries_until_success()
    {
        var providerStub = new ResiliencePipelineProvider(2);
        var services = new ServiceCollection();
        services.AddSingleton(providerStub);
        services.AddArkMessaging(typeof(ResilienceMiddlewareTests).Assembly);
        services.AddResiliencePipeline();
        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IArkDispatcher>();

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
