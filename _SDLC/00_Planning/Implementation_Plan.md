# Implementation Plan

## Backlog
The initial backlog is prioritized using the MoSCoW method.

| ID | Item | Priority | Status | Notes |
|----|------|----------|--------|-------|
| 1 | Implement `IArkDispatcher` interface | Must | Closed | Core dispatcher implemented. |
| 2 | Develop Roslyn source generator for handler registration | Must | Closed | `HandlerRegistrationGenerator` shipped. |
| 3 | Integrate Polly for resilience | Must | Closed | Retry middleware available. |
| 4 | Add optional AI middleware using Mistral | Should | In Progress | Implement pluggable decision service and configuration toggle. |
| 5 | Support RabbitMQ and Kafka adapters | Should | Open | Basic abstractions exist, adapters pending. |
| 6 | Prototype ML data ingestion using Event Hubs | Should | In Progress | Measure throughput, encryption overhead and batching impact. |
| 7 | Set up Data Lake and vector store for training data | Should | Planned | Configure lifecycle policies, RBAC and nightly indexing. |

(Track additional items in Jira: STORY-101 etc.)

## PoC Repository
- **Repository**: <https://github.com/ArkAlliance/Ark.Alliance.Core.Mediator-PoC>
- **Setup**: Initialize with .NET 8 solution and projects for Core and Generators.
- **Branches**: `main` for stable, `feature/poc` for experiments.
