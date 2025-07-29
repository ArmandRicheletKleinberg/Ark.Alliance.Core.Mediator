namespace Ark.Alliance.Core.Mediator.ML;

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

/// <summary>
/// Real <see cref="IEventHubProducer"/> implementation using <see cref="EventHubProducerClient"/>.
/// </summary>
public sealed class EventHubProducerWrapper : IEventHubProducer
{
    private readonly EventHubProducerClient _client;

    public EventHubProducerWrapper(MlTelemetryOptions options)
    {
        _client = new EventHubProducerClient(options.EventHubConnectionString, options.EventHubName);
    }

    public async Task SendAsync(IReadOnlyList<EventData> events, CancellationToken ct = default)
    {
        using EventDataBatch batch = await _client.CreateBatchAsync(ct);
        foreach (var ev in events)
            if (!batch.TryAdd(ev))
                throw new InvalidOperationException("Event too large for batch");
        await _client.SendAsync(batch, ct);
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}
