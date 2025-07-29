# Design

The tests follow Arrange‑Act‑Assert. Fixtures configure a fresh `ServiceProvider` for each scenario. A simplified UML sequence diagram illustrates a typical test flow.

```mermaid
sequenceDiagram
    participant Test
    participant DI as ServiceProvider
    participant Disp as IArkDispatcher
    participant Log as Logging
    participant AI as Decision
    participant Ret as Retry
    participant Pre as PreProcessor
    participant Hand as Handler
    participant Post as PostProcessor
    Test->>DI: Resolve Disp
    Test->>Disp: Send(Command)
    Disp->>Log: begin
    Disp->>AI: evaluate
    AI-->>Disp: allow/deny
    Disp->>Ret: policy
    Ret->>Pre: run
    Pre-->>Ret: command
    Ret->>Hand: handle
    Hand-->>Ret: result
    Ret->>Post: run
    Post-->>Ret: result
    Ret-->>Log: final
    Ret-->>Disp: Result
    Disp-->>Test: Assert
```

```mermaid
classDiagram
    class TestFixture
    class ServiceProvider
    class IArkDispatcher
    class LoggingCommandMiddleware
    class AiCommandMiddleware
    class RetryCommandMiddleware
    class PreProcessor
    class PostProcessor
    class OrchestratorHost
    TestFixture --> ServiceProvider
    ServiceProvider --> IArkDispatcher
    IArkDispatcher --> LoggingCommandMiddleware
    LoggingCommandMiddleware --> AiCommandMiddleware
    AiCommandMiddleware --> RetryCommandMiddleware
    RetryCommandMiddleware --> PreProcessor
    RetryCommandMiddleware --> PostProcessor
    TestFixture --> OrchestratorHost
    OrchestratorHost --> IArkDispatcher
```

Pre- and post-processors allow modification of the command and resulting `Result` after the retry logic executes. This design mirrors the production pipeline so tests reflect real usage scenarios.
