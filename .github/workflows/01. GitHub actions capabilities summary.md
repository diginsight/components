# GitHub Actions Capabilities Summary

## Table of Contents

1. [How GitHub Actions Work](#1-how-github-actions-work)
2. [Key Extensibility Capabilities](#2-key-extensibility-capabilities)
3. [How Security Works](#3-how-security-works)
4. [Most Important Limitations](#4-most-important-limitations)
5. [Appendix — Diginsight Components Workflow Architecture](#5-appendix--diginsight-components-workflow-architecture)
6. [Appendix — Comparison: GitHub Actions vs Azure DevOps Pipelines](#6-appendix--comparison-github-actions-vs-azure-devops-pipelines)

---

## 1. How GitHub Actions Work

### Workflow Lifecycle

A GitHub Actions workflow goes through three distinct phases:

| Phase | What Happens |
|-------|-------------|
| **1. Trigger evaluation** | An event occurs (push, pull_request, schedule, workflow_dispatch, etc.). GitHub evaluates which workflows have matching `on:` triggers and filters (branches, paths, tags). Matching workflows are queued. |
| **2. Job planning** | Jobs within the workflow are organized by their `needs:` dependencies. Independent jobs are scheduled for parallel execution. Matrix strategies are expanded into individual job instances. |
| **3. Job execution** | Each job is dispatched to a runner (GitHub-hosted or self-hosted). Steps execute sequentially within a job. Expressions (`${{ }}`) are evaluated at runtime. Outputs, artifacts, and caches are persisted between steps/jobs. |

> **Critical distinction from Azure DevOps**: GitHub Actions has a **single expression syntax** (`${{ }}`) that is evaluated at workflow runtime. There is no separate "compile-time" vs "runtime" phase — all expressions are evaluated when the workflow runs. This eliminates the class of "compile-time validation" errors common in Azure DevOps, but means all references must be valid at execution time.

### Core Structure

```yaml
name: CI/CD Pipeline                # Workflow name

on:                                  # Trigger configuration
  push:
    branches: [main]
    paths: ['src/**']
  pull_request:
    branches: [main]
  workflow_dispatch:                 # Manual trigger
    inputs:
      environment:
        description: 'Target environment'
        required: true
        default: 'staging'
  schedule:
    - cron: '0 6 * * 1'             # Cron-based scheduling

permissions:                         # Workflow-level OIDC permissions
  contents: read
  id-token: write

concurrency:                         # Prevent duplicate runs
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

env:                                 # Workflow-level environment variables
  DOTNET_VERSION: '9.0.x'

jobs:
  build:
    runs-on: ubuntu-latest           # Runner selection
    outputs:
      version: ${{ steps.ver.outputs.version }}

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - run: dotnet build --configuration Release
      - id: ver
        run: echo "version=1.0.${{ github.run_number }}" >> $GITHUB_OUTPUT

  deploy:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://myapp.azurewebsites.net
    steps:
      - uses: azure/login@v2
        with:
          client-id: ${{ vars.AZURE_CLIENT_ID }}
          tenant-id: ${{ vars.AZURE_TENANT_ID }}
          subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}
      - uses: azure/webapps-deploy@v3
        with:
          app-name: my-app
```

### Key Concepts

| Concept | Description |
|---------|-------------|
| **Workflow** | A YAML file in `.github/workflows/` that defines an automated process. Equivalent to a full pipeline definition. |
| **Event / Trigger** | The condition that starts a workflow (push, pull_request, schedule, workflow_dispatch, repository_dispatch, etc.). |
| **Job** | A unit of work that runs on a single runner. Jobs run in parallel by default; use `needs:` for sequential ordering. |
| **Step** | A single task within a job — either a shell command (`run:`) or an action (`uses:`). Steps always run sequentially. |
| **Action** | A reusable unit of automation — can be a JavaScript action, Docker container action, or composite action. Referenced via `uses:`. |
| **Runner** | The machine that executes a job. Can be GitHub-hosted (`ubuntu-latest`, `windows-latest`, `macos-latest`) or self-hosted. |
| **Environment** | A named deployment target (e.g., `production`, `staging`) with optional protection rules, secrets, and approval gates. |
| **Artifact** | A file or directory produced by one job and consumed by another via `actions/upload-artifact` / `actions/download-artifact`. |
| **Cache** | Persisted dependency data across workflow runs via `actions/cache` — speeds up builds by avoiding repeated downloads. |
| **Secret** | An encrypted variable stored at repository, environment, or organization level. Accessed via `${{ secrets.NAME }}`. |
| **Variable** | A non-encrypted configuration value stored at repository, environment, or organization level. Accessed via `${{ vars.NAME }}`. |
| **Matrix** | A strategy that generates multiple job instances from a set of variable combinations (also available in Azure DevOps). |
| **Concurrency** | Controls whether workflows or jobs can run simultaneously, with optional cancellation of in-progress runs. |
| **Reusable Workflow** | A workflow designed to be called by other workflows via `workflow_call` trigger, enabling cross-workflow reuse. |

### Expression Syntax

GitHub Actions uses a single expression syntax `${{ }}` evaluated at runtime:

```yaml
# Accessing contexts
${{ github.ref }}                     # Git ref that triggered the workflow
${{ github.event_name }}              # Event type (push, pull_request, etc.)
${{ secrets.API_KEY }}                # Repository or environment secret
${{ vars.VERSION_PREFIX }}            # Repository or environment variable
${{ needs.build.outputs.version }}    # Output from a dependent job
${{ steps.my_step.outputs.result }}   # Output from a previous step
${{ matrix.os }}                      # Current matrix variable value
${{ runner.os }}                      # Runner operating system
${{ env.MY_VAR }}                     # Environment variable
${{ inputs.environment }}             # Workflow dispatch input value
```

### Variable Scoping and Precedence

Variables are resolved in this order (later overrides earlier):

1. Organization-level variables (`vars.*`)
2. Repository-level variables (`vars.*`)
3. Environment-level variables (`vars.*`) — if an environment is specified
4. Workflow-level `env:`
5. Job-level `env:`
6. Step-level `env:`
7. Runtime `$GITHUB_ENV` file writes
8. Step outputs via `$GITHUB_OUTPUT`

```yaml
# Setting an output variable in a step
- id: set_version
  run: echo "version=1.0.42" >> $GITHUB_OUTPUT

# Consuming in the same job, later step
- run: echo "Version is ${{ steps.set_version.outputs.version }}"

# Consuming in a different job (requires job-level outputs declaration)
jobs:
  build:
    outputs:
      version: ${{ steps.set_version.outputs.version }}
  deploy:
    needs: build
    steps:
      - run: echo "Deploying version ${{ needs.build.outputs.version }}"
```

### Context Objects

GitHub Actions provides rich context objects:

| Context | Contents |
|---------|----------|
| `github` | Event payload, repository, ref, SHA, actor, workflow name, run number, API URL |
| `env` | Environment variables set at workflow, job, or step scope |
| `vars` | Configuration variables from repository, environment, or organization settings |
| `secrets` | Encrypted secrets from repository, environment, or organization settings |
| `job` | Current job status, container info, services |
| `steps` | Outputs and status of completed steps in the current job |
| `runner` | Runner metadata (OS, architecture, temp directory, tool cache path) |
| `needs` | Outputs and results of all jobs that the current job depends on |
| `strategy` | Matrix information for the current job |
| `matrix` | Current matrix variable values for the current job |
| `inputs` | Workflow dispatch or reusable workflow input values |

---

## 2. Key Extensibility Capabilities

### 2.1 Reusable Workflows (`workflow_call`)

Reusable workflows are the primary composition mechanism — equivalent to Azure DevOps templates. A workflow can declare itself as callable and define typed inputs, outputs, and secrets:

```yaml
# Reusable workflow definition (e.g., .github/workflows/deploy.yml)
name: Deploy to Azure

on:
  workflow_call:
    inputs:
      environment:
        required: true
        type: string
      app-name:
        required: true
        type: string
    secrets:
      AZURE_CLIENT_SECRET:
        required: false
    outputs:
      deployed-url:
        description: 'URL of the deployed app'
        value: ${{ jobs.deploy.outputs.url }}

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: ${{ inputs.environment }}
    outputs:
      url: ${{ steps.deploy.outputs.webapp-url }}
    steps:
      - uses: azure/webapps-deploy@v3
        id: deploy
        with:
          app-name: ${{ inputs.app-name }}
```

```yaml
# Caller workflow
jobs:
  deploy-staging:
    uses: ./.github/workflows/deploy.yml
    with:
      environment: staging
      app-name: my-app-staging
    secrets: inherit                    # Pass all secrets from caller
```

**Key differences from Azure DevOps templates:**
- Reusable workflows operate at the **job level** — each reusable workflow call produces one or more complete jobs
- You cannot inject individual steps or stages via reusable workflows
- Inputs are typed (`string`, `boolean`, `number`) — no `object`, `stepList`, or `stageList` types
- `secrets: inherit` passes all caller secrets without explicit listing

### 2.2 Composite Actions

Composite actions package a sequence of steps into a reusable unit — the closest equivalent to Azure DevOps step templates:

```yaml
# action.yml in a repository or local directory
name: 'Build .NET Project'
description: 'Restore, build, and test a .NET solution'

inputs:
  solution:
    description: 'Path to solution file'
    required: true
  configuration:
    description: 'Build configuration'
    default: 'Release'

outputs:
  artifact-path:
    description: 'Path to build output'
    value: ${{ steps.build.outputs.path }}

runs:
  using: 'composite'
  steps:
    - run: dotnet restore ${{ inputs.solution }}
      shell: bash
    - run: dotnet build ${{ inputs.solution }} -c ${{ inputs.configuration }}
      shell: bash
      id: build
```

```yaml
# Consumer workflow
steps:
  - uses: ./.github/actions/build-dotnet    # Local composite action
    with:
      solution: src/MyApp.sln
  - uses: my-org/shared-actions/build@v2    # Cross-repo composite action
    with:
      solution: src/MyApp.sln
```

### 2.3 Matrix Strategy

Matrices generate multiple parallel job instances from variable combinations:

```yaml
jobs:
  test:
    strategy:
      fail-fast: false              # Don't cancel other jobs if one fails
      max-parallel: 4               # Limit concurrent jobs
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        dotnet: ['8.0.x', '9.0.x']
        include:                    # Add specific combinations
          - os: ubuntu-latest
            dotnet: '9.0.x'
            experimental: true
        exclude:                    # Remove specific combinations
          - os: macos-latest
            dotnet: '8.0.x'
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet }}
      - run: dotnet test
```

### 2.4 Dynamic Matrix Generation

Matrices can be computed dynamically using job outputs:

```yaml
jobs:
  prepare:
    runs-on: ubuntu-latest
    outputs:
      matrix: ${{ steps.set-matrix.outputs.matrix }}
    steps:
      - id: set-matrix
        run: |
          echo 'matrix={"environment":["dev","staging","prod"],"include":[{"environment":"prod","approval":true}]}' >> $GITHUB_OUTPUT

  deploy:
    needs: prepare
    strategy:
      matrix: ${{ fromJSON(needs.prepare.outputs.matrix) }}
    runs-on: ubuntu-latest
    steps:
      - run: echo "Deploying to ${{ matrix.environment }}"
```

### 2.5 Workflow Dispatch Inputs

Manual triggers with typed, validated inputs:

```yaml
on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target environment'
        required: true
        type: choice
        options:
          - development
          - staging
          - production
      dry-run:
        description: 'Dry run mode'
        type: boolean
        default: false
      version:
        description: 'Version to deploy'
        type: string
        required: false
```

### 2.6 Expressions and Functions

GitHub Actions provides built-in functions for use in `${{ }}` expressions:

| Category | Functions |
|----------|----------|
| **Comparison** | `==`, `!=`, `<`, `>`, `<=`, `>=` |
| **Logical** | `&&`, `\|\|`, `!` |
| **Status checks** | `success()`, `failure()`, `always()`, `cancelled()` |
| **String** | `contains()`, `startsWith()`, `endsWith()`, `format()` |
| **JSON** | `toJSON()`, `fromJSON()` |
| **Hash** | `hashFiles()` |
| **Conditionals** | `if` on steps, jobs |

```yaml
# Step condition
- run: echo "Deploying to prod"
  if: github.ref == 'refs/heads/main' && success()

# Job condition
deploy:
  if: github.event_name == 'push' && contains(github.ref, 'refs/tags/v')
  needs: build

# Using fromJSON for dynamic values
- run: echo "${{ fromJSON(steps.data.outputs.result).name }}"
```

### 2.7 Job and Workflow Dependencies

```yaml
jobs:
  lint:
    runs-on: ubuntu-latest
    steps: [...]

  test:
    runs-on: ubuntu-latest
    steps: [...]

  build:
    needs: [lint, test]              # Runs after BOTH lint and test succeed
    runs-on: ubuntu-latest
    steps: [...]

  deploy:
    needs: build
    if: always() && needs.build.result == 'success'
    runs-on: ubuntu-latest
    steps: [...]
```

### 2.8 Artifacts and Data Flow Between Jobs

```yaml
# Upload in build job
- uses: actions/upload-artifact@v4
  with:
    name: build-output
    path: ./publish/
    retention-days: 5
    if-no-files-found: error

# Download in deploy job
- uses: actions/download-artifact@v4
  with:
    name: build-output
    path: ./deploy-package/
```

Artifacts are the primary mechanism for passing files between jobs (since jobs may run on different runners).

### 2.9 Caching

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

Caches persist across workflow runs within the same branch (with fallback to the default branch). This is fundamentally different from artifacts, which are scoped to a single workflow run.

### 2.10 Service Containers

Jobs can spin up sidecar containers for integration testing:

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: testpass
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      redis:
        image: redis:7
        ports:
          - 6379:6379
    steps:
      - run: dotnet test  # Tests can connect to localhost:5432 and localhost:6379
```

### 2.11 Cross-Repository Workflow Calls

Reusable workflows can be hosted in a separate repository:

```yaml
jobs:
  deploy:
    uses: my-org/shared-workflows/.github/workflows/deploy.yml@main
    with:
      environment: production
    secrets: inherit
```

This enables a **centralized workflow library** similar to Azure DevOps shared templates.

### 2.12 Repository Dispatch (Event-Driven)

Trigger workflows programmatically via the GitHub API:

```yaml
on:
  repository_dispatch:
    types: [deploy-request]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - run: echo "Deploying version ${{ github.event.client_payload.version }}"
```

Triggered via API call:
```bash
curl -X POST -H "Authorization: token $TOKEN" \
  -d '{"event_type":"deploy-request","client_payload":{"version":"1.2.3"}}' \
  https://api.github.com/repos/OWNER/REPO/dispatches
```

---

## 3. How Security Works

### 3.1 Secrets Management

Secrets are encrypted values stored at three levels:

| Level | Scope | Access |
|-------|-------|--------|
| **Organization secrets** | All repos (or selected repos) in the org | `${{ secrets.NAME }}` |
| **Repository secrets** | Single repository | `${{ secrets.NAME }}` |
| **Environment secrets** | Single environment within a repo | `${{ secrets.NAME }}` (only when job uses that environment) |

Key behaviors:
- Secrets are **masked in logs** — GitHub automatically redacts any log output matching a secret value
- Secrets are **not passed to workflows triggered by forks** (for pull_request events from forks)
- Secrets are **not accessible in template expressions** in reusable workflows — must be passed explicitly or via `secrets: inherit`
- **GITHUB_TOKEN** is automatically generated per workflow run with configurable permissions

### 3.2 Configuration Variables

Non-sensitive configuration values stored at organization, repository, or environment level:

```yaml
steps:
  - run: echo "Deploying version prefix ${{ vars.VERSION_PREFIX }}"
  - run: echo "Client ID: ${{ vars.AZURE_CLIENT_ID }}"
```

Variables are **not masked in logs** — use secrets for sensitive values.

### 3.3 GITHUB_TOKEN and Permissions

Every workflow run receives an automatic `GITHUB_TOKEN` with configurable permissions:

```yaml
# Workflow-level permissions (restrictive)
permissions:
  contents: read
  pull-requests: write
  id-token: write           # Required for OIDC (e.g., Azure federated credentials)

# Job-level permissions (override workflow-level)
jobs:
  deploy:
    permissions:
      id-token: write
      contents: read
```

**Available permission scopes**: `actions`, `checks`, `contents`, `deployments`, `id-token`, `issues`, `packages`, `pages`, `pull-requests`, `repository-projects`, `security-events`, `statuses`.

**Default behavior**: If `permissions` is not specified, `GITHUB_TOKEN` gets the default permissions configured in repository settings (typically `read` for all, or `write` for all).

### 3.4 Environments and Protection Rules

Environments provide deployment gates and approval workflows:

```yaml
jobs:
  deploy:
    environment:
      name: production
      url: https://myapp.example.com
```

Environment protection rules include:
- **Required reviewers** — one or more users/teams must approve before the job runs
- **Wait timer** — delay execution by a specified number of minutes
- **Branch/tag restrictions** — only specific branches or tags can deploy to the environment
- **Custom deployment protection rules** — third-party integrations (e.g., ServiceNow, Datadog) can gate deployments
- **Deployment branches** — restrict which branches can deploy to the environment
- **Prevent self-review** — prevent the person who triggered the workflow from approving their own deployment

### 3.5 OpenID Connect (OIDC) — Federated Credentials

GitHub Actions supports OIDC for **secretless authentication** to cloud providers:

```yaml
permissions:
  id-token: write            # Required for OIDC

steps:
  - uses: azure/login@v2
    with:
      client-id: ${{ vars.AZURE_CLIENT_ID }}
      tenant-id: ${{ vars.AZURE_TENANT_ID }}
      subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}
      # No client secret needed — uses OIDC federated credential
```

This eliminates the need to store cloud provider credentials as secrets. The cloud provider trusts GitHub's OIDC token issuer and maps it to a specific identity based on claims (repository, branch, environment).

### 3.6 Fork Security Model

| Trigger | Secret Access | GITHUB_TOKEN Permissions |
|---------|--------------|-------------------------|
| `pull_request` from same repo | ✅ Full access | ✅ Full configured permissions |
| `pull_request` from fork | ❌ No secrets | 🔒 Read-only |
| `pull_request_target` from fork | ✅ Full access (target repo secrets) | ✅ Full configured permissions |

> **Warning**: `pull_request_target` runs in the context of the **base** (target) repository, not the fork. It has access to all secrets. Use with extreme caution — never checkout and run untrusted code from the fork's PR branch within a `pull_request_target` workflow.

### 3.7 Concurrency Controls

Prevent duplicate or conflicting runs:

```yaml
concurrency:
  group: deploy-${{ github.ref }}
  cancel-in-progress: true          # Cancel older runs in the same group
```

### 3.8 Dependency Review and Supply Chain Security

| Feature | Description |
|---------|-------------|
| **Dependabot** | Automated dependency update PRs with security vulnerability detection |
| **Dependency review action** | `actions/dependency-review-action` blocks PRs that introduce vulnerable dependencies |
| **Code scanning** | `github/codeql-action` for SAST analysis integrated into PR checks |
| **Secret scanning** | Automatic detection of leaked secrets in commits (push protection) |
| **Software Bill of Materials** | SBOM generation via `actions/dependency-submission` |
| **Artifact attestations** | `actions/attest-build-provenance` for SLSA provenance attestation |

### 3.9 Branch Protection and Rulesets

Branch protection rules complement Actions security:
- **Required status checks** — specific workflow jobs must pass before merge
- **Required reviews** — PRs need approval before merge
- **Signed commits** — enforce GPG/SSH commit signatures
- **Merge queue** — automated merge with required checks
- **Branch rulesets** — organization-wide rules that apply to multiple repositories

### 3.10 Private Action Access Controls

- Actions from **public repos** are available by default
- Actions from **internal repos** require explicit opt-in via repo settings
- Actions from **private repos** in the same org require `actions: read` permission on the action's repo
- Organizations can restrict which actions are allowed (all, GitHub-authored only, verified creators, or an explicit allowlist)

---

## 4. Most Important Limitations

### Execution & Platform Limits

#### 4.0.1 Concurrency and Billing

| Aspect | Free (Public) | Free (Private) | Team (Private) | Enterprise |
|--------|--------------|----------------|----------------|------------|
| **Included minutes/month** | Unlimited | 2,000 | 3,000 | 50,000 |
| **Max concurrent jobs** | 20 | 20 | 60 | 500 (org-wide) |
| **Max concurrent macOS jobs** | 5 | 5 | 5 | varies |
| **Storage (artifacts + caches)** | 500 MB | 500 MB | 2 GB | 50 GB |

**Minute multipliers** (for private repos):
| Runner OS | Multiplier |
|-----------|-----------|
| Linux | 1× |
| Windows | 2× |
| macOS | 10× |

Key constraints:
- **Concurrency limits are org-wide** — all repos share the same concurrent job pool
- **Self-hosted runners** are free and unlimited — no minute charges or concurrency limits beyond what your infrastructure supports
- **Spending limits** can be set to prevent unexpected charges from overage minutes
- **Jobs queued beyond concurrency limits** are queued (not rejected) and will run when a slot becomes available

#### 4.0.2 Job and Workflow Timeouts

| Scope | Default | Maximum |
|-------|---------|---------|
| **Job timeout (GitHub-hosted)** | 360 min (6 hrs) | 360 min |
| **Job timeout (self-hosted)** | 360 min (6 hrs) | 35 days |
| **Workflow run duration** | None | 35 days |
| **Workflow run queue time** | N/A | 24 hours (then cancelled) |
| **API request wait** | N/A | 10 min per individual request |
| **Job matrix (max generated)** | N/A | 256 per workflow |

```yaml
jobs:
  long-running:
    timeout-minutes: 120            # Override default timeout
```

#### 4.0.3 GitHub-Hosted Runner Constraints

- **Ephemeral** — every job gets a fresh VM; no state persists between jobs
- **Available images**: `ubuntu-latest` (22.04), `ubuntu-24.04`, `windows-latest` (2022), `windows-2025`, `macos-latest` (14), `macos-15`, and ARM64 variants
- **Larger runners** available (Team/Enterprise): 4–64 vCPU Linux/Windows, GPU runners (limited), ARM64 runners
- **14 GB RAM / 14 GB SSD** for standard runners (2 vCPU)
- **Cannot SSH/RDP** into runners for debugging (but `tmate` action exists as a workaround)
- **Pre-installed software**: each image comes with a large set of pre-installed tools (documented in [runner-images](https://github.com/actions/runner-images))
- **Cold start latency**: typically 15–45 seconds for runner provisioning

#### 4.0.4 Self-Hosted Runner Constraints

- **Free** — no per-minute or per-runner charges
- **No concurrency limits** beyond your own infrastructure
- **Runner groups** for organization-level management (Enterprise)
- **Runner labels** for targeting specific runner capabilities
- **Auto-scaling** via Actions Runner Controller (ARC) for Kubernetes-based scaling
- **Ephemeral runners** (`--ephemeral` flag) — runner de-registers after one job, ensuring clean state
- **Security risk**: self-hosted runners persist state unless ephemeral mode is used — public repos should **never** use non-ephemeral self-hosted runners (risk of malicious PR code executing on your infrastructure)
- **Runner application auto-updates** — runners auto-update within 30 days of a new version release

#### 4.0.5 Retention and Storage

| Setting | Default | Maximum |
|---------|---------|---------|
| **Artifact retention** | 90 days | 400 days (configurable per-repo or per-upload) |
| **Cache retention** | 7 days (unused) | Evicted LRU when > 10 GB per repo |
| **Log retention** | Same as artifact retention | 400 days |
| **Workflow run retention** | 90 days | 400 days |

Key notes:
- **Artifacts count toward storage billing** (500 MB free for private repos)
- **Caches are branch-scoped** — a cache created on a feature branch can be restored on the default branch, but not vice versa (except for fallback to the default branch via `restore-keys`)
- **Caches are repository-scoped** — caches cannot be shared across repositories
- **Old caches are evicted** when total cache size exceeds 10 GB per repository (LRU eviction)

#### 4.0.6 Rate Limits and API Throttling

| Limit | Value |
|-------|-------|
| **Workflow runs per hour per repo** | 500 (across all events) |
| **API requests per hour (GITHUB_TOKEN)** | 1,000 per repo |
| **API requests per hour (PAT)** | 5,000 per user |
| **Concurrent workflow runs per repo** | 500 queued, based on plan concurrency |
| **Job matrix per workflow** | 256 jobs max |
| **Workflow file size** | 512 KB max |
| **Reusable workflow nesting depth** | 4 levels |
| **Env variables per workflow** | 100 |

#### 4.0.7 Pricing (as of 2025)

| Item | Cost |
|------|------|
| **Free tier (public repos)** | Unlimited minutes |
| **Free tier (private repos)** | 2,000 minutes/month (Linux) |
| **Linux minute overage** | $0.008/min |
| **Windows minute overage** | $0.016/min |
| **macOS minute overage** | $0.08/min |
| **Larger runners** | Varies by vCPU count; 4-vCPU Linux ≈ $0.032/min |
| **Self-hosted runners** | Free (your infrastructure costs) |
| **Storage overage** | $0.25/GB/month |

> Prices are indicative — check [GitHub Actions pricing](https://docs.github.com/billing/managing-billing-for-github-actions/about-billing-for-github-actions) for current rates.

---

### Workflow Composition Limitations

### 4.1 Reusable Workflow Constraints

**Reusable workflows operate at the job level only.** Unlike Azure DevOps templates that can inject steps, jobs, or stages, reusable workflows always produce complete jobs. You cannot:

- Call a reusable workflow as a **step within a job**
- Use a reusable workflow to inject **individual steps** into a calling job
- Nest reusable workflows more than **4 levels deep**
- Have a single reusable workflow produce **multiple stages** (GitHub Actions has no stage concept)

```yaml
# DOES NOT WORK — reusable workflows cannot be steps
jobs:
  build:
    steps:
      - uses: ./.github/workflows/reusable.yml  # ❌ Not allowed
```

### 4.2 No Stage / Phase Concept

GitHub Actions has **no built-in stage concept**. All organization is at the job level. To simulate stages, you use job dependencies:

```yaml
# Simulating stages via job dependencies
jobs:
  # "Build Stage"
  build:
    runs-on: ubuntu-latest
    steps: [...]

  # "Test Stage"
  unit-tests:
    needs: build
    runs-on: ubuntu-latest
    steps: [...]

  integration-tests:
    needs: build
    runs-on: ubuntu-latest
    steps: [...]

  # "Deploy Stage"
  deploy:
    needs: [unit-tests, integration-tests]
    runs-on: ubuntu-latest
    environment: production
    steps: [...]
```

This means you cannot visualize or manage "stages" as a first-class concept in the GitHub UI — the dependency graph is flat.

### 4.3 Limited Input Types for Reusable Workflows

Reusable workflow inputs support only: `string`, `boolean`, `number`. There is **no `object` type** — you cannot pass structured data like lists of environments. Workaround: serialize as JSON string and deserialize with `fromJSON()`.

```yaml
# Workaround for passing structured data
on:
  workflow_call:
    inputs:
      environments:
        type: string             # JSON-encoded array
        required: true

jobs:
  deploy:
    strategy:
      matrix:
        env: ${{ fromJSON(inputs.environments) }}
```

### 4.4 No Dynamic Job Generation from Parameters

Unlike Azure DevOps `${{ each }}` which can generate N stages at compile time from a parameter list, GitHub Actions cannot dynamically create jobs based on inputs. The closest equivalent is matrix strategy (which requires a fixed set of values or a JSON-encoded dynamic matrix).

### 4.5 Expression Limitations

- **No regular expressions** in expressions (use `contains()`, `startsWith()`, `endsWith()` instead)
- **Limited string manipulation** — no `split()`, `replace()`, `substring()`, `toLower()` natively
- **No arithmetic** — no addition, subtraction, etc. in expressions (use shell scripts)
- **Expression evaluation is permissive** — invalid property access returns empty string rather than an error, making typos hard to detect
- **No `coalesce()` equivalent** — use chained `||` operator: `${{ inputs.version || 'latest' }}`

### 4.6 Concurrency and Cancellation

- **Concurrency groups are string-based** — typos in group names silently create separate groups
- **`cancel-in-progress: true`** cancels the older run, which may be undesirable for deployment workflows
- **No built-in priority system** — all queued jobs are equal priority
- **No exclusive locks on environments** beyond the built-in "one deployment at a time" protection rule

### 4.7 Cross-Job Communication Constraints

- **No direct variable sharing between jobs** — must use `outputs` + `needs` or artifacts
- **Job outputs are strings only** — complex data must be JSON-serialized
- **Artifact upload/download adds overhead** — each artifact operation takes 10–30+ seconds
- **No artifact streaming** — entire artifact must be downloaded before use
- **Environment variables set via `$GITHUB_ENV`** are scoped to the current job only

### 4.8 Trigger and Event Limitations

- **`paths` filters are not supported with `schedule` or `workflow_dispatch`** — path filters only work with `push` and `pull_request`
- **No cross-repository triggers** for private repos without `repository_dispatch` or workflow_run
- **`workflow_run` is limited** — can only be triggered by workflows in the **same repository**, and only on the default branch
- **Scheduled workflows run on the default branch only** — cron triggers ignore non-default branches
- **Cron schedule minimum interval** is 5 minutes (but actual execution may be delayed by up to 15 minutes during high load)
- **Scheduled workflows are disabled** after 60 days of repository inactivity

### 4.9 Environment and Deployment Constraints

- **Environments are repository-scoped** — cannot be shared across repositories
- **Deployment approvals are per-environment** — no way to require approval for a specific job without an environment
- **No programmatic approval API** — approvals require manual interaction via the GitHub UI or mobile app
- **Deployment history** is tracked per-environment, but limited querying capability

### 4.10 Workflow File Constraints

| Constraint | Limit |
|-----------|-------|
| **Max workflow file size** | 512 KB |
| **Max workflows per repository** | Unlimited (but 500 runs/hour limit) |
| **Max jobs per workflow** | 500 |
| **Max steps per job** | 1,000 |
| **Reusable workflow nesting depth** | 4 levels |
| **Matrix combinations per workflow** | 256 |
| **Env variables per step** | 100 |
| **Total env size** | 256 KB |

---

## 5. Appendix — Diginsight Components Workflow Architecture

### Workflow Inventory

| Workflow File | Purpose | Trigger | Runner |
|---------------|---------|---------|--------|
| `v3.yml` | Build and publish NuGet packages for v3+ releases | Tag push (`v3*`, `v4*`, …) | `ubuntu-latest` |
| `v2_99.Package.CICD.yml` | Main CI/CD pipeline: build, package, publish NuGet | Push/PR to `main` (paths: `src/Diginsight.Components*/**`), manual dispatch | `self-hosted` |
| `v2_00.InstallActions.yml` | Reusable: checkout, LFS, .NET SDK, NuGet setup | `workflow_call` | `self-hosted` |
| `v2_01.GetCompositeVariables.yml` | Reusable: compute assembly version from run number + offset | `workflow_call` (outputs: `assemblyVersion`) | `self-hosted` |
| `v2_02.GetKeyVaultSecrets.yml` | Reusable: Azure login + Key Vault secret retrieval | `workflow_call` (outputs: secrets) | `self-hosted` |
| `20.DeploySamples.yml` | Build and deploy sample apps to Azure App Service | Manual dispatch | `self-hosted` |
| `21.DeployAppService.yml` | Reusable: deploy a single app to Azure App Service | `workflow_call` (inputs: environment, appName, webAppName) | `self-hosted` + `ubuntu-latest` |
| `quarto-publish.yml` | Render Quarto docs and deploy to GitHub Pages | Push/PR to `main`, manual dispatch | `ubuntu-latest` |
| `github-pages.ym_` | Legacy GitHub Pages deployment (disabled via `.ym_` extension) | Push to `main` | `ubuntu-latest` |

### Workflow Dependency Graph

```
v2_99.Package.CICD.yml (main CI/CD)
  │
  ├─► v2_00.InstallActions.yml (checkout, .NET, NuGet)
  │
  ├─► v2_01.GetCompositeVariables.yml (version computation)
  │     └─ depends on: installActions
  │
  ├─► build (job: restore, build Debug+Release, upload artifacts)
  │     └─ depends on: getCompositeVariables
  │
  ├─► publishNugetPackage (job: download artifact, push to nuget.org)
  │     └─ depends on: build, getCompositeVariables
  │
  └─► upload2AzureFolder (job: download artifact, copy to Azure Storage)
        └─ depends on: build, getCompositeVariables

20.DeploySamples.yml (sample deployment)
  │
  ├─► build (job: checkout, build, publish samples, upload artifact)
  │
  ├─► afterBuild (job: dump outputs)
  │     └─ depends on: build
  │
  ├─► deployAuthenticationSampleApi
  │     └─ calls: 21.DeployAppService.yml (matrix: [Testms])
  │     └─ depends on: afterBuild, build
  │
  └─► deployAuthenticationSampleServerApi
        └─ calls: 21.DeployAppService.yml (matrix: [Testms])
        └─ depends on: afterBuild, build

v3.yml (tag-based NuGet publish)
  └─► build-and-publish (single job: checkout, restore, build, push NuGet)
```

### Key Design Patterns Used

| Pattern | Implementation |
|---------|---------------|
| **Reusable workflows** | `v2_00`, `v2_01`, `v2_02`, `21.DeployAppService` are `workflow_call` workflows composed into main pipelines |
| **Job output chaining** | `getCompositeVariables` outputs `assemblyVersion` → consumed by `build` and `publishNugetPackage` via `needs.*.outputs.*` |
| **Matrix deployments** | `20.DeploySamples.yml` uses `strategy.matrix.environment: [Testms]` for per-environment deployment |
| **Artifact flow** | Build job uploads `.nupkg` files → publish/deploy jobs download via `actions/download-artifact` |
| **OIDC authentication** | `21.DeployAppService.yml` uses `azure/login@v2` with federated credentials (`client-id`, `tenant-id`, `subscription-id` from `vars.*`) |
| **Secrets inheritance** | Caller workflows pass `secrets: inherit` to reusable workflows |
| **Path-scoped triggers** | CI/CD pipeline triggers only on changes to `src/Diginsight.Components*/**` |
| **Tag-based versioning** | `v3.yml` extracts version from git tag (`GITHUB_REF_NAME` minus `v` prefix) |
| **Cross-repo checkout** | `20.DeploySamples.yml` checks out `diginsight/components.internal` for private appsettings |
| **Concurrency control** | `quarto-publish.yml` uses `concurrency.group: "pages"` to prevent overlapping Pages deployments |
| **Environment protection** | `21.DeployAppService.yml` references `environment: ${{ inputs.environment }}` for deployment gates |
| **NuGet cache** | `20.DeploySamples.yml` uses `actions/cache@v4` with `hashFiles('**/packages.lock.json')` |
| **Disabled workflows** | `github-pages.ym_` — renamed extension to disable without deleting |

---

## 6. Appendix — Comparison: GitHub Actions vs Azure DevOps Pipelines

### 6.1 Terminology Mapping

| Azure DevOps | GitHub Actions | Notes |
|-------------|---------------|-------|
| **Pipeline** | **Workflow** | Top-level automation definition |
| **Stage** | *(no equivalent)* | GitHub uses job dependencies to simulate stages |
| **Job** | **Job** | Unit of work on a single agent/runner |
| **Deployment Job** | **Job + Environment** | `deployment:` keyword vs `environment:` property |
| **Step** | **Step** | Single task within a job |
| **Task** (e.g., `DotNetCoreCLI@2`) | **Action** (e.g., `actions/setup-dotnet@v4`) | Reusable automation unit |
| **Template** | **Reusable Workflow / Composite Action** | Two mechanisms depending on granularity |
| **Variable Group** | **Variables (repo/org/env)** | Centralized configuration |
| **Service Connection** | **Secret / OIDC Federated Credential** | External service authentication |
| **Agent Pool** | **Runner / Runner Group** | Execution environment |
| **Demands** | **Runner Labels** | Agent/runner capability matching |
| **Artifact** | **Artifact** | Files passed between jobs |
| **Environment** | **Environment** | Deployment target with protection rules |
| **Pipeline Artifact** | **Artifact** via `upload-artifact`/`download-artifact` actions |
| **`extends`** | *(no equivalent)* | GitHub has no enforced template inheritance |
| **`${{ }}` (compile-time)** | **`${{ }}` (runtime)** | Same syntax, different evaluation timing |
| **`$[ ]` (runtime)** | *(no equivalent)* | GitHub has only one expression syntax |
| **`condition:`** | **`if:`** | Job/step conditional execution |
| **`##vso[task.setvariable]`** | **`>> $GITHUB_OUTPUT`** / **`>> $GITHUB_ENV`** | Setting outputs and env vars |

### 6.2 Structural Comparison

| Capability | Azure DevOps | GitHub Actions |
|-----------|-------------|---------------|
| **Pipeline hierarchy** | Stages → Jobs → Steps | Jobs → Steps (no stages) |
| **Template/reuse granularity** | Steps, Jobs, Stages, Variables templates | Composite Actions (steps), Reusable Workflows (jobs) |
| **Template parameters** | `string`, `boolean`, `number`, `object`, `step`, `stepList`, `job`, `jobList`, `stage`, `stageList` | `string`, `boolean`, `number` only |
| **Dynamic generation** | `${{ each }}` iterates over parameter lists at compile time | Matrix strategy or dynamic JSON matrix via job outputs |
| **Object spread** | `${{ insert }}: ${{ item }}` | Not available |
| **Compile-time vs runtime** | Separate phases — `${{ }}` at compile-time, `$[ ]` and `condition:` at runtime | Single phase — all `${{ }}` evaluated at runtime |
| **Stage-level deployment gates** | Via Environments with checks | Via Environment protection rules |
| **Approval workflow** | Environment checks + `ManualValidation@0` task | Environment required reviewers + wait timers |
| **Artifact mechanism** | `PublishPipelineArtifact`/`download: current` | `upload-artifact`/`download-artifact` actions |

### 6.3 Security Comparison

| Capability | Azure DevOps | GitHub Actions |
|-----------|-------------|---------------|
| **Secret storage** | Variable Groups, Key Vault integration | Repository/Org/Environment Secrets |
| **Secret masking** | Manual (`issecret=true` logging command) | Automatic (all secrets masked in logs) |
| **External auth** | Service Connections (Azure RM, Kubernetes, Docker, etc.) | OIDC Federated Credentials + Secrets |
| **Secretless cloud auth** | Workload Identity Federation (preview) | OIDC natively supported (GA) |
| **Fork PR security** | Not applicable (no fork model in Azure Repos) | Secrets blocked from fork PRs; `pull_request_target` for controlled access |
| **Template enforcement** | `extends` + required template checks | No equivalent — org-level action restrictions only |
| **Branch protection for deployments** | Environment branch filters | Environment deployment branch rules |
| **Pipeline/workflow authorization** | Service connections require per-pipeline authorization | Secrets available to all workflows in the repo (unless environment-scoped) |
| **Built-in SAST** | Microsoft Security DevOps extension | GitHub CodeQL (Advanced Security) |
| **Built-in SCA** | None built-in (use third-party) | Dependabot + Dependency Review Action |
| **Secret scanning** | None built-in | GitHub Secret Scanning + Push Protection |
| **Supply chain attestation** | None built-in | Artifact Attestations (SLSA provenance) |
| **Audit logging** | Azure DevOps audit logs | GitHub Audit Log |

### 6.4 Extensibility Comparison

| Capability | Azure DevOps | GitHub Actions |
|-----------|-------------|---------------|
| **Step-level reuse** | Steps templates (`template: steps/build.yml`) | Composite Actions (`uses: ./.github/actions/build`) |
| **Job-level reuse** | Jobs templates | Reusable Workflows (`uses: ./.github/workflows/deploy.yml`) |
| **Stage-level reuse** | Stages templates | Not available (no stage concept) |
| **Cross-repo templates** | `resources.repositories` + `template: file@repo` | `uses: org/repo/.github/workflows/file.yml@ref` |
| **Marketplace** | Azure DevOps Marketplace (tasks, extensions) | GitHub Marketplace (20,000+ actions) |
| **Custom task/action types** | TypeScript/PowerShell extensions | JavaScript, Docker, Composite actions |
| **Matrix builds** | `strategy.matrix` (limited) | `strategy.matrix` with `include`/`exclude`, dynamic JSON |
| **Service containers** | Container jobs | `services:` sidecar containers |
| **Caching** | `Cache@2` task | `actions/cache@v4` (more tightly integrated) |
| **Manual approval** | `ManualValidation@0` task (server job) | Environment protection rules (required reviewers) |
| **Event-driven triggers** | Limited (CI/CD triggers, scheduled, pipeline completion) | Rich (25+ event types including `issues`, `discussion`, `release`, `workflow_run`) |
| **API trigger** | Not natively (use service hooks) | `repository_dispatch` + API call |

### 6.5 Limitations Comparison

| Limitation Area | Azure DevOps | GitHub Actions |
|----------------|-------------|---------------|
| **Compile-time service connection validation** | ⚠️ Major — connections validated even behind `condition:` guards; must use `${{ if }}` | ✅ Not an issue — no compile-time validation |
| **Dynamic connection names** | ❌ Not supported at runtime | ✅ Secrets can be dynamically selected via expressions |
| **Cross-stage variable passing** | ⚠️ Verbose `stageDependencies` syntax | ⚠️ `needs.*.outputs.*` syntax (similar verbosity) |
| **Stages** | ✅ First-class concept | ❌ No stages — must simulate with job dependencies |
| **Template parameter types** | ✅ Rich (object, stepList, stageList, etc.) | ❌ Limited (string, boolean, number only) |
| **Dynamic stage/job generation** | ✅ `${{ each }}` over parameter lists | ⚠️ Matrix strategy + `fromJSON()` (less flexible) |
| **String manipulation in expressions** | ❌ No split/replace/substring | ❌ No split/replace/substring |
| **Arithmetic in expressions** | ❌ Not supported | ❌ Not supported |
| **Template enforcement** | ✅ `extends` + required template checks | ❌ No equivalent (org-level restrictions only) |
| **YAML anchors** | ❌ Not supported | ❌ Not supported |
| **Reusable nesting depth** | ✅ Unlimited (templates reference templates) | ⚠️ 4 levels maximum |
| **Max job duration** | 60 min (free) / 360 min (paid) hosted | 360 min (hosted) / 35 days (self-hosted) |
| **Scheduled triggers on non-default branches** | ✅ Supported | ❌ Default branch only |
| **Fork PR model** | N/A (Azure Repos has no forks) | ✅ Built-in fork security model |
| **Agent/runner GPU support** | ❌ No hosted GPU | ⚠️ Limited larger runners with GPU (Enterprise) |
| **Environment sharing across projects/repos** | ❌ Project-scoped | ❌ Repository-scoped |
| **Workflow file size** | No documented limit | 512 KB maximum |
| **Free tier (private)** | 1 parallel job, 1,800 min/month | 2,000 min/month Linux (with multipliers) |
| **Self-hosted agent cost** | ~$15/month per parallel slot | Free (unlimited) |

### 6.6 When to Choose Which

| Scenario | Recommended | Rationale |
|----------|-------------|-----------|
| **Source code on GitHub** | GitHub Actions | Native integration, no external CI/CD setup needed |
| **Source code on Azure Repos** | Azure DevOps | Native integration; GitHub Actions requires mirror setup |
| **Complex multi-stage pipelines** | Azure DevOps | First-class stage concept, richer template parameters, `${{ each }}` |
| **Open-source projects** | GitHub Actions | Unlimited free minutes, rich community action ecosystem |
| **Enterprise governance** | Azure DevOps | `extends` enforcement, required template checks, tighter control |
| **Cloud-native deployments** | GitHub Actions | Native OIDC, simpler Azure/AWS/GCP integration |
| **Matrix testing across OS/versions** | GitHub Actions | More flexible matrix strategy with include/exclude |
| **Self-hosted runners at scale** | GitHub Actions | Free, unlimited runners; ARC for K8s auto-scaling |
| **Integration with Azure Boards** | Azure DevOps | Native work item linking, board integration |
| **Integration with GitHub Issues/PRs** | GitHub Actions | Native event triggers, status checks, PR comments |
| **Security scanning** | GitHub Actions | Built-in CodeQL, Dependabot, secret scanning, SBOM |
| **Hybrid (Azure Repos + GitHub Actions)** | Either | Both support cross-platform triggers via webhooks/dispatch |
