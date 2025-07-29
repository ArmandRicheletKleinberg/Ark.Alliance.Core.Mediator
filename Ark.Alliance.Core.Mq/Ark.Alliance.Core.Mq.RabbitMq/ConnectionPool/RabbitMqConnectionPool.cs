using Ark.Alliance.Core.Diagnostics;
using Ark.Alliance.Core.Eventing;
using Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Resilience;
using System.Collections.Concurrent;


namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Provides a simple connection pool with retry policies.
/// </summary>
public class RabbitMqConnectionPool : IAsyncDisposable
{
    #region Fields
    private readonly ConcurrentBag<RabbitMQ.Client.IConnection> _connections = new();
    private readonly RabbitMqSettings _options;
    private readonly ILogger<RabbitMqConnectionPool> _logger;
    private readonly ResiliencePipeline<RabbitMQ.Client.IConnection> _pipeline;
    private int _createdConnections;
    private int _activeConnections;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="RabbitMqConnectionPool"/>.
    /// </summary>
    public RabbitMqConnectionPool(IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqConnectionPool> logger,
        ResiliencePipelineProvider provider)
    {
        _options = options.Value;
        _logger = logger;
        _pipeline = provider.GetPipeline<RabbitMQ.Client.IConnection>("rabbitmq");
    }
    #endregion

    #region Methods (Public)
    /// <summary>
    /// Acquires an open connection from the pool. A new connection is created
    /// when the pool is not at capacity. Otherwise the call waits until a
    /// connection is returned.
    /// <code>
    /// await using var connection = await pool.AcquireAsync();
    /// </code>
    /// </summary>
    /// <returns>An open <see cref="IConnection"/> instance.</returns>

    public async Task<RabbitMQ.Client.IConnection> AcquireAsync()
    {
        if (TryTakeOpenConnection(out var conn))
        {

            IncrementMetrics();
            return conn;


        }

        if (ShouldCreateConnection())
            return await CreateAndTrackConnectionAsync();

        return await WaitForConnectionAsync();
    }

    private bool TryTakeOpenConnection(out RabbitMQ.Client.IConnection? connection)
    {
        if (_connections.TryTake(out var conn) && conn.IsOpen)
        {

            connection = conn;
            return true;
        }

        connection = null;
        return false;
    }

    private bool ShouldCreateConnection() =>
        Interlocked.Increment(ref _createdConnections) <= _options.MaxConnections;

    private async Task<RabbitMQ.Client.IConnection> CreateAndTrackConnectionAsync()
    {
        var conn = await CreateConnectionAsync();
        IncrementMetrics();
        DomainEvents.Publish(new ConnectionOpenedNotification(_options.HostName));
        return conn;
    }

    private async Task<RabbitMQ.Client.IConnection> WaitForConnectionAsync()
    {
        while (true)
        {
            if (TryTakeOpenConnection(out var conn))
            {
                IncrementMetrics();
                return conn!;
            }

            await Task.Delay(50);
        }
    }

    private void IncrementMetrics()
    {
        Interlocked.Increment(ref _activeConnections);
        RabbitMqMetrics.ConnectionsOpened.Add(1);
        DiagBase.Indicators.SetIndicatorValue("ActiveConnections", _activeConnections);
    }

    /// <summary>
    /// Returns a connection back to the pool.
    /// </summary>
    /// <param name="connection">The connection instance to release.</param>
    public void Release(RabbitMQ.Client.IConnection connection)
    {
        if (connection.IsOpen)
        {
            _connections.Add(connection);
        }
        else
        {
            connection.Dispose();
            DomainEvents.Publish(new ConnectionClosedNotification(_options.HostName));
        }
        Interlocked.Decrement(ref _activeConnections);
        RabbitMqMetrics.ConnectionsClosed.Add(1);
        DiagBase.Indicators.SetIndicatorValue("ActiveConnections", _activeConnections);
    }
    #endregion

    #region Methods (Private)
    /// <summary>
    /// Creates a new RabbitMQ connection using the configured settings.
    /// </summary>
    /// <returns>An open <see cref="IConnection"/>.</returns>
    /// <summary>
    /// Creates a new RabbitMQ connection using the configured options.
    /// </summary>

    private async Task<RabbitMQ.Client.IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = _options.NetworkRecoveryInterval
        };

        if (_options.UseTls)
            factory.Ssl.Enabled = true;

        var result = await _pipeline.ExecuteAsync(async _ =>
        {
            var conn = await factory.CreateConnectionAsync();
            return new Result<RabbitMQ.Client.IConnection>(conn);
        });

        var connection = result.Data;
        _logger.Log(_options.LogLevel, "RabbitMQ connection opened to {Host}", _options.HostName);
        DomainEvents.Publish(new ConnectionOpenedNotification(_options.HostName));
        return connection;
    }
    #endregion

    #region IDisposable
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        while (_connections.TryTake(out var connection))
        {
            await connection.CloseAsync();
            RabbitMqMetrics.ConnectionsClosed.Add(1);
            DomainEvents.Publish(new ConnectionClosedNotification(_options.HostName));
        }
        _activeConnections = 0;
        DiagBase.Indicators.SetIndicatorValue("ActiveConnections", _activeConnections);
    }
    #endregion
}
