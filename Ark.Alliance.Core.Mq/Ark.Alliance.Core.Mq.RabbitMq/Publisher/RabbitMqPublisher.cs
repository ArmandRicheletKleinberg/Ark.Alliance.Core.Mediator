using System;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Threading.RateLimiting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Resilience;
using Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;
using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging.Abstractions;


namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Publishes messages to RabbitMQ using channels from a pool.
/// </summary>
public class RabbitMqPublisher : IBrokerProducer
{
    private readonly IChannelPool _pool;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly TokenBucketRateLimiter? _rateLimiter;
    private readonly ResiliencePipeline<Result> _publishPipeline;
    private static readonly ActivitySource ActivitySource = new("Ark.Alliance.Core.Mq.RabbitMq.Publisher");

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqPublisher"/> class.
    /// </summary>
    /// <param name="pool">Channel pool used to acquire channels.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="options">Options accessor.</param>
    /// <param name="provider">Resilience pipeline provider.</param>
    public RabbitMqPublisher(IChannelPool pool, ILogger<RabbitMqPublisher> logger, IOptions<RabbitMqSettings> options, ResiliencePipelineProvider provider)
    {
        _pool = pool;
        _logger = logger;
        _settings = options.Value;
        _publishPipeline = provider.GetPipeline<Result>("rabbitmq.publish");

        if (_settings.MaxMessagesPerSecond > 0)
        {
            _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = _settings.MaxMessagesPerSecond,
                TokensPerPeriod = _settings.MaxMessagesPerSecond,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _settings.MaxMessagesPerSecond
            });
        }
    }

    /// <inheritdoc />
    /// <param name="msg">Message payload.</param>
    /// <param name="meta">Broker metadata containing exchange and routing key.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task PublishAsync<T>(T msg, BrokerMetadata meta, CancellationToken ct = default) where T : class
    {
        return PublishAsync(meta.Topic, meta.Topic, msg, null, null, null, ct);
    }

    /// <summary>
    /// Publishes a message to the specified exchange.
    /// </summary>
    /// <typeparam name="TMessage">Type of the message.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="message">The message instance.</param>
    /// <param name="headers">Optional application headers.</param>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="messageId">Unique message identifier.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>
    /// Success : The execution has succeeded.
    /// Unexpected : An unexpected error occurs.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await publisher.PublishAsync("ex", "key", msg);
    /// </code>
    /// </example>
    public async Task<Result> PublishAsync<TMessage>(string exchange, string routingKey, TMessage message, IDictionary<string, object>? headers = null, string? correlationId = null, string? messageId = null, CancellationToken token = default) where TMessage : class
    {

        if (_rateLimiter != null)
            await _rateLimiter.AcquireAsync(1, token);

        using var activity = ActivitySource.StartActivity("rabbitmq.publish", ActivityKind.Producer);

        return await _publishPipeline.ExecuteAsync(async ct =>
        {
            try
            {
                await using var lease = await _pool.AcquireAsync(ct);
                var channel = lease.Channel;

                if (_settings.PublisherConfirms)
                    await channel.ConfirmSelectAsync(ct);

                var props = new BasicProperties();
                props.Persistent = true;
                props.ContentType = "application/json";
                props.CorrelationId = correlationId ?? Guid.NewGuid().ToString();
                props.MessageId = messageId ?? Guid.NewGuid().ToString();
                if (headers is not null)
                    props.Headers = new Dictionary<string, object>(headers);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                if (_settings.MaxMessageSizeKb > 0 && body.Length > _settings.MaxMessageSizeKb * 1024)
                    _logger.LogWarning("Message size {Size} bytes exceeds limit {Limit} KB", body.Length, _settings.MaxMessageSizeKb);

                var start = Stopwatch.GetTimestamp();
                await channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: new ReadOnlyMemory<byte>(body),
                    cancellationToken: ct);

                if (_settings.PublisherConfirms)
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.ConfirmTimeoutSeconds));
                    await channel.WaitForConfirmsOrDieAsync(timeoutCts.Token);
                }

                var elapsed = (Stopwatch.GetTimestamp() - start) * 1000d / Stopwatch.Frequency;
                RabbitMqMetrics.PublishDuration.Record(elapsed);
                
                _logger.LogInformation("Published message to {Exchange}/{RoutingKey}", exchange, routingKey);
                RabbitMqMetrics.MessagesPublished.Add(1);
                return new Result<Result>().WithStatus(ResultStatus.Success);
            }
            catch (Exception ex)
            {
                RabbitMqMetrics.MessagesFailed.Add(1);
                _logger.LogError(ex, "Publishing message failed");
                return new Result<Result>(ex).WithStatus(ResultStatus.Failure);

            }
        }, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a message wrapped in a <see cref="MessageContext{TPayload}"/>.
    /// </summary>
    /// <typeparam name="TPayload">Type of the payload.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="context">Message context including headers.</param>
    /// <param name="token">Cancellation token.</param>

    /// <example>
    /// <code>
    /// var ctx = new MessageContext<MyMsg>(new MyMsg());
    /// await publisher.PublishAsync("ex", "rk", ctx);
    /// </code>
    /// </example>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>

    public Task<Result> PublishAsync<TPayload>(string exchange, string routingKey, MessageContext<TPayload> context, CancellationToken token = default) where TPayload : class
        => PublishAsync(exchange, routingKey, context.Payload, context.Headers, context.CorrelationId, context.MessageId, token);
}

