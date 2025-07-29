using Ark.Alliance.Core.Mediator.ML;
using Azure.Messaging.EventHubs;
using System.Threading.Channels;
using Xunit;

public class EventHubTelemetryServiceTests
{
    [Fact]
    public async Task Service_flushes_channel_to_producer()
    {
        var channel = Channel.CreateUnbounded<byte[]>();
        var options = new MlTelemetryOptions { BatchSizeBytes = 1, FlushIntervalSeconds = 1, EncryptionKey = System.Convert.ToBase64String(new byte[32]) };
        var producer = new InMemoryEventHubProducer();
        var service = new EventHubTelemetryService(channel, options, producer);
        await service.StartAsync(CancellationToken.None);
        await channel.Writer.WriteAsync(new byte[] { 1, 2, 3 });
        await Task.Delay(1500);
        await service.StopAsync(CancellationToken.None);
        Assert.True(producer.Sent.Count > 0);
    }

    private class InMemoryEventHubProducer : IEventHubProducer
    {
        public List<EventData> Sent { get; } = new();
        public Task SendAsync(IReadOnlyList<EventData> events, CancellationToken ct = default)
        {
            Sent.AddRange(events);
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
