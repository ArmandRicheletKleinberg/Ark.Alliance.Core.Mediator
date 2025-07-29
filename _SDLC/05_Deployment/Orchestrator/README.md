# Orchestrator Deployment Guide

This document explains how to build and run the orchestrator service from **Ark.Alliance.Core.Mediator** using Docker. It also lists the configuration file structure and links to related documentation. For the broader SDLC context see the [deployment overview](../README.md).

## Index
- [Building the container](#building-the-container)
- [Running the container](#running-the-container)
- [Sample configuration](#sample-configuration)
- [To-Do](#to-do)

## Building the container
```bash
docker build -t ark-orchestrator -f Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Services.Orchestrator/Dockerfile .
```

## Running the container
```bash
docker run --rm -v $(pwd)/orchestrator-config:/app/config ark-orchestrator
```

Mount a folder containing `appsettings.json` under `/app/config`. A sample configuration file is provided below.

## Sample configuration
```json
{
  "MainApplication": {
    "Databases": {
      "Default": {
        "ConnectionString": "Host=db;Port=5432;Database=ark;User Id=ark;Password=changeme",
        "Migrate": true
      }
    },
    "Authorization": {
      "Type": "Bearer",
      "Service": "Auth0"
    }
  },
  "Brokers": {
    "RabbitMQ": {
      "Host": "rabbitmq",
      "Port": 5672
    }
  },
  "Services": {
    "Sample": {
      "Version": "1.0",
      "Roles": ["Consumer"],
      "Broker": "RabbitMQ",
      "Events": {
        "Publish": ["order.created"],
        "Subscribe": [
          { "Name": "order.updated" }
        ]
      }
    }
  }
}
```

The orchestrator loads this file and configures services accordingly. Each service can define a broker, schedule and events to publish or subscribe to.

## To-Do
| ID | Task | Status |
|----|------|--------|
| ORC1 | Document Helm chart packaging | Open |
| ORC2 | Provide sample CI/CD pipeline for container image | Open |
