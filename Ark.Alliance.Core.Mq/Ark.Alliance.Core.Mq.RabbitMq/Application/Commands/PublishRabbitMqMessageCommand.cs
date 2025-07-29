

using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Command describing a message to publish on RabbitMQ.
/// </summary>
/// <typeparam name="TMessage">Type of the message payload.</typeparam>
/// <param name="Exchange">Target exchange name.</param>
/// <param name="RoutingKey">Routing key used for the message.</param>
/// <param name="Message">Message payload to serialize.</param>
public sealed record PublishRabbitMqMessageCommand<TMessage>(string Exchange, string RoutingKey, TMessage Message) : ICommand<Unit> where TMessage : class;
