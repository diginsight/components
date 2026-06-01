# Azure DevOps Capabilities Summary

## Table of Contents

1. [How Azure DevOps Pipelines Work](#1-how-azure-devops-pipelines-work)
2. [Key Extensibility Capabilities](#2-key-extensibility-capabilities)
3. [How Security Works](#3-how-security-works)
4. [Most Important Limitations](#4-most-important-limitations)
5. [Appendix — NH-Shared Template Architecture](#5-appendix--nh-shared-template-architecture)

---

## 1. How Azure DevOps Pipelines Work

### Pipeline Lifecycle

An Azure DevOps YAML pipeline goes through three distinct phases:

| Phase | What Happens |
|-------|-------------|
| **1. Compile-time** | The YAML is parsed. Template expressions (`${{ }}`) are evaluated, templates are expanded, `${{ if }}` / `${{ each }}` blocks are resolved. The result is a fully-expanded pipeline definition. Service connections and environments are **validated at this stage**. |
| **2. Plan-time** | Stages, jobs, and their dependencies are organized into an execution plan. Conditions on stages and jobs are checked against the current context (branch, variables, etc.). |
| **3. Run-time** | Jobs are dispatched to agents. Runtime expressions (`$[ ]`) and `condition:` expressions are evaluated. Tasks execute sequentially within each job. Variables can be set dynamically via logging commands (`##vso[task.setvariable ...]`). |

> **Critical distinction**: `${{ }}` expressions are evaluated **before** the pipeline runs. `condition:` and `$[ ]` expressions are evaluated **during** the run. This difference is the root cause of many "the pipeline is not valid" errors — a service connection referenced inside a `condition:`-guarded step is still validated at compile time.

### Core Structure

```yaml
trigger:          # When the pipeline runs automatically
schedules:        # Cron-based scheduled runs
resources:        # External repos, pipelines, containers
variables:        # Pipeline-scoped variables and variable groups

stages:
  - stage: Build
    jobs:
      - job: BuildApp
        pool:
          vmImage: ubuntu-latest
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: build

  - stage: Deploy
    dependsOn: Build
    jobs:
      - deployment: DeployWeb      # Special job type for deployments
        environment: Production    # Links to an Environment for approvals
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebApp@1
```

### Key Concepts

| Concept | Description |
|---------|-------------|
| **Stage** | A logical boundary in the pipeline (e.g., Build, Deploy, Quality). Stages run sequentially by default but can run in parallel if `dependsOn` is configured. |
| **Job** | A unit of work that runs on a single agent. Jobs within a stage run in parallel by default. |
| **Deployment Job** | A special job type (`deployment:`) that tracks deployment history, links to environments, and supports strategies (runOnce, rolling, canary). |
| **Step** | A single task or script within a job. Steps always run sequentially. |
| **Task** | A pre-built unit of automation (e.g., `DotNetCoreCLI@2`, `AzureWebApp@1`). Tasks are versioned. |
| **Template** | A reusable YAML fragment that can define stages, jobs, steps, or variables. Templates accept parameters. |
| **Variable** | A name-value pair. Can be set at pipeline, stage, job, or step level. Can be marked `readonly`. |
| **Artifact** | A file or package produced by one job/stage and consumed by another (via `publish` / `download`). |
| **Resource** | An external dependency — another repository, pipeline, container, or package feed. |
| **Pool** | The agent pool where jobs run. Can be Microsoft-hosted (`vmImage: ubuntu-latest`) or self-hosted (`name: MyPool`). |
| **Demands** | Agent capabilities required by a job (e.g., `java`, `msbuild`). Only agents meeting all demands are selected. |

### Variable Scoping and Precedence

Variables are resolved in this order (later overrides earlier):

1. Variable groups (linked at pipeline level)
2. Pipeline-level `variables:`
3. Stage-level `variables:`
4. Job-level `variables:`
5. Task `env:` mappings
6. Runtime `##vso[task.setvariable]` logging commands

Variables set via logging commands are scoped to the current job by default. Use `isOutput=true` to expose them to downstream jobs or stages:

```yaml
# Setting an output variable
- pwsh: |
    echo "##vso[task.setvariable variable=MyVar;isOutput=true]myValue"
  name: SetVar

# Consuming in same stage, different job
- job: Consumer
  variables:
    myVar: $[dependencies.Producer.outputs['SetVar.MyVar']]

# Consuming in a different stage
- stage: NextStage
  variables:
    myVar: $[stageDependencies.PrevStage.Producer.outputs['SetVar.MyVar']]
```

---

## 2. Key Extensibility Capabilities

### 2.1 Templates

Templates are the primary reuse mechanism. They can encapsulate **stages, jobs, steps, or variables** and accept **typed parameters**.

```yaml
# Template definition (reusable)
parameters:
  - name: environment
    type: string
  - name: skipTests
    type: boolean
    default: false

steps:
  - task: DotNetCoreCLI@2
    inputs:
      command: build
  - ${{ if not(parameters.skipTests) }}:
    - task: DotNetCoreCLI@2
      inputs:
        command: test
```

```yaml
# Template consumption
steps:
  - template: steps/dotnet.build.yml
    parameters:
      environment: production
      skipTests: false
```

**Template types:**

| Type | Contains | Use Case |
|------|----------|----------|
| **Steps template** | A list of steps | Reusable build/test/deploy step sequences |
| **Jobs template** | A list of jobs | Reusable job definitions (e.g., build + publish) |
| **Stages template** | A list of stages | Reusable stage definitions (e.g., deploy + smoke test) |
| **Variables template** | A list of variables | Shared variable definitions |

### 2.2 `extends` Keyword

The `extends` keyword enforces that a pipeline **must** derive from an approved template. This is used for governance — an organization can require all pipelines to extend from a controlled template:

```yaml
# Consumer pipeline
resources:
  repositories:
    - repository: shared
      type: git
      name: NH-Shared
      ref: refs/heads/main

extends:
  template: devops/pipelines/templates/appService.yml@shared
  parameters:
    mainProjectName: MyApi
    deployedEnvironments:
      - internalName: Prod
        azureSubscription: MySubscription
```

### 2.3 Template Expressions (`${{ }}`)

Template expressions are evaluated at **compile time** and support:

| Expression | Example | Purpose |
|-----------|---------|---------|
| **Conditionals** | `${{ if eq(parameters.env, 'prod') }}:` | Include/exclude YAML blocks |
| **Iteration** | `${{ each item in parameters.list }}:` | Generate repeated blocks from a list |
| **Object spread** | `${{ insert }}: ${{ item }}` | Spread object properties into a mapping |
| **Functions** | `${{ or() }}`, `${{ and() }}`, `${{ not() }}`, `${{ eq() }}`, `${{ ne() }}`, `${{ coalesce() }}`, `${{ convertToJson() }}` | Logic and data transformation |
| **String interpolation** | `value: ${{ parameters.name }}-suffix` | Build strings from parameters |

### 2.4 Dynamic Stage Generation with `${{ each }}`

This is a powerful pattern for generating per-environment deployment stages from a single list:

```yaml
# Top-level template
parameters:
  - name: deployedEnvironments
    type: object

stages:
  - ${{ each env in parameters.deployedEnvironments }}:
    - template: stages/deploy.yml
      parameters:
        ${{ insert }}: ${{ env }}       # Spread all properties from the object
        isFunction: ${{ parameters.isFunction }}  # Add extra params
```

The `${{ insert }}` directive spreads the key-value pairs of the environment object directly into the parameters mapping, avoiding the need to explicitly list every property.

### 2.5 Cross-Repository Templates

Templates can be hosted in a separate repository and referenced via `resources.repositories`:

```yaml
resources:
  repositories:
    - repository: shared        # Alias
      type: git                 # Azure Repos Git
      name: NH-Shared           # Repo name in the same project
      ref: refs/heads/main      # Branch/tag

extends:
  template: devops/pipelines/templates/appService.yml@shared
```

This enables a **centralized template library** that multiple pipelines across different repos can consume.

### 2.6 Parameter Types

| Type | Description |
|------|-------------|
| `string` | A single string value |
| `boolean` | `true` / `false` |
| `number` | Numeric value |
| `object` | Any YAML structure (maps, lists, nested objects) |
| `step` | A single step definition |
| `stepList` | A list of step definitions |
| `job` | A single job definition |
| `jobList` | A list of job definitions |
| `stage` | A single stage definition |
| `stageList` | A list of stage definitions |

### 2.7 Runtime Expressions and Conditions

Runtime expressions (`$[ ]` and `condition:`) are evaluated during pipeline execution:

```yaml
# Runtime variable expression
variables:
  isMain: $[eq(variables['Build.SourceBranch'], 'refs/heads/main')]

# Step condition
- task: AzureWebApp@1
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
```

### 2.8 Artifacts and Data Flow Between Stages

```yaml
# Publish in Build stage
- task: PublishPipelineArtifact@1
  inputs:
    artifact: dist-dotnet
    targetPath: $(Build.ArtifactStagingDirectory)

# Download in Deploy stage
- download: current
  artifact: dist-dotnet
```

Artifacts are the **only** way to pass files between stages (since stages may run on different agents).

---

## 3. How Security Works

### 3.1 Service Connections

Service connections store credentials for external services (Azure subscriptions, SonarQube, Docker registries, etc.). They are:

- **Created** at the project or organization level in Azure DevOps
- **Authorized** per-pipeline (or for all pipelines)
- **Referenced by name** in YAML: `SonarQube: SonarQubeServiceConnection`
- **Validated at compile time** — if the connection doesn't exist or isn't authorized, the pipeline fails before any job runs

| Connection Type | Purpose | Example |
|----------------|---------|---------|
| Azure Resource Manager | Deploy to Azure subscriptions | App Service, AKS, Terraform |
| Kubernetes | Deploy to Kubernetes clusters | `KubernetesManifest@1` |
| Docker Registry | Push/pull container images | ACR login, build, push |
| SonarQube | Code quality analysis | `SonarQubePrepare@6` |
| Generic / Custom | Third-party tools | BlackDuck, JFrog |

### 3.2 Environments and Approvals

Environments provide deployment gates and tracking:

```yaml
- deployment: Deploy
  environment: Production          # Must exist in Azure DevOps
  strategy:
    runOnce:
      deploy:
        steps: ...
```

Environments can have:
- **Approval checks** — require manual approval before deployment
- **Branch filters** — only allow deployments from specific branches
- **Business hours** — restrict deployments to certain time windows
- **Exclusive locks** — prevent concurrent deployments
- **Template checks** — require the pipeline to extend from an approved template

### 3.3 Manual Validation

For human-in-the-loop approval within a pipeline:

```yaml
- job: ManualValidation
  pool: server                     # Runs on Azure DevOps, not an agent
  steps:
    - task: ManualValidation@0
      inputs:
        notifyUsers: approver@company.com
        instructions: 'Please validate the deployment'
      timeoutInMinutes: 1440       # 24 hours
```

### 3.4 Variable Groups and Secrets

Variable groups centralize secrets and configuration:

```yaml
variables:
  - group: NH-SECURITY-VG           # Links a variable group
  - name: PublicVar
    value: 'not-a-secret'
```

- **Secret variables** are masked in logs and cannot be passed across stages (must use artifacts or `isOutput`)
- **Azure Key Vault integration** — variable groups can source secrets from Key Vault

### 3.5 Token and Credential Handling

| Pattern | Mechanism |
|---------|-----------|
| **System access token** | `$(System.AccessToken)` — the pipeline's OAuth token for Azure DevOps APIs and feed access |
| **NuGet authentication** | `NuGetAuthenticate@1` task authenticates against Azure Artifacts feeds |
| **Docker build secrets** | `--build-arg ACCESS_TOKEN=$(System.AccessToken)` passed into Docker builds |
| **Environment variables** | Sensitive values mapped via `env:` blocks on steps |

### 3.6 Pipeline Security Controls

| Control | Description |
|---------|-------------|
| **`extends` templates** | Force all pipelines to derive from approved templates (can be enforced via org policy) |
| **Required template checks** | Environments can mandate that pipelines use specific templates |
| **Branch protection** | Service connections and environments can be restricted to specific branches |
| **Pipeline permissions** | Service connections must be explicitly authorized for each pipeline |
| **Readonly variables** | `readonly: true` prevents overriding in downstream scopes |
| **Audit logging** | All pipeline runs, approvals, and resource access are logged |

### 3.7 Security Scanning Integration

Azure DevOps pipelines commonly integrate security scanning tools:

| Tool Category | Examples | Purpose |
|--------------|---------|---------|
| **SAST** (Static Analysis) | SonarQube, Microsoft Security DevOps | Code quality and vulnerability detection |
| **SCA** (Software Composition) | BlackDuck (Synopsys Detect), Mend Bolt | Dependency vulnerability scanning |
| **Container Scanning** | MetaDefender, JFrog VDOO | Container image security analysis |
| **IaC Scanning** | tfsec, Microsoft Security DevOps | Infrastructure-as-Code compliance |

---

## 4. Most Important Limitations

### Execution & Platform Limits

#### 4.0.1 Parallel Jobs and Concurrency

| Aspect | Free Tier (Private) | Free Tier (Public) | Paid |
|--------|--------------------|--------------------|------|
| **Microsoft-hosted parallel jobs** | 1 | Up to 10 | Up to 25 per org (contact support for more) |
| **Self-hosted parallel jobs** | 1 | Unlimited | Unlimited (no charge per agent) |
| **Monthly time limit** | 1,800 min (30 hrs) | No limit | No limit |
| **Max job duration** | 60 min | 360 min (6 hrs) | 360 min (6 hrs) |

Key constraints:
- **Free tier must be requested** — new organizations don't receive it automatically; submit a request at https://aka.ms/azpipelines-parallelism-request (can take several business days)
- **Parallel jobs are org-wide** — you cannot partition them to specific projects or agent pools. Two runs in Project A will block Project B if all parallel slots are consumed
- **Each agent runs one job at a time** — to run N concurrent jobs, you need N agents (or N parallel job slots for Microsoft-hosted)
- **Rule of thumb**: ~1 parallel job per 4–5 developers
- **Server jobs** (e.g., `ManualValidation@0`) do **not** consume parallel jobs

#### 4.0.2 Job and Pipeline Timeouts

| Scope | Default | Maximum |
|-------|---------|---------|
| **Job timeout (Microsoft-hosted, private)** | 60 min | 60 min (paid: 360 min) |
| **Job timeout (Microsoft-hosted, public)** | 360 min | 360 min |
| **Job timeout (self-hosted)** | 60 min | Unlimited (`timeoutInMinutes: 0`) |
| **Server job timeout** | 60 min | 30 days |
| **Pipeline timeout** | 360 min | 360 min (Microsoft-hosted) |
| **Cancel timeout** (`cancelTimeoutInMinutes`) | 5 min | 35,790 min |

> Setting `timeoutInMinutes` higher than the hosted agent maximum has no effect on Microsoft-hosted agents — the built-in limit always wins.

#### 4.0.3 Microsoft-Hosted Agent Constraints

- **Ephemeral** — every job gets a fresh VM; no state persists between jobs
- **No GPU agents** available
- **10 GB disk space** for artifacts and build outputs (approximate)
- **Cannot RDP/SSH** into the agent for debugging
- **Available images**: `ubuntu-latest`, `windows-latest`, `macos-latest` (and specific versioned images)
- **Cold start latency**: typically 20–40 seconds for agent provisioning before the first step runs
- **Software updates**: Microsoft updates images on their schedule — you cannot pin a specific image patch version indefinitely

#### 4.0.4 Self-Hosted Agent Constraints

- **Unlimited agents** can be registered for free
- **No per-agent charge** — you pay only for parallel job slots
- **Agent pools are org-scoped** — pools can be shared across projects, but projects must be authorized
- **Agent updates** — agents auto-update by default; this can briefly take agents offline
- **Scale set agents** (VMSS) are available for auto-scaling, but require Azure infrastructure setup

#### 4.0.5 Retention and Storage

| Setting | Default | Maximum |
|---------|---------|---------|
| **Run retention (days)** | 30 days | Configurable per project |
| **Artifact retention** | Same as run retention | Configurable per project |
| **Recent runs to keep** | 3 per pipeline per branch | Configurable per project |
| **Release retention** | 30 days | Configurable (per-pipeline for classic) |
| **Permanently destroy releases** | 14 days after deletion | Configurable at project level |
| **Test results retention** | Configurable | Can be set to "Never delete" |

Key notes:
- **Per-pipeline retention is deprecated** for YAML pipelines — retention can only be configured at the project level
- **YAML multistage pipelines cannot vary retention by deployment environment** — unlike classic releases, you can't keep production deployments longer than dev deployments through retention settings
- Retention is processed once per day; you cannot control the schedule
- **Universal Packages, NuGet, npm** are NOT governed by pipeline retention — they have their own feed-level retention

#### 4.0.6 Rate Limits and API Throttling

- Azure DevOps enforces **rate limits on REST API calls** — excessive automation or nonoptimized queries can trigger throttling (HTTP 429)
- Throttled requests are delayed, not rejected, but sustained overuse can cause temporary blocks
- **Pipeline-triggered REST calls** (e.g., via scripts in steps) count toward rate limits

#### 4.0.7 Organization and Project Limits

| Resource | Limit |
|----------|-------|
| **Projects per organization** | 1,000 |
| **Pipelines per project** | Unlimited |
| **Self-hosted agents per org** | Unlimited |
| **Variable groups** | No hard limit |
| **Service connections** | No hard limit |
| **Environments per project** | No hard limit |

#### 4.0.8 Pricing (as of 2025)

| Item | Cost |
|------|------|
| **Microsoft-hosted parallel job** | ~$40/month per additional parallel job |
| **Self-hosted parallel job** | ~$15/month per additional parallel job |
| **Free tier (private, Microsoft-hosted)** | 1 parallel job, 1,800 min/month |
| **Free tier (self-hosted)** | 1 parallel job, unlimited minutes |
| **Azure Artifacts storage** | 2 GB free, then ~$2/GB/month |

> Prices are indicative — check [Azure DevOps pricing](https://azure.microsoft.com/pricing/details/devops/azure-devops-services/) for current rates.

---

### YAML Composition Limitations

### 4.1 Compile-Time Validation of Service Connections

**This is the most impactful limitation.**

Azure DevOps validates all service connection references at compile time, **even if the step that uses them is guarded by a runtime `condition:`**. If the service connection doesn't exist in the project, the pipeline fails with a validation error before any job starts.

```yaml
# THIS FAILS even if SkipSonarQube = true at runtime
- task: SonarQubePrepare@6
  condition: and(succeeded(), ne(variables['SkipSonarQube'], 'true'))  # Runtime!
  inputs:
    SonarQube: SonarQubeServiceConnection   # Validated at compile time!
```

**Workaround**: Use compile-time `${{ if }}` expressions to exclude the entire step:

```yaml
# THIS WORKS — the step is removed from the compiled pipeline
- ${{ if not(parameters.skipSonarQube) }}:
  - task: SonarQubePrepare@6
    inputs:
      SonarQube: SonarQubeServiceConnection
```

### 4.2 No Conditional Service Connection Names

You cannot use runtime variables or expressions to dynamically select a service connection. The connection name must be a **compile-time constant** (literal string or template expression).

```yaml
# DOES NOT WORK — runtime variables not supported for service connections
SonarQube: $(MyConnectionVariable)

# WORKS — template parameter resolved at compile time
SonarQube: ${{ parameters.connectionName }}
```

### 4.3 No Cross-Stage Variable Passing (Directly)

Variables set at runtime in one stage are **not directly accessible** in another stage. You must use one of:

- **Output variables** with the `stageDependencies` syntax
- **Pipeline artifacts** for file-based data
- **Variable groups** or **Key Vault** for shared configuration

```yaml
# Stage 1: Set output
- pwsh: echo "##vso[task.setvariable variable=Tag;isOutput=true]v1.0"
  name: SetTag

# Stage 2: Consume (verbose syntax)
variables:
  tag: $[stageDependencies.Build.BuildJob.outputs['SetTag.Tag']]
```

### 4.4 Template Expression Limitations

- **No string manipulation functions** — no `split()`, `replace()`, `substring()`, `toLower()` in `${{ }}`
- **No arithmetic** — no `${{ add(1, 2) }}` or similar
- **No access to runtime variables** — `${{ variables.Build.SourceBranch }}` doesn't work; `${{ }}` can only see parameters and compile-time-defined variables
- **Limited debugging** — no way to "print" template expression values during compilation

### 4.5 YAML Structure Constraints

- **No anchors or aliases** — YAML anchors (`&` / `*`) are not supported
- **No custom YAML tags** — only Azure DevOps-specific directives are recognized
- **`${{ insert }}` only works in mappings** — you cannot insert into sequences
- **Template nesting depth** — templates can reference other templates, but deep nesting increases compile time and makes debugging harder

### 4.6 Environment / Approval Limitations

- **Environments are project-scoped** — they cannot be shared across Azure DevOps projects
- **No programmatic approval** — approvals require human interaction through the UI or REST API
- **Environment checks are "all must pass"** — you cannot configure "any one of" approval logic natively

### 4.7 Agent and Pool Constraints

- **Microsoft-hosted agents are ephemeral** — no state persists between jobs. All dependencies must be installed fresh or restored from cache
- **No GPU agents** in Microsoft-hosted pools
- **Job timeout** defaults to 60 minutes (max 360 for Microsoft-hosted, unlimited for self-hosted)
- **Pipeline timeout** defaults to 360 minutes
- **Parallel job limits** — governed by your organization's purchased parallel job count

### 4.8 Trigger and Scheduling Limitations

- **Path filters are inclusive by default** — `exclude:` paths only work when `include:` is also specified (or defaults to all paths)
- **Scheduled triggers don't run on paused pipelines**
- **CI triggers on template repos don't cascade** — changing a template in a shared repo does not automatically trigger consumer pipelines (unless the consumer pipeline also monitors that repo)
- **No webhook-based triggers in YAML** — webhook triggers require classic pipeline definitions or service hooks with workarounds

### 4.9 Artifact and Data Limitations

- **Pipeline artifacts are immutable** — once published, they cannot be modified
- **No artifact streaming** — the entire artifact must be downloaded before use
- **Artifact size limits** — no hard limit, but very large artifacts slow down jobs significantly
- **Log output limit** — individual task logs truncate at ~16 MB; extremely verbose tasks may lose output

---

## 5. Appendix — NH-Shared Template Architecture

### Template Hierarchy

```
Consumer Pipeline (e.g., NH-CommonBackendApi.yml)
  │
  │  extends:
  ▼
Top-Level Template (appService.yml │ aks.yml │ aks-arm.yml │ appService_legacy.yml)
  │
  ├─► Build Stage
  │     └─ jobs/dotnet.build.yml
  │          └─ steps/dotnet.build.yml
  │               └─ steps/dotnet.restore.yml
  │     └─ jobs/angular.build.yml  (conditional)
  │          └─ steps/angular.build.yml
  │
  ├─► Deploy Stage ×N  (generated via ${{ each }})
  │     └─ stages/appService.deploy.yml  │  stages/aks.deploy.yml  │  stages/aks-arm.deploy.yml
  │
  ├─► Quality Stage
  │     └─ stages/quality.yml
  │          └─ SonarQube (compile-time guarded)
  │          └─ Tests + Code Coverage
  │          └─ Mend Bolt
  │
  └─► Security Stage
        └─ stages/security.yml
             ├─ jobs/security.scan.blackduck.yml
             ├─ jobs/security.scan.metadefender.yml
             └─ jobs/security.scan.jfrog.yml
```

### How Consumer Pipelines Invoke Templates

```yaml
# 1. Declare the shared repository
resources:
  repositories:
    - repository: shared
      type: git
      name: NH-Shared
      ref: refs/heads/main

# 2. Extend from a top-level template
extends:
  template: devops/pipelines/templates/appService.yml@shared
  parameters:
    # Build configuration
    mainProjectDirectory: MyApi
    mainProjectName: MyApi

    # Quality gates
    skipSonarQube: false
    sonarQubeProjectKey: My-Project
    sonarQubeProjectName: My Project
    skipBolt: false
    skipTests: false
    areTestsBlocking: true
    testProjectCsprojs: MyApi.Tests/MyApi.Tests.csproj

    # Security scanning
    skipSecurity: false
    blackDuckServiceName: BlackDuck Connection
    blackDuckProjectName: My-Project

    # Dynamic environments — each generates a deployment stage
    deployedEnvironments:
      - internalName: Dev
        displayName: Development
        envResourceName: MyApp Dev
        azureSubscription: My-Dev-Subscription
        webAppName: myapp-dev
        condition: eq(variables['Build.SourceBranch'], 'refs/heads/develop')

      - internalName: Prod
        displayName: Production
        envResourceName: MyApp Prod
        azureSubscription: My-Prod-Subscription
        webAppName: myapp-prod
        condition: eq(variables['Build.SourceBranch'], 'refs/heads/main')
```

### Key Design Patterns Used

| Pattern | Implementation |
|---------|---------------|
| **Dynamic stage generation** | `${{ each env in parameters.deployedEnvironments }}` + `${{ insert }}` creates N deployment stages from a parameter list |
| **Compile-time gating** | `${{ if not(parameters.skipSonarQube) }}` excludes steps that reference missing service connections |
| **Object spread** | `${{ insert }}: ${{ env }}` passes all properties of an environment object as parameters |
| **Fallback defaults** | `${{ coalesce(parameters.x, 'default') }}` provides defaults for optional parameters |
| **Dual-track support** | Modern (`dotnet.build.yml`) and legacy (`dotnet_legacy.build.yml`) paths for different .NET versions |
| **Cross-stage artifacts** | `PublishPipelineArtifact` in Build → `download: current` in Deploy |
| **Output variable chaining** | Build stage sets Docker tags → Deploy stage reads via `stageDependencies` |
| **Readonly variables** | All computed variables use `readonly: true` to prevent accidental override |
