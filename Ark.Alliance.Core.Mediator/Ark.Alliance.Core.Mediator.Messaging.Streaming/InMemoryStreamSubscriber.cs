using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
namespace Ark.Alliance.Core.Mediator.Messaging.Streaming;

/// <summary>
/// In-memory implementation of <see cref="IStreamSubscriber{T}"/> reading from a <see cref="Channel{T}"/>.
/// </summary>
/// <typeparam name="T">Type of items consumed.</typeparam>
public class InMemoryStreamSubscriber<T> : IStreamSubscriber<T>
{
    #region Fields

    private readonly Channel<T> _channel;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryStreamSubscriber{T}"/> class.
    /// </summary>
    /// <param name="channel">Channel to consume messages from.</param>
    public InMemoryStreamSubscriber(Channel<T> channel) => _channel = channel;

    #endregion Constructors

    /// <summary>
    /// Consumes items written to the underlying channel.
    /// </summary>
    /// <param name="topic">Ignored topic parameter for compatibility.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>A stream of items produced by the channel.</returns>
    #region Public Methods

    public async IAsyncEnumerable<T> ConsumeAsync(string topic, [EnumeratorCancellation] CancellationToken ct)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            while (_channel.Reader.TryRead(out var item))
                yield return item;
        }
    }

    #endregion Public Methods
}
