# 06_Maintenance
Post-release maintenance notes and improvement plans.

## Index
- [Release Schedule](#release-schedule)


## To-Do

| ID | Task | Status | Notes |
|----|------|--------|-------|
| 15 | Audit .NET 9 features adoption (file-scoped namespaces, primary constructors) | **In Progress** | ResiliencePipeline stubs updated with primary constructors. |
| 20 | Review messaging and IoC classes for style and security | **Open** | Evaluate error handling and adopt consistent naming. |
| 23 | Implement per-service logging toggle using `LoggerSettings` | Open | Apply during orchestrator startup. |
| ML1 | Monitor Event Hubs throughput and adjust partitions | Planned | Provide runbook and automated alerts for scaling. |
| ML2 | Review model accuracy quarterly | Planned | Retrain with latest data and refresh monitoring dashboards. |
| EX-07 | Preliminary review of Maintenance folder | Closed | See [TodoList](../TodoList.md) |

## Release Schedule
| Version | Date | Notes |
|---------|------|------|
| 0.1 | 2025-08 | Initial open source release |
| 0.2 | 2025-10 | Streaming providers and retry middleware |

Bug fixes are handled on a Weekly cadence. Security updates are prioritized and may trigger out-of-band releases.

