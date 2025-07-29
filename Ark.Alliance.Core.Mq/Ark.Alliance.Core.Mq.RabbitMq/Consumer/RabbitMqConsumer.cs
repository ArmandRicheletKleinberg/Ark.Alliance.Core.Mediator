using Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text.Json;


namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Basic queue consumer using an async callback.
/// </summary>
public class RabbitMqConsumer
{
    private readonly IChannelPool _pool;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly RabbitMqSettings _settings;
    private static readonly ActivitySource ActivitySource = new("Ark.Alliance.Core.Mq.RabbitMq.Consumer");

    public RabbitMqConsumer(IChannelPool pool, ILogger<RabbitMqConsumer> logger, IOptions<RabbitMqSettings> options)
    {
        _pool = pool;
        _logger = logger;
        _settings = options.Value;
    }

    /// <summary>
    /// Consumes messages from a queue.
    /// </summary>
    /// <typeparam name="TMessage">Type of the message.</typeparam>
    /// <param name="queue">Queue name.</param>
    /// <param name="onMessage">Callback executed for each payload.</param>
    /// <param name="token">Cancellation token.</param>
    public Task<Result> ConsumeAsync<TMessage>(string queue, Func<TMessage, Task> onMessage, CancellationToken token = default) where TMessage : class
        => ConsumeAsync<TMessage>(queue, ctx => onMessage(ctx.Payload), token);

    /// <summary>
    /// Consumes messages from a queue and exposes the <see cref="MessageContext{T}"/>.
    /// </summary>
    public async Task<Result> ConsumeAsync<TMessage>(string queue, Func<MessageContext<TMessage>, Task> onMessage, CancellationToken token = default) where TMessage : class
    {
        await using var lease = await _pool.AcquireAsync(token);
        var channel = lease.Channel;
        await channel.BasicQosAsync(0, _settings.Prefetch, false, token);

        try
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleReceived(ea, channel, onMessage, queue);
            };
            await channel.BasicConsumeAsync(queue, false, consumer, token);

            _logger.LogInformation("Consuming queue {Queue}", queue);
            token.Register(() => CloseLease(channel, lease));
            return Result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while consuming queue {Queue}", queue);
            await channel.CloseAsync();
            await lease.DisposeAsync();
            return new Result(ex);
        }
    }

    private static void CloseLease(RabbitMQ.Client.IChannel channel, RabbitMqChannelLease lease)
    {
        channel.CloseAsync();
        lease.DisposeAsync().AsTask().Wait();
    }

    private async Task HandleReceived<TMessage>(BasicDeliverEventArgs ea, RabbitMQ.Client.IChannel channel, Func<MessageContext<TMessage>, Task> handler, string queue) where TMessage : class
    {
        using var activity = ActivitySource.StartActivity("rabbitmq.consume", ActivityKind.Consumer);
        var start = Stopwatch.GetTimestamp();
        var body = ea.Body.ToArray();
        if (_settings.MaxMessageSizeKb > 0 && body.Length > _settings.MaxMessageSizeKb * 1024)
            _logger.LogWarning("Received large message of {Size} bytes from {Queue}", body.Length, queue);

        var message = JsonSerializer.Deserialize<TMessage>(body);
        if (message is null)
            return;

        var ctx = new MessageContext<TMessage>(message, ea.BasicProperties.Headers, ea.BasicProperties.CorrelationId, ea.BasicProperties.MessageId);
        var result = await Result.SafeExecute(async () =>
        {
            await handler(ctx);
            return Result.Success;
        }, ex => _logger.LogError(ex, "Error handling message"));

        var elapsed = (Stopwatch.GetTimestamp() - start) * 1000d / Stopwatch.Frequency;
        RabbitMqMetrics.ProcessDuration.Record(elapsed);

        if (result.IsSuccess)
        {
            RabbitMqMetrics.MessagesConsumed.Add(1);
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        else
        {
            RabbitMqMetrics.MessagesFaulted.Add(1);
            await channel.BasicNackAsync(ea.DeliveryTag, false, true);
        }
    }
}
