using Ark.Alliance.Core.Eventing;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Notification raised when a RabbitMQ connection is closed.
/// </summary>
/// <param name="Host">Target host of the connection.</param>
public sealed record ConnectionClosedNotification(string Host) : INotification;
