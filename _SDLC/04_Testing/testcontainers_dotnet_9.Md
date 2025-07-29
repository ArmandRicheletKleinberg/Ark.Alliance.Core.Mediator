#  Testcontainers for .NET 9 – Complete Integration Guide

**Package**: `Testcontainers` **Version**: `4.6.0` **Target**: `.NET 9` **Scope**: Integration Testing with containerized dependencies (PostgreSQL, MongoDB, RabbitMQ, SQL Server, Redis, Elasticsearch, Kafka)

---

## Table of Contents

1. [Overview](#overview)
2. [Installation](#installation)
3. [Supported Environments](#supported-environments)
4. [Core Concepts](#core-concepts)
5. [Single-Service Examples](#single-service-examples)
6. [Multi-Service Integration](#multi-service-integration)
7. [Using IAsyncLifetime](#using-iasynclifetime)
8. [Wait Strategies](#wait-strategies)
9. [Advanced Utilities](#advanced-utilities)
10. [Parallel Test Execution](#parallel-test-execution)
11. [WebApp Integration Example](#webapp-integration-example)
12. [CI/CD Integration](#cicd-integration)
13. [Security Best Practices](#security-best-practices)
14. [Roadmap Highlights](#roadmap-highlights)
15. [Resources](#resources)

---

## Overview

[Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet) allows .NET developers to spin up disposable containers for integration testing. This enables robust test suites that include real services without requiring local or pre-installed environments.

---

## Installation

```bash
dotnet add package Testcontainers --version 4.6.0
```

NuGet: [Testcontainers](https://www.nuget.org/packages/Testcontainers/)

---

## Supported Environments

- Windows, macOS, Linux
- Docker Desktop or compatible runtimes
- Azure DevOps, GitHub Actions, GitLab CI
- .NET Core 3.1+, .NET 6, 7, 8, 9

---

## Core Concepts

| Concept                             | Description                                      |
| ----------------------------------- | ------------------------------------------------ |
| `TestcontainersBuilder`             | Fluent API to configure containers               |
| `WithImage`                         | Specifies container image                        |
| `WithEnvironment`                   | Sets env variables like passwords                |
| `WithPortBinding`                   | Maps container port to random/explicit host port |
| `WithWaitStrategy`                  | Wait for container readiness via ports/logs      |
| `WithBindMount`                     | Mounts host directories                          |
| `WithCommand`                       | Override container entrypoint                    |
| `.StartAsync()` / `.DisposeAsync()` | Start and clean up containers                    |

---

## 📅 Single-Service Examples

### PostgreSQL

```csharp
var postgres = new TestcontainersBuilder<PostgreSqlTestcontainer>()
  .WithDatabase(new PostgreSqlTestcontainerConfiguration { Password = "pw" })
  .WithImage("postgres:15-alpine")
  .WithPortBinding(5432, true)
  .Build();
```

### MongoDB

```csharp
var mongo = new TestcontainersBuilder<MongoDbTestcontainer>()
  .WithDatabase(new MongoDbTestcontainerConfiguration())
  .WithImage("mongo:6")
  .WithPortBinding(27017, true)
  .Build();
```

### RabbitMQ

```csharp
var rabbit = new TestcontainersBuilder<RabbitMqTestcontainer>()
  .WithImage("rabbitmq:3-management")
  .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
  .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
  .WithPortBinding(5672, true)
  .WithPortBinding(15672, true)
  .Build();
```

### Redis

```csharp
var redis = new TestcontainersBuilder<RedisTestcontainer>()
  .WithImage("redis:7")
  .WithPortBinding(6379, true)
  .Build();
```

### SQL Server

```csharp
var sql = new TestcontainersBuilder<MsSqlTestcontainer>()
  .WithDatabase(new MsSqlTestcontainerConfiguration { Password = "Strong!Pass123" })
  .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
  .WithPortBinding(1433, true)
  .Build();
```

### Elasticsearch

```csharp
var elastic = new TestcontainersBuilder<ElasticsearchTestcontainer>()
  .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.9.0")
  .WithEnvironment("discovery.type", "single-node")
  .WithEnvironment("xpack.security.enabled", "false")
  .WithPortBinding(9200, true)
  .Build();
```

### Kafka

```csharp
var kafka = new TestcontainersBuilder<KafkaTestcontainer>()
  .WithImage("confluentinc/cp-kafka:7.4.0")
  .WithPortBinding(9092, true)
  .Build();
```

---

## 🌐 Multi-Service Integration

Demonstration of PostgreSQL, RabbitMQ, and Elasticsearch used together:

```csharp
// See MultiServiceIntegrationTests in previous code section for full implementation.
```

---

##  Using IAsyncLifetime

Attach to test class to manage lifecycle:

```csharp
public class IntegrationTests : IAsyncLifetime
{
  // Setup containers
  public async Task InitializeAsync() => await container.StartAsync();
  public async Task DisposeAsync() => await container.DisposeAsync();
}
```

---

##  Wait Strategies

Wait for ports:

```csharp
.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
```

Wait for log message:

```csharp
.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("ready"))
```

---

## Advanced Utilities

- Execute commands inside:

```csharp
await container.ExecAsync(["psql", "-c", "SELECT 1"]);
```

- Bind mounts:

```csharp
.WithBindMount("/host", "/container", AccessMode.ReadOnly)
```

- Override entrypoint:

```csharp
.WithCommand("postgres", "-c", "log_statement=all")
```

---

## 📶 Parallel Test Execution

To avoid conflicts with shared containers:

```csharp
[CollectionDefinition("Shared", DisableParallelization = true)]
```

---

## WebApp Integration Example

See `MyWebAppTests` example in prior section for `.NET 9` host integration with PostgreSQL container and runtime injection.

---

## CI/CD Integration

### GitHub Actions

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      docker:
        image: docker:24
        options: --privileged
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet test
```

---

##  Security Best Practices

- Avoid `latest` tag in images
- Use secrets in environment values
- Isolate container network if needed

---

##  Roadmap Highlights

- Kubernetes support via k3s
- Integration with FluentMigrator
- .NET 9 extended support

---

##  Resources

- GitHub: [testcontainers-dotnet](https://github.com/testcontainers/testcontainers-dotnet)
- Docs: [dotnet.testcontainers.org](https://dotnet.testcontainers.org)
- License: MIT

---

**Author**: Armand Richelet-Kleinberg

