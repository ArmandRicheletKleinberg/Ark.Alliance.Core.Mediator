namespace Ark.Alliance.Core.Mediator.ML;

using Azure.Messaging.EventHubs;

/// <summary>
/// Abstraction over <see cref="Azure.Messaging.EventHubs.Producer.EventHubProducerClient"/> for testability.
/// </summary>
public interface IEventHubProducer : IAsyncDisposable
{
    /// <summary>Sends a batch of events.</summary>
    Task SendAsync(IReadOnlyList<EventData> events, CancellationToken ct = default);
}
