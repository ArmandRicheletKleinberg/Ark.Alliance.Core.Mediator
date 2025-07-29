# Test Plan

- Manual run via `dotnet run` ensures numbers stream correctly.
- Verify `PingCommand` prints "Pong" to the console.
- Automated test validates console output using a test host.
- Confirm Docker image executes without errors.
- Container run via `docker build -t sample . && docker run sample` verifies environment independence.
- Copier steps can be tested by cloning the folder to a new location and running `dotnet test`.
- Tests require .NET 9 SDK and can be executed on any platform supporting containers.
The [solution testing notes](../../04_Testing/README.md) describe the broader unit and integration test strategy.
