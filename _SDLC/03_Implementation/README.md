# 03_Implementation

Implementation resources and third-party packages. For build and package details see the [root README](../README.md).
The implementation replaces reflection with a source generator and integrates optional logging and resilience middleware.
This section tracks development tasks and packaging guidelines.
Dedicated notes for the generator live in
[`Ark.Alliance.Core.Mediator.Generators/_Sdlc`](../../Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Generators/_Sdlc).

See [`Samples/StreamingLoggingSample`](../Samples/StreamingLoggingSample) for a minimal reference implementation.

## Index
- [Benchmarks Implementation](Ark_Alliance_Core_Mediator_Benchmarks_Implementation.md)
- [Tests Implementation](Ark_Alliance_Core_Mediator_Tests_Implementation.md)
- [PoC Documentation](PoC_Documentation.md)


## To-Do


| ID | Task | Status | Notes |
|----|------|--------|-------|
| 4 | Extend unit tests for middleware, streaming and orchestrator components | **Done** | `OrchestratorHostTests` and middleware scenarios are fully covered. |
| 5 | Review broker adapters (RabbitMQ/Kafka) and implement integration tests | **Open** | Implement adapters and verify end-to-end messaging. |
| 8 | Eliminate runtime reflection in dispatcher streaming and broker registration | **Open** | Add generators for stream handlers and broker registrars. |
| 11 | Remove dynamic dispatch in `ArkDispatcher.CreateStream` | **Open** | Generate strongly typed stream dispatch methods. |
| 12 | Pre-generate broker registrations to eliminate reflection in IoC | **Open** | Scan for `IArkBrokerRegistrar` implementations at build time. |
| 13 | Finalize orchestrator service README and sample configuration | **Closed** | Schema and Docker usage documented. |
| 17 | Add integration tests for streaming publishers and subscribers | **Closed** | In-memory and file stream tests implemented. |
| 18 | Complete API documentation with missing XML comments | **Closed** | XML docs added to all public classes. |
| 21 | Establish CI workflow to run tests and analyzers | **Closed** | Workflow executes tests and `dotnet format`. |
| 23 | Add READMEs for benchmarks and tests | **Closed** | Documentation consolidated under the solution `_SDLC`. |
| 22 | Review IoC and Messaging XML documentation | **Closed** | Documentation reviewed; comments verified for all messaging and IoC classes. |
| 24 | Add hash-based caching to handler generator | **Done** | New enum `RegistrationCacheMode` supports memory or file cache. |
| 25 | Benchmark generator caching modes | **Open** | Compare None vs Memory vs File using BenchmarkDotNet. |
| 26 | Test generator caching behaviour | **Open** | Validate skip logic with Roslyn SDK tests. |
| 27 | Add component tables to project READMEs | **Closed** | IoC and Messaging READMEs list core components. 
| 28 | Provide per-project SDLC folders for Benchmarks and Tests | **Closed** | Benchmarks and tests now documented in the root `_SDLC`. |
| 29 | Support HandlerRegistration option for runtime control | **Done** | `ArkMessagingOptions.HandlerRegistration` toggles reflection scanning. |
| 30 | Update root README with solution diagram and test summary | **Done** | Documentation refreshed. |
| ML1 | Implement logging middleware for ML | **Done** | Payloads captured with sampling and AES encryption. |
| ML2 | Push telemetry to Event Hubs using background worker | **Done** | `EventHubTelemetryService` flushes encrypted batches asynchronously. |

| S2 | Demonstrate custom middleware usage in StreamingLoggingSample | Done | Sample updated with custom logging middleware. |
| S4 | Publish sample image to container registry | Open | Automate image build and push. |
| EX-04 | Preliminary review of Implementation folder | Closed | See [TodoList](../TodoList.md) |


