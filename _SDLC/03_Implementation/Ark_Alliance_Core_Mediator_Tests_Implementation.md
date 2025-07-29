# Implementation

The project uses xUnit and the .NET 9 test SDK. Each test class focuses on a specific aspect (command middleware, streaming, events). Reusable helpers create the dispatcher with required services. No external brokers are required.

```csharp
var provider = new ServiceCollection()
    .AddArkMediator()
    .BuildServiceProvider();
var dispatcher = provider.GetRequiredService<IArkDispatcher>();
```
List loggers capture output for assertions and simple AI decision services simulate allow/deny scenarios during testing.
Compile‑time handler registration ensures tests run without reflection. The helper `ListLoggerProvider` collects messages and `StubDecisionService` toggles command approval.
Orchestrator host tests spin up multiple fake services to check priority, delays and cancellation timeouts, mirroring production microservice orchestration.

```csharp
var settings = new OrchestratorSettings { Services = new() { ["A"] = new() } };
var host = new OrchestratorHost(settings, (_, _) => Task.CompletedTask);
await host.StartAsync();
```
