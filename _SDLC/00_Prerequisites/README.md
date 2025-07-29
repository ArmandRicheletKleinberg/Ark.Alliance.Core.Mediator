# 00_Prerequisites

This folder captures the baseline environment required to build and test **Ark.Alliance.Core.Mediator**.

## Index
- [Prerequisite Checklist](#prerequisite-checklist)

## Requirements
- .NET 9 SDK and runtime
- Visual Studio 2022 or VS Code with the C# Dev Kit extension
- Docker Engine for containerized samples
- Optional message brokers: RabbitMQ and Kafka for integration testing

## Prerequisite Checklist

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET SDK  | 9.0+    | Build and execute Ark mediator libraries |
| Visual Studio / VS Code | 2022+ | IDE for development and debugging |
| Docker Engine | 23+ | Run samples and brokers locally |
| RabbitMQ | 3.12+ | Optional broker integration tests |
| Kafka | 3.x | Optional streaming broker |

Additional tools (such as Mistral AI models and Kubernetes) may be installed depending on feature usage.

## Setup
1. **Install .NET 9 SDK** from [dotnet.microsoft.com](https://dotnet.microsoft.com/).
2. **Configure an IDE**: install Visual Studio 2022 or VS Code with the *C# Dev Kit*.
3. **Enable Docker** and pull broker containers:
   ```bash
   docker pull rabbitmq:3-management
   docker pull confluentinc/cp-kafka:latest
   ```
4. **Set environment variables** for broker connection strings as needed:
   - `RabbitMq__Host=localhost`
   - `Kafka__BootstrapServers=localhost:9092`
5. **Run tests** with `dotnet test` to verify the toolchain.
6. Review security policies and apply organization guidelines for credential storage.
7. **Encrypt secrets** such as connection strings with your preferred key vault
   solution (e.g., Azure Key Vault or HashiCorp Vault). Set environment variables
   like `RabbitMq__Password` or `Kafka__SaslPassword` from your secret store and
   avoid committing them into source control.

## Containerization

The project can be evaluated locally with Docker Compose. The following example
starts RabbitMQ and the orchestrator service:

```yaml
version: "3.8"
services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
  orchestrator:
    build:
      context: ../..
      dockerfile: Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Services.Orchestrator/Dockerfile
    volumes:
      - ./orchestrator-config:/app/config
    depends_on:
      - rabbitmq
```

Run it with `docker compose up` and browse to `http://localhost:15672` for the
RabbitMQ management UI.

## Kubernetes Setup

If you deploy to Kubernetes, install `kubectl` and apply manifests as follows:

```bash
kubectl apply -f k8s/
```

The `k8s` folder should contain manifests for brokers, services and deployments.

## To-Do

| ID | Task | Status | Notes |
|----|------|--------|-------|
| 2 | Create SDLC prerequisites document | **Closed** | Documented baseline environment and encryption guidelines. |
| 19 | Update prerequisites with containerization and Kubernetes setup | **Closed** | Docker Compose and kubectl examples added. |
| EX-01 | Preliminary review of prerequisites folder | Closed | See [TodoList](../TodoList.md) |
