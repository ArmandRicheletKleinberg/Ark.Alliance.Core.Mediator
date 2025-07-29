using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ark.Alliance.Core.Mediator.IoC;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Xunit;

/// <summary>
/// Verifies handler discovery when using reflection versus generated registration.
/// </summary>
public class RegistrationModeTests
{
    public record ModeCommand() : ICommand<string>;

    public class ModeHandler : ICommandHandler<ModeCommand, string>
    {
        public Task<Result<string>> HandleAsync(ModeCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("ok"));
    }

    /// <summary>
    /// Ensures reflection-based registration locates the command handler.
    /// </summary>
    [Fact]
    public async Task Reflection_only_mode_resolves_handler()
    {
        var options = new ArkMessagingOptions { HandlerRegistration = HandlerRegistrationMode.Reflection };
        var services = new ServiceCollection();
        services.AddArkMessaging(options, typeof(RegistrationModeTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<ModeCommand, string>(new ModeCommand());

        Assert.Equal(ResultStatus.Success, result.Status);
    }

    /// <summary>
    /// Confirms generated registration finds the handler without reflection.
    /// </summary>
    [Fact]
    public async Task Generated_only_mode_resolves_handler()
    {
        var options = new ArkMessagingOptions { HandlerRegistration = HandlerRegistrationMode.Generated };
        var services = new ServiceCollection();
        services.AddArkMessaging(options, typeof(RegistrationModeTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<ModeCommand, string>(new ModeCommand());

        Assert.Equal(ResultStatus.Success, result.Status);
    }
}
