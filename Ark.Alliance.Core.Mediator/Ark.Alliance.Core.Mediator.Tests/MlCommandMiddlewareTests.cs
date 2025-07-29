using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core.Mediator.ML;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class MlCommandMiddlewareTests
{
    [Fact]
    public async Task Captured_payload_is_enqueued()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var options = new MlTelemetryOptions { EncryptionKey = System.Convert.ToBase64String(new byte[32]), SamplingRate = 100 };
        services.AddMlTelemetry(options);
        services.AddArkMessaging(typeof(MlCommandMiddlewareTests).Assembly);

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();
        var channel = provider.GetRequiredService<Channel<byte[]>>();

        var result = await dispatcher.SendAsync<TestCommand, string>(new TestCommand());
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.True(channel.Reader.TryRead(out var data));
        Assert.NotEmpty(data);
    }

    public record TestCommand() : ICommand<string>;
    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("ok"));
    }
}
