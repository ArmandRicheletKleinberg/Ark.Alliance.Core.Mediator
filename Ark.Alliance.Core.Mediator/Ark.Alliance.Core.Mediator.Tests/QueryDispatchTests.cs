using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests synchronous and dynamic dispatch of queries through the dispatcher.
/// </summary>
public class QueryDispatchTests
{
    public record PingQuery(string Message) : IQuery<Pong>;
    public record Pong(string Message);

    public class PingQueryHandler : IQueryHandler<PingQuery, Pong>
    {
        public Task<Result<Pong>> HandleAsync(PingQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Pong>.Success.WithData(new Pong(query.Message + " Pong")));
    }

    /// <summary>
    /// Dispatches a typed query and verifies the response contains appended text.
    /// </summary>
    [Fact]
    public async Task Should_dispatch_query()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(QueryDispatchTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.QueryAsync<PingQuery, Pong>(new PingQuery("Ping"));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", result.Data!.Message);
    }

    /// <summary>
    /// Sends the same query using object-based dispatch to ensure runtime resolution.
    /// </summary>
    [Fact]
    public async Task Should_dispatch_query_via_dynamic_dispatch()
    {
        var services = new ServiceCollection();
        services.AddArkMessaging(typeof(QueryDispatchTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        object query = new PingQuery("Ping");
        var result = await dispatcher.QueryAsync(query);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Ping Pong", ((Pong)result.Data!).Message);
    }
}
