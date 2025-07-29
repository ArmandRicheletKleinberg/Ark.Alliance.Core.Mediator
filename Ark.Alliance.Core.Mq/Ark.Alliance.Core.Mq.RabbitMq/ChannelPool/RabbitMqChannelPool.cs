using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;



namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Provides a thread-safe pool of RabbitMQ channels for publishing and consuming.
/// </summary>
public class RabbitMqChannelPool : IChannelPool
{
    // Channel pool using IChannel from RabbitMQ.Client 7.x
    private readonly ConcurrentBag<RabbitMQ.Client.IChannel> _channels = new();
    private readonly IConnectionManager _connectionProvider;
    private readonly ILogger<RabbitMqChannelPool> _logger;
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a new instance of <see cref="RabbitMqChannelPool"/>.
    /// </summary>
    public RabbitMqChannelPool(
        IConnectionManager connectionProvider,
        ILogger<RabbitMqChannelPool> logger,
        IOptions<RabbitMqSettings> options)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
        _semaphore = new SemaphoreSlim(options.Value.ChannelPoolSize);
    }

    /// <summary>
    /// Acquires a channel lease from the pool. A new channel is created using the
    /// shared connection if none are available.
    /// </summary>
    public async Task<RabbitMqChannelLease> AcquireAsync(CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);

        if (_channels.TryTake(out var channel) && channel.IsOpen)
            return new RabbitMqChannelLease(channel, this);

        var connection = await _connectionProvider.GetConnectionAsync(ct);
        channel = await connection.CreateChannelAsync();
        _logger.LogDebug("RabbitMQ channel opened");
        return new RabbitMqChannelLease(channel, this);
    }

    public void Release(RabbitMQ.Client.IChannel channel)
    {
        if (channel.IsOpen)
        {
            _channels.Add(channel);
        }
        else
        {
            channel.Dispose();
        }

        _semaphore.Release();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        while (_channels.TryTake(out var channel))
        {
            if (channel.IsOpen)
                await channel.CloseAsync();

            channel.Dispose();
        }

        _semaphore.Dispose();
    }
}
