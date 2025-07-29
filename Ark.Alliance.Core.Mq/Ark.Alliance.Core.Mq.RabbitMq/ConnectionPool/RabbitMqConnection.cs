using Ark.Alliance.Core.Eventing;
using Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Resilience;


namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Provides a singleton RabbitMQ connection managed through
/// <see cref="IConnectionManager"/>. The connection is created lazily and
/// automatically recovered when possible.
/// </summary>
public class RabbitMqConnection : IConnectionManager
{
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly RabbitMqSettings _options;
    private readonly ResiliencePipeline<RabbitMQ.Client.IConnection> _pipeline;
    private RabbitMQ.Client.IConnection? _connection;

    /// <inheritdoc />
    public bool IsConnected => _connection?.IsOpen == true;

    /// <summary>
    /// Creates a new instance of <see cref="RabbitMqConnection"/>.
    /// </summary>
    /// <param name="options">RabbitMQ configuration options.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="provider">Resilience pipeline provider.</param>
    public RabbitMqConnection(IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqConnection> logger,
        ResiliencePipelineProvider provider)
    {
        _options = options.Value;
        _logger = logger;
        _pipeline = provider.GetPipeline<RabbitMQ.Client.IConnection>("rabbitmq");
    }

    /// <summary>Gets an open connection instance.</summary>
    /// <param name="ct">Cancellation token used to abort connection creation.</param>
    /// <returns>An open <see cref="IConnection"/>.</returns>
    public async Task<RabbitMQ.Client.IConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            NetworkRecoveryInterval = _options.NetworkRecoveryInterval,
            AutomaticRecoveryEnabled = true
        };

        if (_options.UseTls)
        {
            factory.Ssl.Enabled = true;
        }

        var result = await _pipeline.ExecuteAsync(async _ =>
        {
            var conn = await factory.CreateConnectionAsync();
            return new Result<RabbitMQ.Client.IConnection>(conn);
        }, ct);

        _connection = result.Data;
        _logger.Log(_options.LogLevel, "RabbitMQ connection opened to {Host}", _options.HostName);
        RabbitMqMetrics.ConnectionsOpened.Add(1);
        DomainEvents.Publish(new ConnectionOpenedNotification(_options.HostName));
        return _connection;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection?.IsOpen == true)
        {
            await _connection.CloseAsync();
            _logger.Log(_options.LogLevel, "RabbitMQ connection closed");
            RabbitMqMetrics.ConnectionsClosed.Add(1);
            DomainEvents.Publish(new ConnectionClosedNotification(_options.HostName));
        }
    }
}
