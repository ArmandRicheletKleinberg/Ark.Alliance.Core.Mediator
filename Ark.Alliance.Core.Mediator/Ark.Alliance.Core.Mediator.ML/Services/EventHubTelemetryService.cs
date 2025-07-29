using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace Ark.Alliance.Core.Mediator.ML;

/// <summary>
/// Background service that pushes telemetry to Azure Event Hubs.
/// </summary>
public class EventHubTelemetryService : BackgroundService
{
    private readonly ChannelReader<byte[]> _reader;
    private readonly MlTelemetryOptions _options;
    private readonly IEventHubProducer _producer;

    public EventHubTelemetryService(Channel<byte[]> queue, MlTelemetryOptions options, IEventHubProducer producer)
    {
        _reader = queue.Reader;
        _options = options;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<EventData>();
        var batchSize = 0;
        var flushInterval = TimeSpan.FromSeconds(_options.FlushIntervalSeconds);
        var timer = PeriodicTimer(flushInterval);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_reader.TryRead(out var data))
                {
                    batch.Add(new EventData(data));
                    batchSize += data.Length;
                    if (batchSize >= _options.BatchSizeBytes)
                    {
                        await FlushAsync(batch, stoppingToken);
                        batch.Clear();
                        batchSize = 0;
                    }
                }

                if (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    if (batch.Count > 0)
                    {
                        await FlushAsync(batch, stoppingToken);
                        batch.Clear();
                        batchSize = 0;
                    }
                }
            }
        }
        finally
        {
            await _producer.DisposeAsync();
        }
    }

    private Task FlushAsync(List<EventData> batch, CancellationToken ct)
        => _producer.SendAsync(batch, ct);

    private static PeriodicTimer PeriodicTimer(TimeSpan interval) => new(interval);
}
