# CI/CD Pipelines — ABB EL Common Backend

## Table of Contents

- [Overview](#overview)
- [Pipeline Inventory](#pipeline-inventory)
- [Shared Template Reference (NH-Shared)](#shared-template-reference-nh-shared)
- [Branch Strategy and Promotion Flow](#branch-strategy-and-promotion-flow)
- [Deployment Environments](#deployment-environments)
- [Build Configuration](#build-configuration)
- [Quality Gates](#quality-gates)
- [Pipeline Parameters](#pipeline-parameters)
- [Path Filtering](#path-filtering)
- [Scheduled Builds](#scheduled-builds)
- [References](#references)

---

## Overview

This repository contains **two Azure DevOps YAML pipelines** that build, test, and deploy two independent .NET 8 App Service applications from the same solution. Both pipelines extend a **shared template** hosted in the `NH-Shared` repository, ensuring consistency across the organization's deployment practices.

The workflow follows a **branch-based promotion model**: code moves from feature branches through integration testing, staging, and into production via well-defined branch gates.

---

## Pipeline Inventory

| Pipeline File | Deployable Project | SonarQube Key | BlackDuck |
|---|---|---|---|
| `NH-CommonBackendApi.yml` | `ABB.EL.Common.Api` | `ABB-Common-Backend` | Configured (skipped by default) |
| `NH-NotificationApi.yml` | `ABB.EL.Common.Notification.Api` | `ABB-Notification-API` | Not configured |

Both pipelines share the **same test project**: `ABB.EL.Common.Services.Test/ABB.EL.Common.Services.Test.csproj`.

---

## Shared Template Reference (NH-Shared)

Both pipelines delegate their entire build-deploy lifecycle to a shared template:

```yaml
resources:
  repositories:
    - repository: shared
      type: git
      name: NH-Shared
      ref: refs/heads/main

extends:
  template: devops/pipelines/templates/appService.yml@shared
```

### What the <mark>shared template</mark> provides

The `appService.yml` template from the `NH-Shared` repo encapsulates the full **WARP cycle** (Workflow for Application Release Pipeline):

| Stage | Responsibility | Key Parameters |
|---|---|---|
| **Build** | `dotnet restore` → `dotnet build` → `dotnet publish` for `mainProjectDirectory` / `mainProjectName` | `mainProjectDirectory`, `mainProjectName` |
| **Test** | Runs test projects specified in `testProjectCsprojs`; optionally blocks on failures | `skipTests`, `areTestsBlocking`, `testProjectCsprojs` |
| **Static Analysis** | SonarQube scan with project key/name | `skipSonarQube`, `sonarQubeProjectKey`, `sonarQubeProjectName` |
| **Security Scan** | Bolt dependency scanning + BlackDuck OSS analysis | `skipBolt`, `skipSecurity`, `blackDuckServiceName`, `blackDuckProjectName`, `blackDuckProjectVersion` |
| **Deploy** | Multi-environment Azure App Service deployment with branch-based conditions | `deployedEnvironments[]` |

### Parameters passed to the shared template

Each pipeline passes a standardized set of parameters:

- **`mainProjectDirectory`** / **`mainProjectName`** — Identifies the .NET project to build and publish.
- **`testProjectCsprojs`** — Semicolon-separated list of test `.csproj` paths.
- **`sonarQubeProjectKey`** / **`sonarQubeProjectName`** — SonarQube identification.
- **`deployedEnvironments`** — Array of environment definitions (see [Deployment Environments](#deployment-environments)).
- **Toggle flags** — `skipTests`, `areTestsBlocking`, `skipSonarQube`, `skipBolt`, `skipSecurity`.

---

## Branch Strategy and Promotion Flow

```
feature/*  ──────►  IntegrationFeatures  ──────►  development  ──────►  master
    │                      │                          │                    │
    ▼                      ▼                          ▼                    ▼
IntegrationTest      IntegrationTest               Stage                Prod
```

| Source Branch | Target Environment | Condition |
|---|---|---|
| `feature/*` | IntegrationTest | `startsWith(variables['Build.SourceBranch'], 'refs/heads/feature/')` |
| `IntegrationFeatures` | IntegrationTest | `eq(variables['Build.SourceBranch'], 'refs/heads/IntegrationFeatures')` |
| `development` | Stage | `eq(variables['Build.SourceBranch'], 'refs/heads/development')` |
| `hotfix/*` | Stage | `startsWith(variables['Build.SourceBranch'], 'refs/heads/hotfix/')` |
| `master` | Prod | `eq(variables['Build.SourceBranch'], 'refs/heads/master')` |

> **Note:** The `NH-CommonBackendApi` pipeline also triggers on a specific feature branch `feature/20250820_211729_reports_devicelimits`.

---

## Deployment Environments

### Common Backend API (`ABB.EL.Common.Api`)

| Environment | Azure Subscription | App Service |
|---|---|---|
| IntegrationTest | `ELSP-ELCommonPlatform-Dev NH` | `electrificationapi-aps-test-01` |
| Stage | `ELSP-ELCommonPlatform-Dev NH` | `electrificationapi-aps-stage-01` |
| Prod | `EL-CommonPlatformBackend-Prd NH` | `electrificationapi-aps-prod-01` |

### Notification API (`ABB.EL.Common.Notification.Api`)

| Environment | Azure Subscription | App Service |
|---|---|---|
| IntegrationTest | `ELSP-ELCommonPlatform-Dev NH` | `elnotificationapi-aps-test-01` |
| Stage | `ELSP-ELCommonPlatform-Dev NH` | `elnotificationapi-aps-stage-01` |
| Prod | `EL-CommonPlatformBackend-Prd NH` | `elnotificationapi-aps-prod-01` |

**Key observations:**

- IntegrationTest and Stage share the **Dev subscription** (`ELSP-ELCommonPlatform-Dev NH`).
- Prod uses a **dedicated production subscription** (`EL-CommonPlatformBackend-Prd NH`).
- Environment resource names follow the pattern `{PipelineName} {EnvironmentName}` for Azure DevOps environment approvals.

---

## Build Configuration

| Setting | Value |
|---|---|
| .NET SDK | `8.0.*` (latestMinor rollForward) |
| Target Framework | `net8.0` |
| C# Language Version | 12 |
| Build Pool | `Azure Pipelines` / `windows-latest` |
| Package Lock Files | Enabled (`RestorePackagesWithLockFile`) |
| NuGet Feeds | `nuget.org`, `ABB.Ability`, `EkipFeed`, `NHFeed` |

The `Directory.Build.props` injects Azure Pipelines metadata (build number, source branch, repository URI) as assembly-level attributes for traceability.

---

## Quality Gates

| Gate | Tool | Default State |
|---|---|---|
| Unit Tests | MSTest + coverlet | **Enabled** (non-blocking by default) |
| Static Analysis | SonarQube | **Enabled** |
| Dependency Scan | Bolt | **Enabled** |
| OSS Compliance | BlackDuck | **Disabled** (Common API only) |

---

## Pipeline Parameters

Both pipelines expose runtime parameters to toggle quality gates:

| Parameter | Type | Default | Description |
|---|---|---|---|
| `skipTests` | boolean | `false` | Skip test execution entirely |
| `areTestsBlocking` | boolean | `false` | Fail the build on test failures |
| `skipSonarQube` | boolean | `false` | Skip SonarQube analysis |
| `skipBolt` | boolean | `false` | Skip Bolt dependency scanning |
| `skipSecurity` | boolean | `true`* | Skip BlackDuck scanning |
| `blackDuckProjectVersion` | string | `'-'` | BlackDuck version override |

*`skipSecurity` is only present in the Common Backend API pipeline.

---

## Path Filtering

Each pipeline excludes paths owned by the other pipeline to avoid unnecessary builds:

| Pipeline | Excluded Paths |
|---|---|
| NH-CommonBackendApi | `ABB.EL.Common.Api.DataExport.IntegrationTests/**`, `ABB.EL.Common.Notification.Api/**`, `ABB.EL.Common.Services.Test/**`, `ABB.EL.Common.SubscriptionSynchronizer/**` |
| NH-NotificationApi | `ABB.EL.Common.Api.DataExport.IntegrationTests/**`, `ABB.EL.Common.Api/**`, `ABB.EL.Common.Services.Test/**`, `ABB.EL.Common.SubscriptionSynchronizer/**` |

---

## Scheduled Builds

Both pipelines run a **daily scheduled build** on the `IntegrationFeatures` branch:

```yaml
schedules:
  - cron: '30 15 * * *'
    displayName: Test - At 15:30 UTC, everyday
    branches:
      include:
        - IntegrationFeatures
```

---

## References

- **Shared template source:** `NH-Shared` repository → `devops/pipelines/templates/appService.yml` (branch: `main`)
- **NuGet configuration:** [NuGet.Config](../../NuGet.Config)
- **Build properties:** [Directory.Build.props](../../Directory.Build.props)
- **Build targets:** [Directory.Build.targets](../../Directory.Build.targets)
- **SDK pinning:** [global.json](../../global.json)

<!--
---
article_metadata:
  filename: "readme.md"
  last_updated: "2025-05-11"
---
-->