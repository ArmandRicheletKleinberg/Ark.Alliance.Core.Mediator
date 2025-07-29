using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class PollyConfigurationTests
{
    [Fact]
    public async Task Polly_is_registered_when_enabled()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("Ark:Messaging:EnablePolly","true"),
                new KeyValuePair<string,string>("Ark:Messaging:PollyRetryCount","2")
            })
            .Build();

        var services = new ServiceCollection();
        services.AddArkCqrs(config, typeof(PollyConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var middlewares = provider.GetServices<ICommandMiddleware<TestCommand, string>>();
        Assert.Contains(middlewares, m => m.GetType().Name.Contains("PollyCommandMiddleware"));
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();
        var result = await dispatcher.SendAsync<TestCommand, string>(new TestCommand());
        Assert.Equal(ResultStatus.Success, result.Status);
    }

    [Fact]
    public void Polly_is_not_registered_when_disabled()
    {
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddArkCqrs(config, typeof(PollyConfigurationTests).Assembly);
        var provider = services.BuildServiceProvider();

        var middlewares = provider.GetServices<ICommandMiddleware<TestCommand, string>>();
        Assert.DoesNotContain(middlewares, m => m.GetType().Name.Contains("PollyCommandMiddleware"));
    }

    public record TestCommand() : ICommand<string>;

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        private int _attempt;
        public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            if (_attempt++ < 1)
                throw new InvalidOperationException("fail");
            return Task.FromResult(Result<string>.Success.WithData("ok"));
        }
    }
}
