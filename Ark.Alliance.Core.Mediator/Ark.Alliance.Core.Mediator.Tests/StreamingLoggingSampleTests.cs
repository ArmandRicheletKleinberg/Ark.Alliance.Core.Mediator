using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using StreamingLoggingSample;
using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core;
using Xunit;

public class StreamingLoggingSampleTests
{
    [Fact]
    public async Task Ping_command_outputs_pong()
    {
        var sw = new StringWriter();
        Console.SetOut(sw);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkMessaging(typeof(PingCommand).Assembly);
        services.AddCqrsLogging();
        services.AddRetryCommandMiddleware();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<PingCommand, string>(new PingCommand());
        Console.WriteLine(result.Data);

        var output = sw.ToString();
        Assert.Contains("Handling PingCommand", output);
        Assert.Contains("Pong", output);
    }

    [Fact]
    public async Task Number_stream_can_be_cancelled()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddArkMessaging(typeof(PingCommand).Assembly);
        services.AddCqrsLogging();
        services.AddRetryCommandMiddleware();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(150);

        var numbers = new List<int>();
        var ex = await Record.ExceptionAsync(async () =>
        {
            await foreach (var n in dispatcher.CreateStream(new NumberStreamRequest(10), cts.Token))
                numbers.Add(n);
        });

        Assert.IsType<OperationCanceledException>(ex);
        Assert.NotEmpty(numbers);
        Assert.True(numbers.Count < 10);
    }
}
