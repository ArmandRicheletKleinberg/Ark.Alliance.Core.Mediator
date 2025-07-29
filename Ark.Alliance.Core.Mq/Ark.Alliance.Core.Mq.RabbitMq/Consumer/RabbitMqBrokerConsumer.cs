using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using Ark.Alliance.Core;
using Microsoft.Extensions.Options;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Adapter exposing <see cref="RabbitMqConsumer"/> through the <see cref="IBrokerConsumer"/> contract.
/// </summary>
public class RabbitMqBrokerConsumer(RabbitMqConsumer consumer, IOptions<RabbitMqSettings> options) : IBrokerConsumer
{
    private readonly RabbitMqConsumer _consumer = consumer;
    private readonly RabbitMqSettings _settings = options.Value;

    /// <inheritdoc />
    public async Task SubscribeAsync<T>(Func<T, BrokerMetadata, Task> handler, CancellationToken ct = default) where T : class
    {
        var queue = _settings.QueueName;
        await _consumer.ConsumeAsync<T>(queue, (MessageContext<T> ctx) =>
        {
            var meta = new BrokerMetadata(queue, ctx.Headers.ToStringDictionary());
            return handler(ctx.Payload, meta);
        }, ct);
    }
}
