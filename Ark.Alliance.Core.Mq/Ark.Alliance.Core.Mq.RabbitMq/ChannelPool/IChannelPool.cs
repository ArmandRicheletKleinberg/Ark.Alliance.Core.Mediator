namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Abstraction for channel pools managing reusable <see cref="IChannel"/> instances.
/// </summary>
public interface IChannelPool : IAsyncDisposable
{
    /// <summary>
    /// Acquires a channel lease for publishing or consuming.
    /// </summary>
    /// <param name="ct">Cancellation token to abort waiting for a channel.</param>
    /// <returns>A <see cref="RabbitMqChannelLease"/> representing the rented channel.</returns>
    Task<RabbitMqChannelLease> AcquireAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a channel back to the pool.
    /// </summary>
    /// <param name="channel">Channel instance to release.</param>
    void Release(RabbitMQ.Client.IChannel channel);
}
