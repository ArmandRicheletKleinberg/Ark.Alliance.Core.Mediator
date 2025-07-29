# 04_Testing

Test plans, test cases and quality assurance reports. The suite verifies the generator-based dispatcher and middleware and includes benchmarks that compare it with MediatR. The overall test summary is included in the [root README](../README.md). This folder explains how test coverage aligns with project requirements.


See [`Samples/StreamingLoggingSample`](../Samples/StreamingLoggingSample) for manual and future automated tests.
Generator-specific test scenarios are documented in
[`Ark.Alliance.Core.Mediator.Generators/_Sdlc`](../../Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Generators/_Sdlc).

## Index
- [Benchmarks Testing](Ark_Alliance_Core_Mediator_Benchmarks_Testing.md)
- [Tests Plan](Ark_Alliance_Core_Mediator_Tests_Plan.md)
- [Tests Todo List](TodoList.Tests.md)
- [Testcontainers Guide](testcontainers_dotnet_9.Md)
- [Troubleshooting](testcontainers_troubleshooting.md)
- [Troubleshooting Procedure](testcontainers_troubleshooting_procedure.md)



## To-Do
| ID | Task | Status | Notes |
|----|------|--------|-------|
| 4 | Extend unit tests for middleware, streaming and orchestrator components | **Closed** | Added `OrchestratorHostTests` covering concurrency, priority and fail-fast options. |
| 5 | Review broker adapters and implement integration tests | Open | Requires RabbitMQ/Kafka adapters. |
| 10 | Benchmark dispatch vs. MediatR and publish results | Closed | Benchmark project implemented. |
| 17 | Add integration tests for streaming publishers and subscribers | Closed | Implemented for in-memory and file streams. |
| 21 | Establish CI workflow to run tests and analyzers | Closed | Workflow added in `.github/workflows`. Pull requests to `master` and `develop` must pass; others can be skipped by labelling `skip-ci`. |
| 22 | Add unit tests for ServiceCollectionExtensions | Open | Validate DI configuration helpers. |
| 24 | Verify table sections in READMEs through doc tests | Closed | Markdown tests ensure links are valid. |
| 23 | Provide README for test project | Closed | Describes test coverage and execution. |
| 26 | Document per-project SDLC folders | **Closed** | Benchmarks and unit tests docs consolidated in the root `_SDLC`. |
| 30 | Add tests for ReflectionCache modes | Done | File and memory caches validated |
| 31 | Summarise coverage in root README | **Done** | Added high level overview. |
| 32 | Add automated console output test for StreamingLoggingSample | Done | Implemented using `Microsoft.Extensions.Hosting` test host. |
| ML1 | Test ML middleware performance impact | Planned | Measure latency with 10% sampling and 1&nbsp;MB batches. |
| ML2 | Validate Event Hubs ingestion end-to-end | Planned | Run integration tests against developer namespace. |
| EX-05 | Preliminary review of Testing folder | Closed | See [TodoList](../TodoList.md) |

