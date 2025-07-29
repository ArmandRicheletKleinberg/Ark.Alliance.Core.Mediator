namespace Ark.Alliance.Core.Mediator.ML;

/// <summary>
/// Configuration options for ML telemetry capture and ingestion.
/// </summary>
public class MlTelemetryOptions
{
    /// <summary>Percentage of requests to sample (0-100).</summary>
    public int SamplingRate { get; set; } = 10;

    /// <summary>AES-256 key used to encrypt payloads (base64 encoded).</summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>Azure Event Hubs connection string.</summary>
    public string? EventHubConnectionString { get; set; }

    /// <summary>Event Hub name.</summary>
    public string? EventHubName { get; set; }

    /// <summary>Maximum batch size in bytes before flushing.</summary>
    public int BatchSizeBytes { get; set; } = 1024 * 1024;

    /// <summary>Time in seconds between flushes.</summary>
    public int FlushIntervalSeconds { get; set; } = 30;
}
