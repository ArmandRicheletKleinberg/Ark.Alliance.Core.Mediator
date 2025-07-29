# 02_Design

High level architecture and design documents. It details the UML diagrams referenced from the [root README](../README.md) and explains how source generators replace reflection to register handlers.
These notes ensure the solution remains aligned with DDD, Event‑Driven and Clean Architecture principles.

See [`Samples/StreamingLoggingSample`](../Samples/StreamingLoggingSample) for a reference project with simplified diagrams that complement this design overview.
Design notes for the handler registration generator are located under
[`Ark.Alliance.Core.Mediator.Generators/_Sdlc`](../../Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Generators/_Sdlc).

## Index
- [Architecture Specification](Architecture_Spec.md)
- [Benchmarks Design](Ark_Alliance_Core_Mediator_Benchmarks_Design.md)
- [Tests Design](Ark_Alliance_Core_Mediator_Tests_Design.md)


## Diagrams
See [Architecture_Spec.md](Architecture_Spec.md) for C4 diagrams and generator specifications.

## To-Do
| ID | Task | Status | Notes |
|----|------|--------|-------|
| 6 | Update design docs with UML/C4 diagrams and benchmark results | Closed | UML diagram added in Architecture_Spec.md. |
| 8 | Eliminate runtime reflection in dispatcher streaming and broker registration | Open | Requires additional source generators. |
| 14 | Verify dispatcher reflection usage is replaced by source generators | Open | Depends on tasks 11 and 12. |
| 15 | Audit .NET 9 features adoption | In Progress | ResiliencePipeline stubs converted to file-scoped namespaces and primary constructors. |
| 16 | Review IoC and messaging namespaces for DDD compliance | In Progress | Naming updates under review. |
| 17 | Document DI registration sequence | Closed | Added "AddArkCqrs" sequence diagram. |



| ID | Task | Status | Notes |
|----|------|--------|-------|
| 3 | Confirm generator based handler registration | **Closed** | `HandlerRegistrationGenerator` emits service registrations. |
| 6 | Update design docs with UML/C4 diagrams and benchmark results | **Closed** | UML diagram documented in Architecture_Spec.md. |
| 8 | Eliminate runtime reflection in dispatcher streaming and broker registration | **Open** | Design source generators to map stream handlers and broker registrars. |
| 11 | Remove dynamic dispatch in `ArkDispatcher.CreateStream` | **Open** | Generate strongly typed stream handler invocations. |
| 12 | Pre-generate broker registrations to eliminate reflection in IoC | **Open** | Discover `IArkBrokerRegistrar` implementations at compile time. |
| 14 | Verify dispatcher reflection usage is replaced by source generators | **Open** | Ensure all dynamic calls are compiled mappings. |
| 16 | Review IoC and messaging namespaces for DDD compliance | **In Progress** | Remaining work: update generator namespaces and fix reflection usage. |
| 18 | Document overall solution diagram in root README | **Done** | Linked projects via Mermaid diagram. |
| ML1 | Design data capture middleware sequence | In Progress | Outline async queueing, per-command sampling and encrypted payloads. |
| ML2 | Specify Azure storage architecture for training data | In Progress | Map Event Hubs, Data Lake and vector DB with network and SKU options. |
| EX-03 | Preliminary review of Design folder | Closed | See [TodoList](../TodoList.md) |

See `Architecture_Spec.md` for diagrams and detailed design notes.

