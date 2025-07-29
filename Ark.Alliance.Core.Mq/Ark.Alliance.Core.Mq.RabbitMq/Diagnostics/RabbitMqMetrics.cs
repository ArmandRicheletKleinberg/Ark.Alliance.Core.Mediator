using System.Diagnostics.Metrics;

namespace Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;

/// <summary>
/// Exposes OpenTelemetry metrics related to RabbitMQ connections.
/// </summary>
internal static class RabbitMqMetrics
{
    /// <summary>Meter name for all RabbitMQ metrics.</summary>
    public const string MeterName = "Ark.Alliance.Core.Mq.RabbitMq";

    /// <summary>Meter instance used to create metrics instruments.</summary>
    public static readonly Meter Meter = new(MeterName);

    /// <summary>Counts opened RabbitMQ connections.</summary>
    public static readonly Counter<long> ConnectionsOpened = Meter.CreateCounter<long>("rabbitmq.connections.opened");

    /// <summary>Counts closed RabbitMQ connections.</summary>
    public static readonly Counter<long> ConnectionsClosed = Meter.CreateCounter<long>("rabbitmq.connections.closed");

    /// <summary>Counts successfully published messages.</summary>
    public static readonly Counter<long> MessagesPublished = Meter.CreateCounter<long>("rabbitmq.messages.published");

    /// <summary>Counts messages that failed to publish.</summary>
    public static readonly Counter<long> MessagesFailed = Meter.CreateCounter<long>("rabbitmq.messages.failed");

    /// <summary>Counts messages successfully consumed.</summary>
    public static readonly Counter<long> MessagesConsumed = Meter.CreateCounter<long>("rabbitmq.messages.consumed");

    /// <summary>Counts messages that failed during processing.</summary>
    public static readonly Counter<long> MessagesFaulted = Meter.CreateCounter<long>("rabbitmq.messages.faulted");

    /// <summary>
    /// Tracks publish latency in milliseconds.
    /// </summary>
    public static readonly Histogram<double> PublishDuration = Meter.CreateHistogram<double>(
        "rabbitmq.publish.duration.ms",
        unit: "ms",
        description: "Time taken to publish a message.");

    /// <summary>
    /// Tracks message processing latency in milliseconds.
    /// </summary>
    public static readonly Histogram<double> ProcessDuration = Meter.CreateHistogram<double>(
        "rabbitmq.process.duration.ms",
        unit: "ms",
        description: "Time taken to process a consumed message.");
}
