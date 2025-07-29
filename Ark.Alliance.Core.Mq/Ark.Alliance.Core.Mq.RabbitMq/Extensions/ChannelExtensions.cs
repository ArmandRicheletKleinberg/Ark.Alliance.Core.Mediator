using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Helper extensions for <see cref="RabbitMQ.Client.IChannel"/>.
/// </summary>
public static class ChannelExtensions
{
    /// <summary>
    /// Enables publisher confirmations using the synchronous API when
    /// the async method is unavailable.
    /// </summary>
    /// <param name="channel">The channel instance.</param>
    /// <param name="ct">Cancellation token.</param>
    public static Task ConfirmSelectAsync(this RabbitMQ.Client.IChannel channel, CancellationToken ct = default)
    {

        // RabbitMQ.Client 7.x exposes an asynchronous confirmation API.
        // Simply delegate to the built-in async method.

        return channel.ConfirmSelectAsync(ct);
    }

    /// <summary>
    /// Waits for publisher confirms and throws if any are lost.
    /// </summary>
    /// <param name="channel">Channel to await confirms on.</param>
    /// <param name="ct">Cancellation token.</param>
    public static Task WaitForConfirmsOrDieAsync(this RabbitMQ.Client.IChannel channel, CancellationToken ct = default)
    {
        // Delegate to the native async API which throws if confirmations fail.
        return channel.WaitForConfirmsOrDieAsync(ct);
    }
}
