# Orchestrator Analysis Tasks

| ID | Task | Status | Notes |
|----|------|--------|-------|
| OR1 | Design concurrency options for service startup | Done | Implemented `MaxDegreeOfParallelism` property referencing [SemaphoreSlim docs](https://learn.microsoft.com/dotnet/standard/parallel-programming/how-to-limit-the-degree-of-concurrency-in-a-task-parallel-library-application). |
| OR2 | Provide health-check toggle | Done | Added `EnableHealthChecks` boolean in settings. |
| OR3 | Add start delay and enable flag per service | Done | `StartupDelaySeconds` and `Enabled` introduced. |
| OR4 | Implement orchestrator host class | Done | `OrchestratorHost` manages parallel startup. |
| OR5 | Extend unit tests for orchestrator | Done | New `OrchestratorHostTests` cover concurrency and delay logic. |
| OR6 | Allow start order via priority setting | Done | `Priority` property controls sequence. |
| OR7 | Fail-fast option when a service fails | Done | Added `StopOnError` guided by [Task.WhenAll docs](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.whenall). |
| OR8 | Per-service startup timeout | Done | Uses `CancellationTokenSource.CancelAfter`. |
