using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core;
using StreamingLoggingSample;
using Xunit;

public class PingCommandHandlerTests
{
    [Fact]
    public async Task Handler_returns_pong()
    {
        var handler = new PingCommandHandler();
        var result = await handler.HandleAsync(new PingCommand(), CancellationToken.None);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Pong", result.Data);
    }
}
