# Ark.Alliance.Core.Mq.RabbitMq.Test

Integration tests validating the RabbitMQ messaging helpers. The suite spins up
a disposable broker using **Testcontainers** so tests do not depend on local
infrastructure. The project targets `Testcontainers` version `4.6.0` and
implements the practices described in
[`testcontainers_dotnet_9.Md`](../../../_SDLC/04_Testing/testcontainers_dotnet_9.Md).

`PublishConsumeTests` verifies that `RabbitMqPublisher`, `RabbitMqConsumer`,
`RabbitMqConnection` and the `IChannelPool` behave correctly against a live
container. The channel pool size is asserted and messages are published and
consumed end-to-end. Tests are decorated with `DockerAvailableFact` which skips
execution when Docker isn't running.

CI executes these tests through the workflow defined in
`.github/workflows/dotnet.yml`, which installs Docker and runs `dotnet test`.
See the SDLC document
[`testcontainers_dotnet_9.Md`](../../../_SDLC/04_Testing/testcontainers_dotnet_9.Md)
for the full integration guide and CI configuration.

To run these tests locally or in CI:

```bash
dotnet test Ark.Alliance.Core.Mq/Ark.Alliance.Core.Mq.RabbitMq.Test/Ark.Alliance.Core.Mq.RabbitMq.Test.csproj
```

If the container fails to start, check the troubleshooting notes in
[`testcontainers_troubleshooting.md`](../../../_SDLC/04_Testing/testcontainers_troubleshooting.md).