All tests are executed with `dotnet test`. Continuous integration will run these in the future when task 21 of `DevTodo.md` is complete.
To focus on a specific subset during development you can apply standard xUnit filtering:
```bash
dotnet test --filter Category=Streaming
```
## Unit Test Summary
| Test | Purpose | Status |
|------|---------|-------|
| `AiCommandMiddlewareTests` | Decision service short-circuits or permits commands | Pass |
| `DispatcherErrorTests` | Null arguments and missing handler errors | Pass |
| `DynamicDispatchTests` | Runtime dispatch of commands, queries, events and streams | Pass |
| `EventHandlerTests` | Base event handler invocation | Pass |
| `EventPublisherConfigurationTests` | Selects publisher via configuration | Pass |
| `EventPublisherTests` | Parallel vs sequential publishing performance | Pass |
| `ExceptionMiddlewareTests` | Custom exception pipelines | Pass |
| `GeneratorCacheTests` | Generator output caching | Pass |
| `GenericHandlerRegistrationTests` | Open and closed generic handlers | Pass |
| `GenericMiddlewareTests` | Middleware order and constraints | Pass |
| `LoggingCommandMiddlewareTests` | Logs execution start and end | Pass |
| `LoggingConfigurationTests` | Logging enabled or disabled via configuration | Pass |
| `OrchestratorHostTests` | Service startup ordering, retries and timeouts | Pass |
| `PrePostProcessorTests` | Command preprocessing and postprocessing | Pass |
| `QueryDispatchTests` | Typed and dynamic query dispatch | Pass |
| `ReflectionCacheTests` | Reflection registration caching | Pass |
| `RegistrationModeTests` | Reflection vs generated registration | Pass |
| `ResilienceMiddlewareTests` | Polly and pipeline retry middleware | Pass |
| `RetryCommandMiddlewareTests` | Command retry middleware | Pass |
| `RetryConfigurationTests` | Retry middleware configuration | Pass |
| `StreamingTests` | In-memory and file stream roundtrip | Pass |
| `UnitTests` | Equality semantics of <code>Unit</code> | Pass |
| `LoggingCommandMiddlewareTests` | Ensures command logging middleware records execution start and end | Pass |
| `RetryCommandMiddlewareTests` | Validates retry policy executes the handler until success | Pass |
| `AiCommandMiddlewareTests` | Checks command interception by decision service | Pass |
| `StreamingTests` | Confirms in-memory and file stream roundtrip | Pass |
| `OrchestratorHostTests` | Validates concurrency limits, priorities, retries and multiple instances | Pass |
| `GeneratorCacheTests` | Ensures source generator cache prevents duplicate output | Pass |
| `DynamicDispatchTests` | Verifies runtime handler resolution for commands, queries, events and streams | Pass |
| `QueryDispatchTests` | Tests typed and dynamic query dispatch | Pass |
| `ResilienceMiddlewareTests` | Validates Polly and pipeline retry middlewares | Pass |
| `ReflectionCacheTests` | Ensures reflection-based registration caching | Pass |
| `OrchestratorHostTests` | Validates concurrency limits and delays | Pass |
| `PollyConfigurationTests` | Registers Polly middleware via configuration | Pass |
| `PipelineConfigurationTests` | Registers resilience pipeline via configuration | Pass |


All tests are executed with `dotnet test`.

## Thanks
Testing guidance was informed by discussions in the Roslyn and ASP.NET GitHub communities. Notable resources:
- [Microsoft.CodeAnalysis.Testing samples](https://github.com/dotnet/roslyn-sdk/tree/main/src/Microsoft.CodeAnalysis.Testing) – illustrates generator test patterns.
- [Discussion on caching strategies](https://github.com/dotnet/roslyn/issues/65552) – explains benefits of hash based caches.
- [How to limit concurrency with SemaphoreSlim](https://learn.microsoft.com/dotnet/standard/parallel-programming/how-to-limit-the-degree-of-concurrency-in-a-task-parallel-library-application) – guidance for the orchestrator tests.
- [MediatR issue #537](https://github.com/jbogard/MediatR/issues/537) – motivates source generator based handler registration used in tests.
- [Optimising source generators with incremental models](https://andrewlock.net/optimising-source-generators-using-incremental-generators/) – community walkthrough on performance improvements.
- [Incremental generators documentation](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md) – explains design considerations and caching behaviour.
- [Task.WhenAll documentation](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.whenall) – used to aggregate service startup tasks.
- [CancellationTokenSource.CancelAfter documentation](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtokensource.cancelafter) – demonstrates per-service startup timeouts.
- [Polly retry documentation](https://github.com/App-vNext/Polly/wiki/Retry) – background for resilience tests.
- [Resilience pipeline package](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.Resilience) – describes pipeline behavior.
- [IHostedService restart guidance](https://github.com/dotnet/runtime/issues/36063) – discusses restarting hosted services on failure.
- [Channels documentation](https://learn.microsoft.com/dotnet/standard/parallel-programming/system-threading-channels) – shows patterns for concurrent producers.
- [Polly issue #601](https://github.com/App-vNext/Polly/issues/601) – discusses default retry handling for transient faults.
- [dotnet/runtime issue #88419](https://github.com/dotnet/runtime/issues/88419) – highlights usage of the new resilience pipeline.





