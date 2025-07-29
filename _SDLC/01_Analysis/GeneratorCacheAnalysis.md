# Handler Registration Generator - Analysis

This note summarises the current design of the `HandlerRegistrationGenerator` and future improvements.

## Overview
The generator scans for implementations of `ICommandHandler`, `IQueryHandler`, `IEventHandler` and `IStreamRequestHandler`. It emits a partial method used by `AddArkMessaging` to register these handlers without reflection. A hash based cache avoids generating identical code on subsequent compilations.

## References
- [Source generators overview](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Incremental generator performance](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/incremental-generators)

## To-Do
| ID | Task | Status | Notes |
|----|------|--------|-------|
| G1 | Benchmark memory vs file cache modes | Open | Use BenchmarkDotNet in `Ark.Alliance.Core.Mediator.Benchmarks` |
| G2 | Add Roslyn SDK tests for cache skip logic | Open | Validate behaviour using Microsoft.CodeAnalysis.Testing |
| G3 | Support runtime toggle via `HandlerRegistration` option | Done | `ArkMessagingOptions` exposes `HandlerRegistrationMode` |
| G4 | Implement reflection cache in runtime options | Done | `ArkMessagingOptions.ReflectionCache` with Memory/File modes |
| G5 | Document community references about generator caching | Done | Links added below |
| G6 | Allow cache mode override via environment variables | Done | Build systems can set `ARK_MEDIATOR_GENERATOR_CACHE_MODE` |
| G7 | Use async file I/O for reflection cache | Done | Improves startup when cache file is large |

## Thanks
This analysis references the following community resources and issue discussions:
- [Incremental generator performance](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/incremental-generators)
- [Source generators overview](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [GitHub issue on generator caching strategies](https://github.com/dotnet/roslyn/issues/65552)
- [Optimising source generators with incremental models](https://andrewlock.net/optimising-source-generators-using-incremental-generators/)

Special thanks to contributors on the Roslyn repository for clarifying caching best practices.
