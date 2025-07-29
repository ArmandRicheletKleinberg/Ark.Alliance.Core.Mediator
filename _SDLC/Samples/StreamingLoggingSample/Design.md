# Design

A simple command triggers a streaming handler while logging and retry middleware write to the console and handle transient faults.

```mermaid
sequenceDiagram
    participant Cli as CLI
    participant Disp as IArkDispatcher
    participant Log as Logging
    participant Ret as Retry
    participant Ping as PingCommandHandler
    Cli->>Disp: Send(StreamNumbers)
    Disp->>Log: begin
    Disp->>Ret: policy
    Ret->>Ping: Ping
    Ping-->>Disp: Pong
    Disp-->>Cli: Stream<int>
```

```mermaid
classDiagram
    class Program
    class StreamNumbersHandler
    class PingCommandHandler
    class LoggingCommandMiddleware
    class RetryCommandMiddleware
    Program --> IArkDispatcher
    IArkDispatcher --> LoggingCommandMiddleware
    LoggingCommandMiddleware --> RetryCommandMiddleware
    RetryCommandMiddleware --> StreamNumbersHandler
    RetryCommandMiddleware --> PingCommandHandler
```

The application can run inside a container using the provided Dockerfile to mirror deployment environments.
