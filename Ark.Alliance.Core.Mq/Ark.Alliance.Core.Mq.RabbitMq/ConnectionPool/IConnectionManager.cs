namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Provides access to a shared RabbitMQ <see cref="IConnection"/> instance.
/// Connections are automatically recovered when possible.
/// </summary>
public interface IConnectionManager : IAsyncDisposable
{
    /// <summary>
    /// Gets an open connection, creating it if necessary.
    /// </summary>
    /// <param name="ct">Cancellation token used to abort the attempt.</param>
    /// <returns>An active <see cref="IConnection"/>.</returns>
    Task<IConnection> GetConnectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Indicates whether a connection is currently opened.
    /// </summary>
    bool IsConnected { get; }
}
