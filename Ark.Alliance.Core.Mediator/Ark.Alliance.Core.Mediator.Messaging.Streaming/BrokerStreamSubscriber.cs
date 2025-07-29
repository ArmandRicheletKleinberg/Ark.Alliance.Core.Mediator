using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
namespace Ark.Alliance.Core.Mediator.Messaging.Streaming;

/// <summary>
/// Stream subscriber that receives items from a message broker.
/// </summary>
/// <typeparam name="T">Type of items consumed.</typeparam>
public class BrokerStreamSubscriber<T> : IStreamSubscriber<T> where T : class
{
    #region Fields

    private readonly IBrokerConsumer _consumer;
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerStreamSubscriber{T}"/> class.
    /// </summary>
    /// <param name="consumer">Broker consumer used to receive messages.</param>
    public BrokerStreamSubscriber(IBrokerConsumer consumer) => _consumer = consumer;

    #endregion Constructors

    /// <summary>
    /// Consumes items published on the specified broker topic.
    /// </summary>
    /// <param name="topic">Topic to subscribe to.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>A stream of items consumed from the broker.</returns>
    #region Public Methods

    public async IAsyncEnumerable<T> ConsumeAsync(string topic, [EnumeratorCancellation] CancellationToken ct)
    {
        _ = _consumer.SubscribeAsync<T>((msg, meta) =>
        {
            _channel.Writer.TryWrite(msg);
            return Task.CompletedTask;
        }, ct);

        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            while (_channel.Reader.TryRead(out var item))
                yield return item;
        }
    }

    #endregion Public Methods
}
