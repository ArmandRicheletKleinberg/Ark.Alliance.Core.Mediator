# RabbitMQ Module Todo

This document tracks the refactoring work required to align the RabbitMQ integration with the `rabbit_client_7_1_2_guide.md` recommendations.

| ID | Task | Status | Comment | Prompt |
|----|------|-------|---------|-------|
| R1 | Migrate publisher to fully async API (`BasicPublishAsync`/`WaitForConfirmsAsync`). | Done | Implemented in `RabbitMqPublisher`. | "Update publisher to use async RabbitMQ API" |
| R2 | Fix BasicProperties creation and add cancellation tokens. | Done | `RabbitMqPublisher` now creates properties via `CreateBasicProperties`. | "Ensure BasicProperties are instantiated correctly" |
| R3 | Use async consumer API with `BasicConsumeAsync` and metrics. | Done | Consumer fully async and metrics recorded. | "Finish migrating consumer to async API" |
| R4 | Add network recovery interval option to settings and use in connection factory. | Done | Connection factories now enable automatic recovery and configurable intervals. | "Add NetworkRecoveryInterval to RabbitMqSettings" |
| R5 | Create integration tests using TestContainers. | Done | Basic publish/consume test implemented. | "Write integration tests using Testcontainers" |
| R6 | Add `ChannelPoolSize` option and limit channels with `SemaphoreSlim`. | Done | `RabbitMqChannelPool` now enforces pool size. | "Implement bounded channel pool" |
| R7 | Introduce `EnableOpenTelemetry` flag to toggle metrics registration. | Done | Service registration conditional on this flag. | "Add OpenTelemetry toggle" |
| R8 | Expose `IChannelPool` abstraction for easier testing. | Done | All services depend on the interface. | "Create IChannelPool interface" |
| R9 | Honor `ConfirmTimeoutSeconds` when waiting for publisher confirms. | Done | Timeout applied in `RabbitMqPublisher`. | "Add confirm timeout support" |
| R10 | Introduce `IConnectionManager` abstraction and async disposal. | Done | `RabbitMqConnection` implements `IConnectionManager`. | "Create IConnectionManager interface" |
| R11 | Update `RabbitMqChannelPool` to use the new interface. | Done | Constructor now accepts `IConnectionManager`. | "Inject IConnectionManager into channel pool" |
| R12 | Remove unnecessary generic constraints from `RabbitMqBrokerConsumer`. | Done | Matches `IBrokerConsumer` contract. | "Fix generic constraints" |
| R13 | Align test project package reference to available version. | Done | Test project now references `Testcontainers` 4.6.0. | "Fix test project dependencies" |

