# ML Pipeline Deployment

This folder contains infrastructure templates for the optional machine learning pipeline. For a full deployment overview see the [deployment README](../README.md).

## Index
- [Bicep template](#bicep-template)
- [Deployment](#deployment)
- [To-Do](#to-do)

## Bicep template
The `ml_pipeline.bicep` file provisions:

- **Event Hubs namespace** and a `telemetry` hub to ingest encrypted payloads.
- **Storage account** enabled for hierarchical namespaces to store raw data.
- **Key Vault** for secrets such as the Event Hubs connection string.

Deploy the resources into a resource group:

```bash
az deployment group create \
  --resource-group my-rg \
  --template-file ml_pipeline.bicep
```

Use the `enableMl` parameter to skip deployment when the ML pipeline is disabled.

## To-Do
| ID | Task | Status |
|----|------|--------|
| MLDEP1 | Provide Terraform equivalent examples | Open |
| MLDEP2 | Document CI/CD pipeline for Bicep deployment | Open |

