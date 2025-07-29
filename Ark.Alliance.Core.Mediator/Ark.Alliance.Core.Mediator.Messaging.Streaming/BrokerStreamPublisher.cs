using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Ark.Alliance.Core.Mediator.Messaging.Abstractions;

namespace Ark.Alliance.Core.Mediator.Messaging.Streaming;

/// <summary>
/// Stream publisher that forwards items to a message broker.
/// </summary>
/// <typeparam name="T">Type of items being published.</typeparam>
public class BrokerStreamPublisher<T> : IStreamPublisher<T> where T : class
{
    #region Fields

    private readonly IBrokerProducer _producer;
    private readonly string _topic;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerStreamPublisher{T}"/> class.
    /// </summary>
    /// <param name="producer">Broker producer used to publish messages.</param>
    /// <param name="topic">Target topic for the messages.</param>
    public BrokerStreamPublisher(IBrokerProducer producer, string topic) => (_producer, _topic) = (producer, topic);

    #endregion Constructors

    #region Public Methods
    /// <summary>
    /// Publishes the items from the source to the configured broker topic.
    /// </summary>
    /// <param name="source">Source stream of items.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>A sequence of acknowledgements for each published item.</returns>
    public async IAsyncEnumerable<Ack> PublishAsync(IAsyncEnumerable<T> source, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in source.WithCancellation(ct))
        {
            await _producer.PublishAsync(item, new(_topic), ct);
            yield return Ack.Ok;
        }
    }

    #endregion Public Methods
}
