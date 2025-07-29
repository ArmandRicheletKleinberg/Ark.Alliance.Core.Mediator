using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Background service that consumes messages from a queue.
/// </summary>
public class RabbitMqBackgroundConsumer<TMessage> : BackgroundService where TMessage : class
{
    #region Fields
    private readonly IChannelPool _pool;
    private readonly ILogger<RabbitMqBackgroundConsumer<TMessage>> _logger;
    private readonly string _queue;
    private readonly Func<TMessage, Task> _onMessage;
    private readonly RabbitMqSettings _settings;
    private static readonly ActivitySource ActivitySource = new("Ark.Alliance.Core.Mq.RabbitMq.Consumer");
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="RabbitMqBackgroundConsumer{TMessage}"/>.
    /// </summary>
    public RabbitMqBackgroundConsumer(
        IChannelPool pool,
        ILogger<RabbitMqBackgroundConsumer<TMessage>> logger,
        string queue,
        Func<TMessage, Task> onMessage,
        IOptions<RabbitMqSettings> options)
    {
        _pool = pool;
        _logger = logger;
        _queue = queue;
        _onMessage = onMessage;
        _settings = options.Value;
    }
    #endregion

    #region Methods (Protected)
    /// <summary>
    /// Consumes messages from the queue until the service is stopped.
    /// </summary>
    /// <param name="stoppingToken">Token used to gracefully stop the consumer.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var lease = await _pool.AcquireAsync(stoppingToken);
        var channel = lease.Channel;

        await channel.BasicQosAsync(0, _settings.Prefetch, false, stoppingToken);

        try
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                using var activity = ActivitySource.StartActivity("rabbitmq.consume", ActivityKind.Consumer);
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<TMessage>(body);
                if (message != null)
                {
                    try
                    {
                        await _onMessage(message);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling message");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
            };
            await channel.BasicConsumeAsync(_queue, false, consumer);
            _logger.LogInformation("Consuming queue {Queue}", _queue);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background consumer for queue {Queue}", _queue);
        }
        finally
        {
            await channel.CloseAsync();
            await lease.DisposeAsync();
        }
    }
    #endregion
}
