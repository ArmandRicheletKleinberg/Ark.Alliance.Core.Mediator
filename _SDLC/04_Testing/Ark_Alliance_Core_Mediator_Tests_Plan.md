# Test Plan

All tests run with `dotnet test`.

The suite covers the following scenarios:
- `AiCommandMiddlewareTests` – command execution allowed or denied by a decision service.
- `DispatcherErrorTests` – null arguments and missing handler exceptions.
- `DynamicDispatchTests` – runtime dispatch of commands, queries, events and streams.
- `EventHandlerTests` – base event handler behaviour.
- `EventPublisherConfigurationTests` – publisher selection via configuration.
- `EventPublisherTests` – parallel versus sequential performance.
- `ExceptionMiddlewareTests` – custom exception handling pipelines.
- `GeneratorCacheTests` – generator output caching strategies.
- `GenericHandlerRegistrationTests` – open and closed generic handlers.
- `GenericMiddlewareTests` – middleware invocation order and constraints.
- `LoggingCommandMiddlewareTests` – logging middleware records execution.
- `LoggingConfigurationTests` – enabling or disabling logging via configuration.
- `OrchestratorHostTests` – service startup ordering, retries and timeouts.
- `PrePostProcessorTests` – command pre- and post-processing hooks.
- `QueryDispatchTests` – typed and dynamic query dispatch.
- `ReflectionCacheTests` – reflection registration caching.
- `RegistrationModeTests` – reflection vs generated registration modes.
- `ResilienceMiddlewareTests` – Polly and pipeline retry policies.
- `RetryCommandMiddlewareTests` – command retry middleware.
- `RetryConfigurationTests` – configuring retry middleware options.
- `StreamingTests` – in-memory and file stream roundtrips.
- `UnitTests` – semantics of the `Unit` struct.

Tests can be filtered by trait, e.g. `dotnet test --filter Category=Streaming` to run only streaming scenarios.
