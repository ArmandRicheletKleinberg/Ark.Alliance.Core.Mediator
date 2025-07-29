# 05_Deployment
Deployment scripts and infrastructure configuration.

## Index
- [Orchestrator Guide](Orchestrator/README.md)
- [ML Pipeline Deployment](ML/README.md)


## To-Do
| ID | Task | Status | Notes |
|----|------|--------|-------|
| 7 | Provide orchestrator sample documentation and deployment guides | Closed | Guide located in `Orchestrator/README.md`. |
| 13 | Finalize orchestrator service README and sample configuration | Closed | Configuration schema and Docker usage documented. |
| 19 | Update prerequisites document with containerization and Kubernetes setup | Closed | Documented in `_SDLC/00_Prerequisites/README.md`. |
| ML1 | Provide Bicep templates for ML pipeline resources | **Done** | Template added in [`ML/ml_pipeline.bicep`](ML/ml_pipeline.bicep). |
| ML2 | Document secure secret storage with Key Vault | **Done** | Explained in [`ML/README.md`](ML/README.md). |
| EX-06 | Preliminary review of Deployment folder | Closed | See [TodoList](../TodoList.md) |

## Environments
| Stage | Deploy Method | Notes |
|-------|---------------|------|
| Development | `docker compose up` | Spins up RabbitMQ and orchestrator locally |
| QA | Kubernetes (Helm charts) | Mirrors production topology |
| Production | Kubernetes (Helm charts) | Horizontal scaling and rolling upgrades |

### Orchestrator Container
The orchestrator service is built from `Services.Orchestrator/Dockerfile` and coordinates broker connections.
Run:
```bash
docker build -t ark-orchestrator -f Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Services.Orchestrator/Dockerfile .
docker run --rm ark-orchestrator
```
See [Orchestrator](Orchestrator/README.md) for configuration details.
