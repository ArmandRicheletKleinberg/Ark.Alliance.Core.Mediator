using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Runtime.CompilerServices;
using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
namespace Ark.Alliance.Core.Mediator.Messaging.Streaming;

/// <summary>
/// Simple in-memory implementation of <see cref="IStreamPublisher{T}"/> based on <see cref="Channel{T}"/>.
/// </summary>
/// <typeparam name="T">Type of items being published.</typeparam>
public class InMemoryStreamPublisher<T> : IStreamPublisher<T>
{
    #region Fields

    private readonly Channel<T> _channel;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryStreamPublisher{T}"/> class.
    /// </summary>
    /// <param name="channel">Channel used to publish items.</param>
    public InMemoryStreamPublisher(Channel<T> channel) => _channel = channel;

    #endregion Constructors

    /// <summary>
    /// Publishes all items from the source into the underlying channel.
    /// </summary>
    /// <param name="source">Source stream of items.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>A sequence of acknowledgements for each published item.</returns>
    #region Public Methods

    public async IAsyncEnumerable<Ack> PublishAsync(IAsyncEnumerable<T> source, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in source.WithCancellation(ct))
        {
            await _channel.Writer.WriteAsync(item, ct);
            yield return Ack.Ok;
        }
    }

    #endregion Public Methods
}
