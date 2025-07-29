namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Represents a leased RabbitMQ channel returned from <see cref="RabbitMqChannelPool"/>.
/// Disposing the lease returns the channel to the pool.
/// </summary>
public sealed class RabbitMqChannelLease : IAsyncDisposable
{
    // Represents a leased IChannel
    internal RabbitMqChannelLease(RabbitMQ.Client.IChannel channel, IChannelPool pool)
    {
        Channel = channel;
        _pool = pool;
    }

    private readonly IChannelPool _pool;

    /// <summary>The leased channel instance.</summary>
    public RabbitMQ.Client.IChannel Channel { get; }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _pool.Release(Channel);
        return ValueTask.CompletedTask;
    }
}
