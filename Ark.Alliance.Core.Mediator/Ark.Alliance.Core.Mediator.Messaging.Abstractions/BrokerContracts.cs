using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ark.Alliance.Core.Mediator.Messaging.Abstractions;

/// <summary>
/// Producer contract used to publish messages to an external broker.
/// </summary>
public interface IBrokerProducer
{
    #region Members
    /// <summary>
    /// Publishes a message to the broker.
    /// </summary>
    /// <typeparam name="T">Type of the message payload.</typeparam>
    /// <param name="msg">Message instance to publish.</param>
    /// <param name="meta">Associated metadata such as topic or headers.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    Task PublishAsync<T>(T msg, BrokerMetadata meta, CancellationToken ct = default) where T : class;

    #endregion Members
}

/// <summary>
/// Consumer contract used to subscribe to messages from an external broker.
/// </summary>
public interface IBrokerConsumer
{
    #region Members
    /// <summary>
    /// Subscribes the provided handler to messages of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the message payload.</typeparam>
    /// <param name="handler">Handler invoked for each received message.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    Task SubscribeAsync<T>(Func<T, BrokerMetadata, Task> handler, CancellationToken ct = default) where T : class;

    #endregion Members
}

/// <summary>
/// Marker interface for broker specific option classes.
/// </summary>
public interface IBrokerOptions { }

/// <summary>
/// Metadata describing a broker message.
/// </summary>
/// <param name="Topic">Topic or queue name.</param>
/// <param name="Headers">Optional message headers.</param>
public record BrokerMetadata(string Topic, IDictionary<string, string>? Headers = null);

/// <summary>
/// Publisher capable of streaming items to a broker.
/// </summary>
/// <typeparam name="T">Type of items being published.</typeparam>
public interface IStreamPublisher<in T>
{
    #region Members
    /// <summary>
    /// Publishes a sequence of items to the broker.
    /// </summary>
    /// <param name="source">Async enumerable source of items.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>A sequence of acknowledgements from the broker.</returns>
    IAsyncEnumerable<Ack> PublishAsync(IAsyncEnumerable<T> source, CancellationToken ct);

    #endregion Members
}

/// <summary>
/// Subscriber capable of consuming items from a broker.
/// </summary>
/// <typeparam name="T">Type of items consumed.</typeparam>
public interface IStreamSubscriber<out T>
{
    #region Members
    /// <summary>
    /// Consumes items from the specified topic.
    /// </summary>
    /// <param name="topic">Topic to subscribe to.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>A sequence of incoming items.</returns>
    IAsyncEnumerable<T> ConsumeAsync(string topic, CancellationToken ct);

    #endregion Members
}


/// <summary>
/// Registrar used to configure broker specific services.
/// </summary>
public interface IArkBrokerRegistrar
{
    #region Members
    /// <summary>
    /// Registers broker services using configuration data.
    /// </summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="config">Configuration section containing broker options.</param>
    void Register(IServiceCollection services, IConfigurationSection config);

    #endregion Members
}
