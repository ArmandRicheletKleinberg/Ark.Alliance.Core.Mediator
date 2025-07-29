# Implementation

`Program.cs` configures the mediator and registers a command handler returning an `IAsyncEnumerable<int>`. Logging and retry middleware are enabled via the built-in `ILogger` and resilience extensions.

```csharp
builder.Services.AddLogging();
builder.Services.AddArkMessaging(typeof(Program).Assembly);
builder.Services.AddArkMediator();
builder.Services.AddRetryCommandMiddleware();
```
The `PingCommandHandler` demonstrates basic command processing:
```csharp
public class PingCommandHandler : ICommandHandler<PingCommand, string>
{
    public Task<Result<string>> HandleAsync(PingCommand command, CancellationToken ct)
        => Task.FromResult(Result<string>.Success.WithData("Pong"));
}
```
`NumberStreamHandler` yields numbers asynchronously to showcase streaming.
The program writes the ping result to the console and then starts another stream
using a `CancellationTokenSource` to demonstrate cooperative cancellation.
The included Dockerfile builds a small image so the sample can run in isolation using `docker run`. No additional runtime dependencies are required. See the [solution implementation notes](../../03_Implementation/README.md) for broader context.

```bash
docker build -t streaming-sample .
docker run --rm streaming-sample
```
