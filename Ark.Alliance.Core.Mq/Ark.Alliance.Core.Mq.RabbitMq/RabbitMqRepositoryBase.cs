using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;

using Ark.Alliance.Core;
using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Base class providing common RabbitMQ operations using the connection pool.
/// </summary>
/// <remarks>
/// <code>
/// public class MyRepo : RabbitMqRepositoryBase { ... }
/// </code>
/// </remarks>
public abstract class RabbitMqRepositoryBase
{
    private readonly IChannelPool _pool;
    private readonly ILogger<RabbitMqRepositoryBase> _logger;

    protected RabbitMqRepositoryBase(IChannelPool pool, ILogger<RabbitMqRepositoryBase> logger)
    {
        _pool = pool;
        _logger = logger;
    }

    protected async Task<Result> Execute(Func<RabbitMQ.Client.IChannel, Task<Result>> action)
    {
        await using var lease = await _pool.AcquireAsync();
        try
        {
            return await action(lease.Channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ operation failed");
            return new Result(ex);
        }
    }

    protected async Task<Result<T>> Execute<T>(Func<RabbitMQ.Client.IChannel, Task<Result<T>>> action)
    {
        await using var lease = await _pool.AcquireAsync();
        try
        {
            return await action(lease.Channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ operation failed");
            return new Result<T>(ex);
        }
    }

    /// <summary>
    /// Declares an exchange.
    /// </summary>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="type">The exchange type.</param>
    /// <param name="durable">Whether the exchange is durable.</param>
    /// <param name="autoDelete">Whether the exchange is auto deleted.</param>
    /// <param name="args">Additional arguments.</param>
    /// <returns>
    /// Success : The execution has succeeded.
    /// Unexpected : An unexpected error occurs.
    /// </returns>
    protected Task<Result> DeclareExchange(string exchange, string type, bool durable = true, bool autoDelete = false, IDictionary<string, object>? args = null)
        => Execute(async channel =>
        {
            await channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, args);
            return Result.Success;
        });

    /// <summary>
    /// Declares a queue.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="durable">Whether the queue is durable.</param>
    /// <param name="exclusive">Whether the queue is exclusive.</param>
    /// <param name="autoDelete">Whether the queue is auto deleted.</param>
    /// <param name="args">Additional arguments.</param>
    /// <returns>
    /// Success : The execution has succeeded.
    /// Unexpected : An unexpected error occurs.
    /// </returns>
    protected Task<Result> DeclareQueue(string queue, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? args = null)
        => Execute(async channel =>
        {
            await channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, args);
            return Result.Success;
        });

    /// <summary>
    /// Binds an existing queue to an exchange.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key to bind.</param>
    /// <returns>
    /// Success : The execution has succeeded.
    /// Unexpected : An unexpected error occurs.
    /// </returns>
    protected Task<Result> BindQueue(string queue, string exchange, string routingKey)
        => Execute(async channel =>
        {
            await channel.QueueBindAsync(queue, exchange, routingKey);
            return Result.Success;
        });

    /// <summary>
    /// Retrieves the current message count of a queue.
    /// </summary>
    protected Task<Result<uint>> GetQueueLengthInternal(string queue)
        => Execute(async channel =>
        {
            var info = await channel.QueueDeclarePassiveAsync(queue);
            return new Result<uint>(info.MessageCount);
        });

    /// <summary>
    /// Purges all messages from a queue.
    /// </summary>
    protected Task<Result<uint>> PurgeQueueInternal(string queue)
        => Execute(async channel =>
        {
            var count = await channel.QueuePurgeAsync(queue);
            return new Result<uint>(count);
        });
}
