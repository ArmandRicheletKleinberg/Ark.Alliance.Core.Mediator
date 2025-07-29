# Ark.Alliance.Core.Mediator.ML

This library contains optional middleware and services to capture
command and query data for machine learning pipelines. Payloads are
sampled and encrypted before being sent to Azure Event Hubs by a
background worker.

## Components
| Component | Description |
|-----------|-------------|
| `MlCommandMiddleware` | Captures command payloads and enqueues encrypted telemetry |
| `MlQueryMiddleware` | Captures query payloads with the same mechanism |
| `EventHubTelemetryService` | Background service flushing telemetry batches to Event Hubs |
| `MlTelemetryOptions` | Configuration for sampling, encryption and batching |

## Usage
```csharp
var channel = Channel.CreateUnbounded<byte[]>();
services.AddSingleton(channel);
services.AddSingleton(new MlTelemetryOptions { EncryptionKey = "BASE64KEY" });
services.AddHostedService<EventHubTelemetryService>();
services.AddTransient(typeof(ICommandMiddleware<,>), typeof(MlCommandMiddleware<,>));
services.AddTransient(typeof(IQueryMiddleware<,>), typeof(MlQueryMiddleware<,>));
```
