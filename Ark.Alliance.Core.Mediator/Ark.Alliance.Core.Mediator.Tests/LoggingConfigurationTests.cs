using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests that <see cref="ServiceCollectionExtensions"/> honors the
/// EnableLogging option from configuration.
/// </summary>
public class LoggingConfigurationTests
{
    /// <summary>
    /// Enables logging via configuration and expects the middleware to be registered.
    /// </summary>
    [Fact]
    public void Logging_is_registered_when_enabled()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("Ark:Messaging:EnableLogging", "true")
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkCqrs(config, typeof(LoggingConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var middlewares = provider.GetServices<ICommandMiddleware<TestCommand, string>>();
        Assert.Contains(middlewares, m => m.GetType().Name.Contains("LoggingCommandMiddleware"));
    }

    /// <summary>
    /// Disables logging in configuration and verifies the middleware is absent.
    /// </summary>
    [Fact]
    public void Logging_is_not_registered_when_disabled()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("Ark:Messaging:EnableLogging", "false")
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkCqrs(config, typeof(LoggingConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var middlewares = provider.GetServices<ICommandMiddleware<TestCommand, string>>();
        Assert.DoesNotContain(middlewares, m => m.GetType().Name.Contains("LoggingCommandMiddleware"));
    }

    public record TestCommand() : ICommand<string>;

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("ok"));
    }
}
