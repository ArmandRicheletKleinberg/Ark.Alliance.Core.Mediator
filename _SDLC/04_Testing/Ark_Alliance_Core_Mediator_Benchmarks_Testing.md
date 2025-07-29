# Testing

Run the benchmarks in release mode to gather metrics:
```bash
dotnet run -c Release --project Ark.Alliance.Core.Mediator.Benchmarks
```
Filter by class when focusing on a specific scenario:
```bash
dotnet run -c Release --filter StreamingBenchmarks
```

For profiling with **dotTrace**, append the diagnoser argument:
```bash
dotnet run -c Release -- --profiler dottrace
```

