# Analysis

The benchmark project measures the dispatcher performance compared to MediatR. It focuses on command, event and streaming scenarios to track non-functional requirements such as throughput and memory allocation.

Key points:
- Evaluate the benefit of compile-time handler registration.
- Capture latency differences between custom middleware and MediatR pipelines.
- Provide reproducible metrics using BenchmarkDotNet.

Use cases:
- Validate the impact of source generators on dispatch speed.
- Compare middleware overhead with MediatR pipelines.
- Profile streaming scenarios for latency and allocation checks.

