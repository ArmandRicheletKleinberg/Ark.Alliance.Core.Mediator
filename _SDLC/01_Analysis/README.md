# 01_Analysis

This directory stores requirements and functional analysis documents. For an overview of the solution and its components see the [root README](../README.md). 
The analysis phase records feedback on MediatR limitations and defines how Ark.Alliance.Core.Mediator addresses them.

See [`Samples/StreamingLoggingSample`](../Samples/StreamingLoggingSample) for additional notes on the streaming sample.

## Index
- [DF01 V.3](DF01_V.3-20250510.md)
- [DF01 V.4](DF01_V.4-20250515.md)
- [Benchmarks Analysis](Ark_Alliance_Core_Mediator_Benchmarks_Analysis.md)
- [Tests Analysis](Ark_Alliance_Core_Mediator_Tests_Analysis.md)
- [Generator Cache Analysis](GeneratorCacheAnalysis.md)
- [Orchestrator To-Do](Orchestrator_TodoList.md)


Community feedback highlighted several issues with the traditional MediatR library: reflection-based registration slows startup, advanced logging requires extra packages and the new license key at <https://mediatr.io> introduces legal concerns. This project analyses those weaknesses and captures requirements for a faster, fully MIT licensed alternative.

## To-Do


| ID | Task | Status | Notes |
|----|------|--------|-------|
| 1 | Verify current source structure for DDD/EDA/Clean Architecture alignment | **Closed** | Project layout reviewed and follows layered approach. |
| 10 | Benchmark dispatch vs. MediatR and publish results | **Closed** | Benchmark project implemented. |
| A1 | Collect feedback from existing MediatR usage | **Open** | Survey developers and capture pain points. |
| A2 | Identify performance metrics and non-functional requirements | **Open** | Define baseline throughput and compatibility goals. |
| A3 | Review DDD boundaries and event-driven patterns | **Open** | Ensure bounded contexts and brokers meet requirements. |
| A4 | Document success criteria and stakeholder priorities | **Open** | Summarize measurable goals in ADR. |
| A5 | List reusable components and third-party libraries | **Open** | Evaluate Polly, source generator libraries. |
| A6 | Check licensing and team skills for source generators | **Open** | Confirm MIT/Apache compatibility. |
| A7 | Draft acceptance test scenarios | **Open** | Define QA validation steps. |
| A9 | Document benchmarks and tests in project READMEs | **Closed** | Added dedicated docs under `Ark.Alliance.Core.Mediator` |
| A8 | Validate IoC and Messaging documentation | **Closed** | IoC and Messaging sources reviewed; XML comments and file-scoped namespaces applied. |
| A10 | Add component tables to IoC and Messaging READMEs | **Closed** | READMEs updated with `Table of Components` section. |
| A11 | Analyse service restart scenarios and multi-instance support | **Done** | Added `RetryCount` and `InstanceCount` settings with tests. |
| A13 | Analyse data points required for ML models | Planned | Identify fields, headers and user context to capture. |
| A14 | Evaluate sampling strategy to minimize overhead | Planned | Compare rate-based, random and adaptive sampling. |
| A12 | Gather community guidance on orchestrator reliability | **Done** | Referenced GitHub issue #36063 and channels patterns. |
| EX-02 | Preliminary review of Analysis folder | Closed | See [TodoList](../TodoList.md) |



See the `DF01_*` files for detailed background analysis.
