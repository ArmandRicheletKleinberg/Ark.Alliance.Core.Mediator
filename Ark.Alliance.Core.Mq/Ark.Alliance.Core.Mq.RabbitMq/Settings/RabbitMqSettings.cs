using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Settings used for RabbitMQ connections.
/// </summary>
public class RabbitMqSettings
{
    /// <summary>
    /// Name of the connection pool these settings apply to.
    /// </summary>
    public string? ConnectionPoolName { get; set; }
    /// <summary>RabbitMQ host name.</summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>Port of the broker.</summary>
    public int Port { get; set; } = 5672;

    /// <summary>Virtual host to connect to.</summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>User name for the connection.</summary>
    public string UserName { get; set; } = "guest";

    /// <summary>Password for the connection.</summary>
    public string Password { get; set; } = "guest";

    /// <summary>Default exchange name.</summary>
    public string ExchangeName { get; set; } = string.Empty;

    /// <summary>Default queue name.</summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>Minimum log level for diagnostics.</summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>Maximum number of concurrent connections to keep in the pool.</summary>
    public int MaxConnections { get; set; } = 5;

    /// <summary>Number of retries when establishing a connection.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Use TLS/SSL when connecting to the broker.</summary>
    public bool UseTls { get; set; }

    /// <summary>Enable publisher confirms for reliability.</summary>
    public bool PublisherConfirms { get; set; }

    /// <summary>Prefetch count applied to consuming channels.</summary>
    public ushort Prefetch { get; set; } = 20;

    /// <summary>
    /// Maximum number of channels kept in the channel pool.
    /// </summary>
    public int ChannelPoolSize { get; set; } = 10;

    /// <summary>Maximum messages allowed per second when publishing. 0 to disable.</summary>
    public int MaxMessagesPerSecond { get; set; }

    /// <summary>Timeout in seconds waiting for publish confirmations.</summary>
    public int ConfirmTimeoutSeconds { get; set; } = 5;

    /// <summary>Interval between reconnection attempts when the network drops.</summary>
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Enable OpenTelemetry instrumentation for RabbitMQ operations.
    /// </summary>
    public bool EnableOpenTelemetry { get; set; } = true;

    /// <summary>
    /// Maximum allowed message size in kilobytes. 0 to ignore.
    /// Large messages impact performance and memory usage.
    /// </summary>
    public int MaxMessageSizeKb { get; set; } = 64;
}
