using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Validates command pre- and post-processors modify messages as expected.
/// </summary>
public class PrePostProcessorTests
{
    public record PingCommand : ICommand<string>
    {
        public string Message { get; set; } = string.Empty;
        public PingCommand(string message) => Message = message;
    }

    public class PingHandler : ICommandHandler<PingCommand, string>
    {
        public Task<Result<string>> HandleAsync(PingCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData(command.Message + " Pong"));
    }

    public class PingPreProcessor : ICommandPreProcessor<PingCommand>
    {
        public Task Process(PingCommand command, CancellationToken cancellationToken)
        {
            command.Message += " Pre";
            return Task.CompletedTask;
        }
    }

    public class PingPostProcessor : ICommandPostProcessor<PingCommand, string>
    {
        public Task Process(PingCommand command, Result<string> response, CancellationToken cancellationToken)
        {
            response.WithData(response.Data + " Post");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Ensures processors alter the command before and after handler execution.
    /// </summary>
    [Fact]
    public async Task Should_run_pre_and_post_processors()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(PrePostProcessorTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var cmd = new PingCommand("Ping");
        var result = await dispatcher.SendAsync<PingCommand, string>(cmd);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pre Pong Post", result.Data);
    }
}
