# Implementation

Main components:
- `Benchmarks.cs` and `ComparisonBenchmarks.cs` define scenarios for commands and events.
- `StreamingBenchmarks.cs` measures `CreateStream` performance.
- Middleware samples (`GenericCommandMiddleware`, `GenericEventMiddleware`) simulate pipeline overhead.
- `Program.cs` invokes `BenchmarkSwitcher` to run selected benchmarks.

Project structure:
```
Ark.Alliance.Core.Mediator.Benchmarks/
|-- Benchmarks.cs
|-- ComparisonBenchmarks.cs
|-- StreamingBenchmarks.cs
|-- GenericCommandMiddleware.cs
|-- GenericEventMiddleware.cs
|-- GenericPipelineBehavior.cs
|-- Program.cs
|-- Ark.Alliance.Core.Mediator.Benchmarks.csproj
```

