using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Ensures retry middleware can be configured via <c>Ark:Messaging</c>.
/// </summary>
public class RetryConfigurationTests
{
    /// <summary>
    /// Reads retry options from configuration and expects middleware registration.
    /// </summary>
    [Fact]
    public void Retry_is_registered_when_enabled()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("Ark:Messaging:EnableRetry", "true"),
                new KeyValuePair<string,string>("Ark:Messaging:RetryCount", "2"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkCqrs(config, typeof(RetryConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var middlewares = provider.GetServices<ICommandMiddleware<TestCommand, string>>();
        Assert.Contains(middlewares, m => m.GetType().Name.Contains("RetryCommandMiddleware"));
        var options = provider.GetRequiredService<RetryOptions>();
        Assert.Equal(2, options.RetryCount);
    }

    /// <summary>
    /// Verifies that retry middleware is omitted when the setting is disabled.
    /// </summary>
    [Fact]
    public void Retry_is_not_registered_when_disabled()
    {
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkCqrs(config, typeof(RetryConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var middlewares = provider.GetServices<ICommandMiddleware<TestCommand, string>>();
        Assert.DoesNotContain(middlewares, m => m.GetType().Name.Contains("RetryCommandMiddleware"));
    }

    public record TestCommand() : ICommand<string>;

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("ok"));
    }
}
