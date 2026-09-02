---
title: "CI/CD rework for NuGet.org independence, and the four defects that blocked its first runs"
author: "Dario Airoldi"
date: "2026-09-01"
categories: [cicd, github-actions, self-hosted-runner, packaging, nuget]
---

# ISSUE: CI/CD rework for NuGet.org independence, and the four defects that blocked its first runs

**Date:** 2026-09-01  
**Author:** Dario Airoldi  
**Status:** Resolved — four fixes committed; build and GitHub Release verified on live runs, NuGet push pending  
**Severity:** High (no package could be built, released, or published)  
**Component:** `diginsight/components` CI/CD (`v2_99.Package.CICD.yml` chain), `eng/` release tooling, and `quarto-publish.yml`  
**Target Framework:** net8.0, net9.0, net10.0 (plus `net*-windows` for the WPF projects)  

---

## 📋 Table of Contents

1. [📝 Description](#-description)
2. [🎯 Goal and Rationale](#-goal-and-rationale)
3. [🏗️ Implementation](#-implementation)
4. [🔍 Context Information](#-context-information)
5. [🔬 Analysis](#-analysis)
6. [🔄 Reproduction Steps](#-reproduction-steps)
7. [✅ Solution Implemented](#-solution-implemented)
8. [📚 Additional Information](#-additional-information)
9. [🔗 References](#-references)
10. [✔️ Resolution Status](#-resolution-status)
11. [🎓 Lessons Learned](#-lessons-learned)
12. [📎 Appendix](#-appendix)

---

## 📝 DESCRIPTION

The `Diginsight.Components CICD main` pipeline had just been rewritten to make the repository
**independent of NuGet.org propagation latency in both directions** — consuming upstream
`Diginsight.*` packages from GitHub Releases, and publishing its own packages as a verified GitHub
Release before they ever reach NuGet.org. The design is described in
[🎯 Goal and Rationale](#-goal-and-rationale) and [🏗️ Implementation](#-implementation).

After that rewrite the pipeline never produced a package. Four independent defects were found, each
masking the next — **none of them in the new design itself**; all four were environmental or
workflow-configuration problems:

| # | Defect | Where | Effect |
|---|--------|-------|--------|
| 1 | **No self-hosted runner registered for `diginsight/components`** | GitHub repository settings / build agent | Every run sat in `queued` forever |
| 2 | **`actions/setup-dotnet` cannot write to `C:\Program Files\dotnet`** | `v2_00.InstallActions.yml`, `v2_03.BuildPackages.yml` | First job failed at step 4 |
| 3 | **Workflow triggers overlap** | `quarto-publish.yml` (no `paths:` at all), `v2_99.Package.CICD.yml` (`'.github/workflows/**'`, `'eng/**'`) | Doc-only commits ran a package build+release; workflow edits ran the doc site |
| 4 | **`publishPackages` never starts** — `NUGET_API_KEY_V3` declared `required: true`, but the repository defines `NUGET_API_KEY` | `v2_04.PublishPackages.yml` (also referenced in `v2_00`, `v2_02`) | Build and GitHub Release succeed; the NuGet.org push fails in 3 s with zero steps |

### Error Message

Defect 1 — GitHub Actions run page, job `installActions / main`:

```text
Requested labels: self-hosted
Job defined at: diginsight/components/.github/workflows/v2_00.InstallActions.yml@refs/heads/main
Reusable workflow chain:
diginsight/components/.github/workflows/v2_99.Package.CICD.yml@refs/heads/main (a89f5662...)
-> diginsight/components/.github/workflows/v2_00.InstallActions.yml@refs/heads/main (a89f5662...)
Waiting for a runner to pick up this job...
```

Defect 2 — same job, step `Run actions/setup-dotnet@v4`:

```text
"C:\Program Files\PowerShell\7\pwsh.exe" ... -Command & 'C:\actions-runner\components\_work\_actions\actions\setup-dotnet\v4\externals\install-dotnet.ps1' -SkipNonVersionedFiles -Runtime dotnet -Channel LTS
dotnet-install: The current user doesn't have write access to the installation root 'C:\Program Files\dotnet' to install .NET. Please try specifying a different installation directory using the -InstallDir parameter, or ensure the selected directory has the appropriate permissions.
Exception: C:\actions-runner\components\_work\_actions\actions\setup-dotnet\v4\externals\install-dotnet.ps1:1231
Line |
1231 |      throw
     |      ~~~~~
     | ScriptHalted
Error: Failed to install dotnet, exit code: 1.
```

Defect 4 — run `33493060470`, job `publishPackages / Publish packages to nuget.org`:

```text
Error when evaluating 'secrets'. .github/workflows/v2_99.Package.CICD.yml (Line: 69, Col: 11):
Secret NUGET_API_KEY_V3 is required, but not provided while calling.
```

The job record shows the call itself being rejected — no step ever ran, so no log is produced:

```text
status       : completed
conclusion   : failure
started_at   : 09:43:39Z
completed_at : 09:43:42Z      # 3 seconds
runner_name  : (empty)        # never assigned
steps        : (none)
```

### Impact

- No `Diginsight.Components.*` package could be built, released, or published to NuGet.org.
- Two consecutive pushes to `main` (`1647f85` "cicd fix", `a89f566` "cred provider fix") produced runs stuck in `queued`; a third (`df25628` "metric code cleanup") completed as `action_required` with zero jobs.
- Every doc-only commit was spending a full self-hosted Windows build slot; conversely, every workflow edit re-rendered and redeployed the Quarto site.
- The `eng/` release tooling introduced in `1647f85` (`Download-PackageRelease.ps1`, `Publish-Packages.ps1`) had never been exercised in CI, so its correctness was still unverified.

---

## 🎯 GOAL AND RATIONALE

### The two NuGet.org limitations being worked around

They are different problems and need different answers.

| # | Limitation | Consequence before the rework |
|---|------------|-------------------------------|
| 1 | **Propagation latency** | A version pushed to NuGet.org is not immediately restorable. Indexing takes time, and the corporate proxy (`packagefeedproxy.microsoft.io`) adds its own mirroring delay. `components` therefore could not build against a `Diginsight.Core 3.8.0.1` that `telemetry` had just shipped \u2014 a coordinated three-repository change was serialised by feed latency, not by engineering. |
| 2 | **Reachability** | The self-hosted Windows runner sits on the corporate network and **cannot reach `api.nuget.org` at all**. Yet the build must run on Windows, because `Diginsight.Components.Presentation` targets `net*-windows` with WPF. So the machine that can build is not the machine that can publish. |

A third, non-negotiable constraint shaped every decision: the workaround must not weaken
correctness. It must keep `--locked-mode` determinism, must not let unverified bytes reach
consumers, and must never silently fall back to a stale feed.

### Goals

| # | Goal | Meaning in practice |
|---|------|---------------------|
| 1 | **Restore from releases** | Build against a pinned upstream version the moment it is released, without waiting for NuGet.org or the proxy |
| 2 | **Publish as releases** | Attach the exact `.nupkg`/`.snupkg` bytes to a durable, verified GitHub Release **before** pushing to NuGet.org |
| 3 | **Fully automatic** | No manual tag, no manual version bump, no manual release step \u2014 a push to `main` does everything |
| 4 | **No developer friction** | A plain `dotnet restore`, and any Visual Studio / VS Code build, must just work \u2014 no wrapper script, no `--source` argument, no IDE configuration |
| 5 | **Fail closed** | Never fall back to the proxy silently, never overwrite an already published version |

### Rationale: two channels fed by one build

```text
                              ┌→ GitHub Release assets   (immediate, maintainer-to-maintainer)
version → build/pack ONCE ────┤
                              └→ NuGet.org               (eventual, consumer-facing)
```

The same staged bytes go to both channels; nothing is ever repacked. That is what makes the
GitHub Release a *safe* fast path rather than a parallel, divergent artifact.

**Why a GitHub Release is the right transport**

- The self-hosted runner **can** reach `api.github.com` even though it cannot reach `api.nuget.org` — this was already proven, since the pre-existing pipeline created releases from that runner.
- Authentication already exists for the whole team; no new credential, feed, or infrastructure.
- A release is immutable per tag, durable, and addressable directly by version.
- Assets are **individual files**, so the download folder is directly usable as a NuGet folder source. A single archive would not be, because NuGet needs `.nupkg` files sitting in a directory.
- It can be verified *remotely*: re-download the assets and byte-compare them against what was staged locally.

**Why not the alternatives**

| Alternative | Why it was rejected |
|-------------|---------------------|
| A private/internal NuGet feed | Another moving part to operate, and it would still have its own propagation delay |
| Committing `.nupkg` files to the repository | Repository bloat, and no immutability guarantee |
| Building the upstreams from source | Loses the "exactly the bytes that shipped" property, so a local build could diverge from the released package |
| Just waiting for NuGet.org | The original problem |

**Why the runner split is a consequence, not a preference**

| Job | Runner | Forced by |
|-----|--------|-----------|
| `buildPackages` | self-hosted **Windows** | WPF / `net*-windows` cannot build on a Linux runner |
| `publishPackages` | `ubuntu-latest` | `api.nuget.org` is unreachable from the self-hosted runner |

Because the two halves must run on different machines, the artifact crossing between them has to be
explicit and verifiable — which is precisely what the release-plus-artifact handoff provides.

---

## 🏗️ IMPLEMENTATION

### Consume side — restore from upstream releases

| File | Role |
|------|------|
| `eng/upstream-releases.json` | Maps each upstream repository to the MSBuild property that pins it |
| `eng/Download-PackageRelease.ps1` | Resolves, downloads and verifies the releases; publishes them into the local source |
| `src/Directory.Build.props` | The **only** place upstream versions live (`DiginsightCoreVersion`, `DiginsightSmartCacheVersion` = `3.8.0.1`) |
| `src/NuGet.Config` | Declares `artifacts/packages` as the `local-release` package source |
| `artifacts/packages/.gitkeep` | Tracked marker that keeps the folder source existing |

Flow:

1. Read the upstream inventory (`diginsight/telemetry` → `DiginsightCoreVersion`, `diginsight/smartcache` → `DiginsightSmartCacheVersion`).
2. Read the pinned version with `dotnet msbuild -getProperty:` — the version is never duplicated into the tooling.
3. Resolve the release tag, probing both spellings, because NuGet drops a zero fourth component (`v3.8.0.0` → `3.8.0`).
4. `gh release download --pattern '*.nupkg' --pattern 'SHA256SUMS' --pattern 'release-manifest.json'`.
5. Verify: `release-manifest.json` and `SHA256SUMS` must agree, every file must match its SHA-256, and each package's embedded `.nuspec` identity must match what the manifest claims.
6. Merge all upstreams into one staging folder and replace `artifacts/packages` **atomically** — a failure on any single upstream leaves the local source exactly as it was.

After that, a **plain** restore resolves the packages, because `src/NuGet.Config` declares:

```xml
<packageSources>
  <clear />
  <!-- Populated by eng/Download-PackageRelease.ps1. Must exist even when empty, or restore fails with NU1301. -->
  <add key="local-release" value="../artifacts/packages" />
  <!-- Must NOT be named "nuget.org": disabledPackageSources matches by key and survives <clear />,
       so the user-level disable of "nuget.org" would silently disable this proxy. -->
  <add key="azure-default" value="https://packagefeedproxy.microsoft.io/nuget/v3/index.json" protocolVersion="3" />
</packageSources>
```

Three non-obvious decisions are encoded there and are easy to undo by accident:

- **The config lives at `src/`, not the repository root.** The nearer config wins and its `<clear />` applies, so a root-level `NuGet.Config` would be silently ignored for everything under `src/`.
- **The proxy source is deliberately not named `nuget.org`.** `disabledPackageSources` matches by key and survives `<clear />`, so a user-level disable of `nuget.org` would silently disable the proxy.
- **`artifacts/packages/.gitkeep` is load-bearing.** A configured folder source that does not exist yields `NU1301`, which cannot be suppressed by `NoWarn`, `RestoreNoWarn` or `WarningsNotAsErrors`. `.gitignore` keeps it tracked with a scoped rule set, because a blanket `artifacts/` exclusion would make re-inclusion impossible, and the Visual Studio template's `**/[Pp]ackages/*` rule matches `artifacts/packages/` and must be negated *after* it.

When the pinned version has already reached the proxy, the download step is unnecessary and a plain
restore works with an empty `artifacts/packages`. The fast path is additive, not mandatory.

### Produce side — publish as a verified release

| File | Role |
|------|------|
| `eng/package-manifest.json` | Tracked inventory of the 8 packable projects, each `symbolsRequired: true` |
| `eng/Publish-Packages.ps1` | `ResolveVersion` · `Stage` · `Validate` · `Compare` · `PublishNuGet` |
| `.github/workflows/v2_03.BuildPackages.yml` | Build once, stage, create and remotely verify the release |
| `.github/workflows/v2_04.PublishPackages.yml` | Push the same bytes to NuGet.org |

The `buildPackages` job, in order:

1. `ResolveVersion` — assert the tag really maps to the computed version, before anything is built.
2. Run both test suites (8 + 18 tests) so the release tooling is exercised before it is trusted.
3. `Download-PackageRelease.ps1` — fill the local source from the upstream releases.
4. `dotnet restore --locked-mode` — determinism preserved; the local source changes *where* packages come from, not *which* ones.
5. **Assert the working tree is unchanged** — bootstrap and restore must not mutate tracked files.
6. `dotnet build -p:Version=$PACKAGE_VERSION` — no `.csproj` rewriting (the previous pipeline regex-replaced `<Version>` in every project, which mutated the tree and rewrote file encodings as a side effect).
7. `Stage` — collect the packages named in `package-manifest.json` (**not** a wildcard search, so a package that silently stops being produced fails the release instead of shipping an incomplete set), then emit `SHA256SUMS` and `release-manifest.json`.
8. Create the release, upload any missing asset, and **refuse to overwrite** an existing asset whose bytes differ.
9. Re-download the whole release and run `Validate` + `Compare` against the staged bytes → `release-verified=true`.

`publishPackages` then downloads the immutable artifact, re-validates it independently, and runs
`dotnet nuget push --skip-duplicate`.

### Versioning

```text
assemblyVersion = VERSION_PREFIX . (github.run_number + BUILD_NUMBER_OFFSET)   e.g. 1.0.0.109
sourceTag       = v{assemblyVersion}                                           e.g. v1.0.0.109
```

The tag is an **output, not an input**: `gh release create <tag> --target $GITHUB_SHA` creates the
git tag itself, so nothing is tagged by hand. `--verify-tag` must **not** be used here, unlike in
telemetry and smartcache where the tag is the human-authored trigger. `VERSION_PREFIX` and
`BUILD_NUMBER_OFFSET` are repository variables and must be treated as append-only: lowering either
can produce an already-published version, which then fails closed rather than overwriting anything.

### Safety model

| Control | Effect |
|---------|--------|
| `publishPackages` gated on `releaseVerified == 'true'` | Only remotely verified bytes can be pushed |
| `publishPackages` holds `permissions: contents: read` | The publishing job cannot create or mutate a release |
| The NuGet credential exists only in `publishPackages` | The build job never sees it |
| Pull requests build, stage and validate only | No release, no publication from a PR |
| Staged bytes must match any existing asset of the same name | A published version can never be silently replaced |
| A missing or corrupt upstream release fails the build **before** restore | No silent fallback to the proxy, which would reintroduce the latency this design removes |

### Recovery semantics

| Failure point | State | Recovery |
|---------------|-------|----------|
| Tests, download, restore, build, staging or validation | Nothing published | Fix and re-run |
| Release created, NuGet push failed | The release already unblocks dependent builds | Re-run; `--skip-duplicate` makes the push idempotent |
| Release upload interrupted | Some assets present | A re-run keeps byte-identical assets, uploads the rest, and re-verifies the whole inventory |

This is exactly the situation the pipeline landed in with defect 4: releases `v1.0.0.108` and
`v1.0.0.109` exist and are verified, so only the push has to be repeated — no rebuild.

---

## 🔍 CONTEXT INFORMATION

### Environment Details

- **Repository:** `diginsight/components`, branch `main`
- **Working copy:** `C:\dev\darioa\Diginsight\components`
- **Authoritative pipeline:** [.github/workflows/v2_99.Package.CICD.yml](../../../../../.github/workflows/v2_99.Package.CICD.yml)
- **Build agent:** `AIROLDI02` (Windows), GitHub Actions self-hosted runner
- **Previous build agent:** `AIROLDI01` (no longer serving this repository)
- **Runner service account:** `NT AUTHORITY\NETWORK SERVICE`
- **Installed SDKs on the agent:** 9.0.317, 10.0.111, 10.0.400, 11.0.100-preview.7.26381.103 (all under `C:\Program Files\dotnet`)
- **Upstreams:** `diginsight/telemetry` and `diginsight/smartcache`, both pinned to `3.8.0.1`

### Pipeline shape under review

```text
push to main
  → installActions          (self-hosted)   diagnostics, setup-dotnet, setup-nuget
  → getCompositeVariables   (self-hosted)   assemblyVersion = <prefix>.<run+offset>, sourceTag = v<version>
  → buildPackages           (self-hosted)   Windows/WPF build, stage, create + verify GitHub Release
  → publishPackages         (ubuntu-latest) push the verified bytes to NuGet.org
```

The two constraints that force this split are described in
[🎯 Goal and Rationale](#-goal-and-rationale); both were re-checked during this investigation and
confirmed still valid:

| Constraint | Source | Consequence |
|------------|--------|-------------|
| `Diginsight.Components.Presentation` targets `net8.0-windows;net9.0-windows;net10.0-windows` with `<UseWPF>true</UseWPF>` | `Diginsight.Components.Presentation.csproj` | The build **cannot** move to `ubuntu-latest` |
| The self-hosted runner cannot reach `https://api.nuget.org` | comment in `v2_04.PublishPackages.yml` | The push **must** run on a GitHub-hosted runner |

### Exception Details

| Property | Value |
|----------|-------|
| **Failure type (defect 1)** | Job scheduling — no runner matched label `self-hosted` |
| **Failure type (defect 2)** | `ScriptHalted` from `install-dotnet.ps1:1231` (access denied on install root) |
| **Failure type (defect 3)** | No exception — incorrect `on.push.paths` configuration |
| **Exit code (defect 2)** | 1 |
| **Data Loss** | None |

### Diagnostic evidence collected

```text
# queued runs — never assigned to a runner
gh api repos/diginsight/components/actions/runs/33489051552/jobs
  {"name":"installActions / main","status":"queued","labels":["self-hosted"],
   "runner_name":"","runner_group_name":""}

# last successful run of this pipeline (24/05/2026) ran elsewhere
gh api repos/diginsight/components/actions/runs/26357642375/jobs
  {"name":"installActions / main","conclusion":"success","runner_name":"AIROLDI01"}

# runners actually installed on the current dev box
Get-CimInstance Win32_Service -Filter "Name LIKE '%actions.runner%'"
  actions.runner.darioairoldi-Learn.AIROLDI02     NT AUTHORITY\NETWORK SERVICE
  actions.runner.darioa_microsoft-aicm.AIROLDI02  NT AUTHORITY\NETWORK SERVICE
  actions.runner.diginsight-smartdocs.AIROLDI02   NT AUTHORITY\NETWORK SERVICE
  # (no diginsight/components entry)

$env:COMPUTERNAME  →  AIROLDI02
```

### Run history

| Run | Commit | Workflow | Outcome |
|-----|--------|----------|---------|
| `33428417321` | `df25628` metric code cleanup | CICD main | `completed / action_required`, **0 jobs** |
| `33480679735` | `1647f85` cicd fix | CICD main | `queued` indefinitely |
| `33489051552` | `a89f566` cred provider fix | CICD main | `queued`, then failed at `setup-dotnet` once the runner was installed |
| `33493060470` | `e1fed5f` tool-cache fix | CICD main | `installActions` ✅, `getCompositeVariables` ✅, `buildPackages` ✅ (release `v1.0.0.108` created and verified), `publishPackages` ❌ (failed in 3 s, no steps) |
| `33493745057` | `0b27164` trigger split | CICD main | Same shape — release `v1.0.0.109` created and verified, `publishPackages` ❌ on the same secret |

---

## 🔬 ANALYSIS

### Root Cause Analysis

#### Primary cause (defect 1): the runner moved machines and was never re-registered

Every job in the chain declares `runs-on: self-hosted`. GitHub queues such a job until a runner
carrying that label comes online for the repository; it does **not** fail fast. The last successful
run of this pipeline executed on `AIROLDI01`. Development has since moved to `AIROLDI02`, where
runners exist for `darioairoldi/Learn`, `darioa_microsoft/aicm` and `diginsight/smartdocs` — but not
for `diginsight/components`. A GitHub Actions runner is bound to exactly one repository (or org) at
registration time, so the three existing runners could not serve this repository.

The empty `runner_name` in the jobs API is the decisive signal: the job was never *assigned*, as
opposed to being assigned and then hanging.

#### Secondary cause (defect 2): `setup-dotnet` writes to `C:\Program Files` by default

`actions/setup-dotnet@v4` computes its install root as follows (from the action's own bundle,
`_actions/actions/setup-dotnet/v4/dist/setup/index.js`):

```js
DotnetInstallDir.default = {
    linux:   '/usr/share/dotnet',
    mac:     path.join(process.env['HOME'] + '', '.dotnet'),
    windows: path.join(process.env['PROGRAMFILES'] + '', 'dotnet')
};
DotnetInstallDir.dirPath = process.env['DOTNET_INSTALL_DIR']
    ? DotnetInstallDir.convertInstallPathToAbsolute(process.env['DOTNET_INSTALL_DIR'])
    : DotnetInstallDir.default[PLATFORM];
```

The runner service runs as `NT AUTHORITY\NETWORK SERVICE`, which has no write permission on
`C:\Program Files\dotnet`. `install-dotnet.ps1` therefore throws before installing anything.

This is a **runner-configuration defect, not a workflow regression** — it would have hit any freshly
registered non-elevated Windows runner. It was invisible before because `AIROLDI01` had presumably
been configured differently.

#### Tertiary cause (defect 3): triggers were never scoped

[quarto-publish.yml](../../../../../.github/workflows/quarto-publish.yml) declared
`on.push.branches: [main]` with **no `paths:` key at all**, so it ran on literally every push. In the
opposite direction, `v2_99.Package.CICD.yml` listed `'.github/workflows/**'` and the whole of
`'eng/**'`, so editing the Quarto workflow — or `eng/README.md` — launched a build, a GitHub Release
and a NuGet push.

The two trigger sets therefore intersected on: every `.github/workflows/*` file, every `eng/**.md`
file, and every markdown file anywhere in the repository.

#### Quaternary cause (defect 4): the publish job's required secret is not the one the repository defines

Once the first three defects were cleared, `buildPackages` completed and produced a fully verified
GitHub Release — but `publishPackages` failed after **3 seconds with zero steps and no runner**. That
signature means the *call* to the reusable workflow was rejected, not that any work failed.

[v2_04.PublishPackages.yml](../../../../../.github/workflows/v2_04.PublishPackages.yml) declares:

```yaml
    secrets:
      NUGET_API_KEY_V3:
        required: true
```

and `v2_99` calls it with `secrets: inherit`. `inherit` forwards only secrets that actually exist, so
a `required: true` entry that is undefined fails the call before job start. The pre-rewrite pipeline
(`df25628`) used **`secrets.NUGET_API_KEY`**; only the dead `v3.yml` — deleted in `1647f85` and never
successfully run — referenced `NUGET_API_KEY_V3`. The name was carried over from the SmartCache model
without the corresponding secret ever being created here.

**Confirmed.** The run annotation names the failing expression and line, and the repository's
**Settings → Secrets and variables → Actions** page lists exactly three repository secrets —
`AZURE_CLIENT_SECRET`, `INTERNAL_REPOSITORY_TOKEN`, `NUGET_API_KEY` — with no `NUGET_API_KEY_V3`.

The stale name appeared in three workflows, but only `v2_04` declared it `required: true`, which is
why `installActions` started normally and the problem surfaced only at the end of the chain:

| Workflow | Reference | Declared |
|----------|-----------|----------|
| `v2_04.PublishPackages.yml` | `secrets.NUGET_API_KEY_V3` | `required: true` → **fails the call** |
| `v2_00.InstallActions.yml` | `secrets.NUGET_API_KEY_V3` (env + `nuget/setup-nuget`) | `required: false` → silently empty |
| `v2_02.GetKeyVaultSecrets.yml` | `secrets.NUGET_API_KEY_V3` (env) | not declared → silently empty |

### Why the earlier run reported `action_required`

Run `33428417321` completed instantly with conclusion `action_required` and **zero** jobs, i.e. it
was blocked before job creation. That is the same class of pre-execution block as defect 1 (no
capacity/approval to start), and it is why the failure looked intermittent rather than systematic.

### Impact Assessment

| Category | Impact | Severity |
|----------|--------|----------|
| **Release pipeline** | No package built, released or published since 24/05/2026 | High |
| **Feedback loop** | Failures surfaced as a silent `queued` state with no notification | High |
| **Runner capacity** | Doc-only commits consumed a Windows build slot | Medium |
| **Site deployment** | Quarto site redeployed on unrelated source commits | Low |
| **Data integrity** | None — nothing was published or deleted | None |
| **Production runtime** | None | None |

### Affected Workflows

1. ❌ **`Diginsight.Components CICD main`** — blocked at `installActions`, so build/release/publish never ran.
2. ❌ **`actions/setup-dotnet` on any new self-hosted Windows runner** — fails on the default install root.
3. ❌ **`v2_04.PublishPackages`** — rejected at call time on a required secret that the repository does not define.
4. ⚠️ **`Render and Deploy Quarto Site`** — succeeded, but ran on every push regardless of relevance.
5. ⚠️ **`20.DeploySamples.yml`** — same `setup-dotnet` pattern on `self-hosted`; not exercised, still latent.
6. ✅ **`v2_03.BuildPackages`** — after the tool-cache fix, restore-from-releases, build, stage, release and remote verification all succeeded.

---

## 🔄 REPRODUCTION STEPS

### Defect 1 — job never starts

1. Ensure no self-hosted runner is registered for the repository.
2. Push any commit matching the CICD `paths:` filter to `main`.
3. Observe the run enter `queued` and stay there.
4. Confirm with:
   ```powershell
   gh api repos/diginsight/components/actions/runs/<runId>/jobs `
     --jq '.jobs[] | {name, status, labels, runner_name}'
   ```
   An empty `runner_name` proves the job was never assigned.

### Defect 2 — setup-dotnet access denied

1. Register a self-hosted Windows runner as a service (`config.cmd --runasservice`), which defaults
   to `NT AUTHORITY\NETWORK SERVICE`.
2. Run a workflow whose job uses `actions/setup-dotnet@v4` without `DOTNET_INSTALL_DIR`.
3. The step fails with `ScriptHalted` at `install-dotnet.ps1:1231`.
4. Confirm the account with:
   ```powershell
   Get-CimInstance Win32_Service -Filter "Name LIKE '%actions.runner%'" |
     Select-Object Name, StartName
   ```

### Defect 3 — trigger overlap

1. Edit only `.github/workflows/quarto-publish.yml` (or only `eng/README.md`).
2. Push to `main`.
3. Both `Diginsight.Components CICD main` **and** `Render and Deploy Quarto Site` start.

### Affected Configuration Locations

| File | Role |
|------|------|
| [.github/workflows/v2_00.InstallActions.yml](../../../../../.github/workflows/v2_00.InstallActions.yml) | `runs-on: self-hosted`; `setup-dotnet` step that failed |
| [.github/workflows/v2_03.BuildPackages.yml](../../../../../.github/workflows/v2_03.BuildPackages.yml) | Windows/WPF build; `setup-dotnet` with 8.x/9.x/10.x |
| [.github/workflows/v2_99.Package.CICD.yml](../../../../../.github/workflows/v2_99.Package.CICD.yml) | Trigger `paths:` that were too broad |
| [.github/workflows/quarto-publish.yml](../../../../../.github/workflows/quarto-publish.yml) | Missing trigger `paths:` entirely |

This was an infrastructure and workflow-configuration issue, so no application method name or source
line number applies.

---

## ✅ SOLUTION IMPLEMENTED

### Fix Overview

| # | Fix | Type | Commit |
|---|-----|------|--------|
| 1 | Register a self-hosted runner for `diginsight/components` on `AIROLDI02` | Infrastructure (manual) | — |
| 2 | Redirect the .NET install to the runner tool cache | Workflow change | `e1fed5f` |
| 3 | Scope the trigger `paths:` of both workflows | Workflow change | `0b27164` |
| 4 | Align the NuGet publish credential on the existing `NUGET_API_KEY` secret | Workflow change | `d0a7256` |

### Code Changes

#### 1. Register the runner

`C:\actions-runner\components` was created and configured against
`https://github.com/diginsight/components`, producing the service
`actions.runner.diginsight-components.AIROLDI02`. This alone unblocked job scheduling and revealed
defect 2.

#### 2. Install .NET into a writable directory

**Location:** `.github/workflows/v2_00.InstallActions.yml` and `.github/workflows/v2_03.BuildPackages.yml`

```yaml
# BEFORE:
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |-
            8.x
            9.x
            10.x

# AFTER:
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        env:
          # The self-hosted runner service runs as NETWORK SERVICE and cannot write to the default
          # install root (C:\Program Files\dotnet). The tool cache is runner-owned and persists.
          DOTNET_INSTALL_DIR: ${{ runner.tool_cache }}/dotnet
        with:
          dotnet-version: |-
            8.x
            9.x
            10.x
```

Why `${{ runner.tool_cache }}` (`C:\actions-runner\components\_work\_tool`):

- Its ACL grants `NT AUTHORITY\Authenticated Users : Modify`, and `NETWORK SERVICE` is a member.
- It survives between runs, so the SDK download happens once and `setup-dotnet` short-circuits afterwards.
- `setup-dotnet` calls `core.addPath(DOTNET_INSTALL_DIR)` and `core.exportVariable('DOTNET_ROOT', …)`, so every later step resolves the redirected SDK automatically.
- It **must** be declared at **step** level: the `runner` context is not available in a job-level `env:` block.
- `v2_04.PublishPackages.yml` was deliberately left untouched — it runs on `ubuntu-latest`, where the default install root is already writable.

#### 3. Separate the two pipelines' triggers

**Location:** `.github/workflows/quarto-publish.yml` — documentation and site assets only

```yaml
on:
  push:
    branches: [main]
    # Keep in sync with the pull_request list below (GitHub Actions does not support YAML anchors).
    paths:
      - '**.md'
      - '**.qmd'
      - '**.css'
      - '**.scss'
      - '_quarto.yml'
      - 'src/_quarto.yml'
      - 'header-includes.html'
      - '*.svg'
      - 'images/**'
      - 'src/docs/**'
      - '.github/workflows/quarto-publish.yml'
      # docs/ is the rendered output (output-dir in _quarto.yml), never an input.
      - '!docs/**'
```

**Location:** `.github/workflows/v2_99.Package.CICD.yml` — source and build inputs only

```yaml
    paths:
      - 'src/Diginsight.Components*/**'
      - '!src/Diginsight.Components*/**.md'
      - 'src/Directory.Build.props'
      - 'src/Directory.Build.targets'
      - 'src/NuGet.Config'
      - 'src/global.json'                 # newly added: the SDK pin affects the build
      - 'src/*.slnx'
      - 'eng/**'
      - '!eng/**.md'
      - '.github/workflows/v2_*.yml'      # was '.github/workflows/**'
```

#### 4. Align the NuGet publish credential

The repository defines `NUGET_API_KEY`, so the workflows were repointed at it rather than creating a
duplicate secret. All three stale references were changed together, so no workflow is left silently
reading an empty key.

**Location:** `.github/workflows/v2_04.PublishPackages.yml`

```yaml
# BEFORE:
    secrets:
      NUGET_API_KEY_V3:
        required: true
...
      - name: Publish NuGet Packages
        env:
          NUGET_API_KEY: ${{ secrets.NUGET_API_KEY_V3 }}

# AFTER:
    secrets:
      NUGET_API_KEY:
        required: true
...
      - name: Publish NuGet Packages
        env:
          NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}
```

**Location:** `.github/workflows/v2_00.InstallActions.yml` — the `secrets:` declaration, the job-level
`env:`, and the `nuget-api-key:` input of `nuget/setup-nuget@v1`.

**Location:** `.github/workflows/v2_02.GetKeyVaultSecrets.yml` — the job-level `env:`.

`eng/Publish-Packages.ps1` needs no change: it already reads `$env:NUGET_API_KEY` and throws
`'NUGET_API_KEY is required for NuGet publication.'` when it is empty.

Because releases `v1.0.0.108` and `v1.0.0.109` already exist and are verified, re-running only the
`publishPackages` job pushes exactly those bytes — no rebuild is needed.

### Solution Features

#### ✅ Deterministic, non-privileged SDK provisioning

- The runner never needs write access to `C:\Program Files`.
- The pinned `8.x / 9.x / 10.x` matrix is preserved, so build determinism is unchanged.
- No machine-level ACL change and no elevated runner service were required.

#### ✅ Disjoint trigger sets

- A documentation change renders the site and nothing else.
- A source change builds, releases and publishes and nothing else.
- The rendered `docs/` output directory is explicitly excluded, so re-committing generated HTML/CSS cannot re-trigger the site build.

#### ✅ Preserved release contract

None of the release semantics introduced in `1647f85` were altered: build-once, attach the exact
bytes to a GitHub Release, verify them remotely, and only then push the same bytes to NuGet.org.

### Transformation Examples

Path-filter behaviour after the change (simulated against GitHub's glob semantics):

| Changed path | CICD | Quarto |
|--------------|------|--------|
| `src/Diginsight.Components/Helper/DefaultCredentialProvider.cs` | RUN | – |
| `src/Directory.Build.props`, `src/global.json`, `src/*.slnx` | RUN | – |
| `eng/Publish-Packages.ps1`, `eng/upstream-releases.json` | RUN | – |
| `eng/README.md` | – | RUN |
| `.github/workflows/v2_03.BuildPackages.yml` | RUN | – |
| `.github/workflows/quarto-publish.yml` | – | RUN |
| `src/docs/**` (`.md`, `.png`) | – | RUN |
| `README.md`, `index.md`, `_quarto.yml`, `styles.css`, `theme-dark.scss`, `images/**` | – | RUN |
| `docs/index.html`, `docs/site_libs/**.css` (rendered output) | – | – |

No path triggers both workflows.

---

## 📚 ADDITIONAL INFORMATION

### Testing Performed

1. Enumerated all runs of the pipeline and confirmed two were stuck in `queued`.
2. Queried the jobs API and confirmed `runner_name` was empty on both.
3. Identified `AIROLDI01` as the runner of the last successful run and `AIROLDI02` as the current box.
4. Enumerated the four runner services and their `StartName` (`NETWORK SERVICE`).
5. Read the installed `setup-dotnet` bundle on disk and confirmed `DOTNET_INSTALL_DIR` is honoured, and that it is exported to `PATH` / `DOTNET_ROOT` for later steps.
6. Verified the ACL on `C:\actions-runner\components\_work\_tool` grants `Authenticated Users : Modify`.
7. Simulated GitHub's `paths:` glob semantics against 23 representative paths to prove the two trigger sets are disjoint.
8. Confirmed both upstream releases exist: `diginsight/telemetry@v3.8.0.1` and `diginsight/smartcache@v3.8.0.1`.
9. Confirmed `git-lfs` and `gh` are installed on the agent (both are used by the pipeline).
10. Pushed `e1fed5f`; run `33493060470` passed `installActions`, `getCompositeVariables` and `buildPackages` on `AIROLDI02`.
11. Confirmed the release `v1.0.0.108` was created and carries all expected assets: 8 `.nupkg`, 8 `.snupkg`, `release-manifest.json` and `SHA256SUMS`. This is the first end-to-end proof of `Download-PackageRelease.ps1`, the `--locked-mode` restore against the `local-release` source, the Windows/WPF build, and `Publish-Packages.ps1` `Stage` / `Validate` / `Compare`.
12. Established that `publishPackages` fails at call time (3 s, 0 steps, no runner) rather than during the push itself.
13. Confirmed the cause from the run annotation (`Secret NUGET_API_KEY_V3 is required, but not provided while calling`) and the repository secret list, which contains `AZURE_CLIENT_SECRET`, `INTERNAL_REPOSITORY_TOKEN` and `NUGET_API_KEY` only.
14. Repointed all three workflows at `NUGET_API_KEY` and confirmed no `NUGET_API_KEY_V3` reference remains under `.github/workflows/`.

### Testing Recommendations

#### Integration tests

1. **Fresh-runner provisioning**
   - Register a new self-hosted Windows runner as a service.
   - Run the CICD pipeline.
   - Expected: `setup-dotnet` installs into `_work/_tool/dotnet` without elevation.

2. **Trigger isolation**
   - Push a commit touching only `src/docs/**`.
   - Expected: Quarto runs, CICD does not.
   - Push a commit touching only `src/Diginsight.Components*/**/*.cs`.
   - Expected: CICD runs, Quarto does not.

3. **Full release path (dry run)**
   - `workflow_dispatch` with `dryRun: true`.
   - Expected: build + stage + validate succeed, no release, no NuGet push.

### Migration Considerations

#### ⚠️ The runner is a single point of failure

Three of the four jobs require `self-hosted`, but only `buildPackages` genuinely needs Windows.
Moving `installActions` and `getCompositeVariables` to `ubuntu-latest` would reduce the blast radius
of a runner outage from "whole pipeline" to "build only".

#### ⚠️ `src/global.json` pins a preview SDK

`src/global.json` requests `11.0.100-preview.7.26381.103` with `rollForward: latestFeature`, which
cannot fall back to SDK 10. CI installs only 8.x/9.x/10.x. This is currently harmless **only**
because the .NET host resolves `global.json` from the *current working directory* (the repository
root in CI), and no `eng/*.ps1` script does `Set-Location` into `src`. Any future step that runs
`dotnet` from `src/` will fail on the agent.

#### Alternative options considered

**Option 1 — redirect `DOTNET_INSTALL_DIR` (implemented)**
- No machine-level change, no elevation, reversible in a single workflow edit.

**Option 2 — grant `NETWORK SERVICE` write access to `C:\Program Files\dotnet`**
- Fixes it machine-wide but weakens the security posture of a privileged directory.

**Option 3 — drop `setup-dotnet` and rely on the pre-installed SDKs**
- Fastest, but loses version determinism and depends on undocumented agent state.

**Option 4 — run the runner service as an administrator account**
- Broadest privileges for the least benefit; rejected.

### Performance Impact

| Operation | Before fix | After fix | Delta |
|-----------|------------|-----------|-------|
| **Pipeline start** | Never (queued indefinitely) | Immediate on `AIROLDI02` | Unblocked |
| **SDK provisioning** | Failed | ~1 GB downloaded once, cached in `_work/_tool` | One-off cost |
| **Doc-only commit** | Full Windows build + release attempt | Quarto render only | Windows slot freed |
| **Workflow-file edit** | Full build + GitHub Release + NuGet push | Only the relevant workflow | Release noise removed |

### Security Considerations

- ✅ No credentials, tokens, or secrets were added or logged.
- ✅ The fix **narrows** privilege requirements: the runner no longer needs write access to `C:\Program Files`.
- ✅ `publishPackages` remains `permissions: contents: read`, so the publish job still cannot create or modify a release.
- ✅ The NuGet push remains gated on `releaseVerified == 'true'` and never runs from a pull request.
- ⚠️ Self-hosted runners on a developer workstation execute repository-controlled code; keep the repository private to untrusted forks or move to ephemeral runners.

---

## 🔗 REFERENCES

### Official Documentation

- [About self-hosted runners](https://docs.github.com/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners): runner-to-repository binding and labels.
- [Configuring the self-hosted runner application as a service](https://docs.github.com/actions/hosting-your-own-runners/managing-self-hosted-runners/configuring-the-self-hosted-runner-application-as-a-service): the `NETWORK SERVICE` default account.
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet): `DOTNET_INSTALL_DIR` environment override.
- [Workflow syntax — `on.<push>.paths`](https://docs.github.com/actions/writing-workflows/workflow-syntax-for-github-actions#onpushpull_requestpaths): glob semantics and `!` negation ordering.
- [Contexts — availability](https://docs.github.com/actions/writing-workflows/choosing-what-your-workflow-does/contexts#context-availability): the `runner` context is not available in job-level `env:`.
- [global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json): resolution starts from the current working directory.
- [Deprecation of Node 20 on GitHub Actions runners](https://github.blog/changelog/2025-09-19-deprecation-of-node-20-on-github-actions-runners/): source of the (benign) warning in the logs.

### Design Documentation

- [eng/README.md](../../../../../eng/README.md) — the authoritative description of the release tooling: developer flow, upstream pinning, versioning, release contents, recovery, and why `artifacts/packages/.gitkeep` is load-bearing.
- [eng/upstream-releases.json](../../../../../eng/upstream-releases.json) — upstream repository → MSBuild version property inventory.
- [eng/package-manifest.json](../../../../../eng/package-manifest.json) — the tracked list of the 8 packable projects that a release must contain.

### Related Issues

- [Debug solution fails because the selected SDK cannot target .NET 11](../20260901.01-componentsbuild/overview.md) — introduced `src/global.json`, the preview SDK pin discussed above.
- [Fix Diginsight Components CI/CD for GitHub Release based package integration](../../202608/20260831.01-restore-by-release/01-fix-cicd-for-release-integration.plan.md) — the plan this pipeline implements, including the findings (F1–F13) that shaped it.

### Code References

#### Modified files

| File | Path | Changes |
|------|------|---------|
| **v2_00.InstallActions.yml** | `.github/workflows/` | Added `DOTNET_INSTALL_DIR` step env; `NUGET_API_KEY_V3` → `NUGET_API_KEY` |
| **v2_02.GetKeyVaultSecrets.yml** | `.github/workflows/` | `NUGET_API_KEY_V3` → `NUGET_API_KEY` |
| **v2_03.BuildPackages.yml** | `.github/workflows/` | Added `DOTNET_INSTALL_DIR` step env |
| **v2_04.PublishPackages.yml** | `.github/workflows/` | `NUGET_API_KEY_V3` → `NUGET_API_KEY` (declaration and step env) |
| **quarto-publish.yml** | `.github/workflows/` | Added `paths:` filters to `push` and `pull_request`, excluded `docs/**` |
| **v2_99.Package.CICD.yml** | `.github/workflows/` | Narrowed `paths:`; added `src/global.json`; excluded markdown; `.github/workflows/**` → `.github/workflows/v2_*.yml` |

#### Commits

- `e1fed5f` — *cicd: install .NET into the runner tool cache instead of Program Files*
- `0b27164` — *cicd: separate triggers - quarto on docs, package pipeline on source*
- `d0a7256` — *cicd: use the NUGET_API_KEY secret the repository actually defines*

---

## ✔️ RESOLUTION STATUS

### 🎯 **STATUS: RESOLVED — awaiting the confirming NuGet push**

**Resolution Date:** 2026-09-01  
**Resolved By:** Dario Airoldi  
**Resolution Type:** Infrastructure provisioning + workflow configuration change

### Verification Checklist

- [x] **Root causes identified**
  - [x] Missing self-hosted runner for `diginsight/components`
  - [x] `setup-dotnet` default install root not writable by `NETWORK SERVICE`
  - [x] Overlapping workflow triggers
  - [x] `publishPackages` rejected at call time on a required secret
- [x] **Infrastructure**
  - [x] Runner `actions.runner.diginsight-components.AIROLDI02` registered and running
  - [x] Tool cache confirmed writable (`Authenticated Users : Modify`)
- [x] **Workflow changes implemented**
  - [x] `DOTNET_INSTALL_DIR` added to both self-hosted `setup-dotnet` steps (`e1fed5f`)
  - [x] `paths:` filters scoped on both workflows (`0b27164`)
  - [x] `NUGET_API_KEY_V3` → `NUGET_API_KEY` in `v2_00`, `v2_02`, `v2_04` (`d0a7256`)
  - [x] YAML validated (no parser diagnostics)
  - [x] Path filters simulated against 23 representative paths
  - [x] No `NUGET_API_KEY_V3` reference remains anywhere under `.github/workflows/`
- [ ] **End-to-end pipeline**
  - [x] `installActions` succeeds on `AIROLDI02`
  - [x] `getCompositeVariables` succeeds (`assemblyVersion = 1.0.0.108`, `sourceTag = v1.0.0.108`)
  - [x] `buildPackages` completes: upstream releases downloaded, `--locked-mode` restore, build, stage, release created and remotely verified (twice: `v1.0.0.108`, `v1.0.0.109`)
  - [ ] `publishPackages` pushes to NuGet.org — fix committed, not yet exercised
- [ ] **Trigger isolation observed on real commits**
  - [ ] Doc-only commit runs Quarto only
  - [ ] Source-only commit runs CICD only

### Follow-up Actions

#### Immediate (Priority 1)
- [ ] Push `d0a7256` and confirm `publishPackages` completes; alternatively re-run only that job on run `33493745057`, whose release `v1.0.0.109` is already verified.
- [ ] Verify the packages appear on NuGet.org, and reconcile the version line — `vars.VERSION_PREFIX` is `1.0.0`, so the pipeline is publishing `Diginsight.Components.* 1.0.0.<run>`.
- [ ] Do **not** commit the locally modified `packages.lock.json` files — they come from the gitignored `Directory.Build.props.user` direct-import build and would break `dotnet restore --locked-mode` in CI.

#### Short-term (Priority 2)
- [ ] Apply the same `DOTNET_INSTALL_DIR` fix to `.github/workflows/20.DeploySamples.yml` (same latent defect).
- [ ] Move `installActions` and `getCompositeVariables` to `ubuntu-latest` so a runner outage only blocks the build.
- [ ] Remove the `git lfs fetch --all` / `git lfs checkout` steps from `v2_00.InstallActions.yml` — LFS was removed in `98360aa`.
- [ ] Decide whether `src/global.json` should pin a preview SDK at all, or be moved/removed so CI and developer machines agree.

#### Long-term (Priority 3)
- [ ] Add a scheduled or `workflow_dispatch` canary so a dead runner is detected without waiting for a release.
- [ ] Document the runner provisioning steps (account, tool cache, prerequisites: `gh`, `git-lfs`) so a new agent can be stood up reproducibly.
- [ ] Consider ephemeral/containerised Windows runners to remove the workstation dependency.

### Success Criteria

✅ **Achieved:**
- The pipeline starts and progresses past the jobs that previously blocked it.
- `setup-dotnet` provisions SDKs without elevated permissions.
- Documentation and source pipelines have disjoint trigger sets.
- The restore-from-releases and publish-as-releases design is proven end to end: releases `v1.0.0.108` and `v1.0.0.109` each contain all 16 package files plus `release-manifest.json` and `SHA256SUMS`, remotely re-downloaded and byte-compared.
- Every workflow now references a secret that exists.

📋 **Pending Verification:**
- A NuGet.org publication from `publishPackages`.
- Observed trigger isolation on real doc-only and source-only commits.

---

## 🎓 LESSONS LEARNED

### What Went Wrong

1. **A queued job is silent.** GitHub does not fail or notify when no runner matches; the pipeline looked "in progress" for hours. Two consecutive releases were lost before anyone noticed.
2. **Runner provisioning was not part of the repository's knowledge.** Nothing recorded that this repository depended on a specific machine, so the migration from `AIROLDI01` to `AIROLDI02` silently dropped it.
3. **A newly registered runner is not equivalent to the old one.** The `NETWORK SERVICE` default made `setup-dotnet` fail in a way that had never occurred before, which briefly looked like a regression in the freshly rewritten workflows.
4. **Triggers were never revisited when the pipeline gained real consequences.** `.github/workflows/**` was harmless when the pipeline only built; once it created releases and pushed to NuGet.org, editing an unrelated workflow could cut a release.
5. **The failure ordering hid the real state.** Four defects stacked, and each was only visible after the previous one was cleared.
6. **A workflow rewrite copied a secret *name* but not the secret.** `NUGET_API_KEY_V3` came from the SmartCache model; this repository has always used `NUGET_API_KEY`. Because the mismatch only bites in the last job, it cost a full build to discover.

### What Went Right

1. **The API told the truth quickly.** `runner_name: ""` in the jobs API distinguished "never scheduled" from "hung" in one call, avoiding a long log hunt.
2. **Reading the action's own bundle beat guessing.** Inspecting `dist/setup/index.js` on disk proved `DOTNET_INSTALL_DIR` was honoured *and* exported, rather than relying on documentation.
3. **The fix reduced privileges instead of raising them.** Redirecting the install root was chosen over granting `NETWORK SERVICE` write access to `C:\Program Files`.
4. **Path filters were simulated before being trusted.** The simulation caught a real glob bug (see below) that would otherwise have shipped.
5. **The release architecture held up.** Nothing in the restore-from-releases / publish-as-releases design was implicated; all four defects were environmental or trigger/secret configuration. The first run that got far enough exercised the whole design at once — upstream download and verification, `--locked-mode` restore against the local source, Windows/WPF build, staging from the tracked manifest, release creation, and remote byte-comparison — and every step passed.
6. **The design's own recovery semantics paid off immediately.** Because the release is created and verified *before* the push, defect 4 left two usable, verified releases behind. Only the push has to be repeated; nothing has to be rebuilt.

### Improvements for Future

1. **Never let a single job type gate the whole chain.** Only `buildPackages` needs Windows; the other jobs should run on GitHub-hosted runners.
2. **Treat trigger `paths:` as a safety control**, not an optimisation — a pipeline that can publish must not be reachable from unrelated file changes.
3. **Glob negations need care.** `dir/**/*.md` does not match `dir/README.md`, because a `**` segment must be followed by a literal `/`. The first attempt at `!eng/**/*.md` therefore failed to exclude `eng/README.md`; the working form is `dir/**.md`.
4. **GitHub Actions does not support YAML anchors/aliases**, so `push` and `pull_request` path lists must be duplicated — add a "keep in sync" comment.
5. **The `runner` context is unavailable in job-level `env:`** — environment values derived from it must be declared per step.
6. **Distinguish noise from failure.** The Node 20 deprecation and `punycode` warnings in the same log were purely informational and cost time to rule out.
7. **Record agent prerequisites in the repository** so a new machine can serve the pipeline without archaeology.
8. **Validate credentials early, not last.** A cheap `installActions` step that asserts the presence of every secret the chain will need would surface a naming mismatch in seconds instead of after a full Windows build and a published GitHub Release.

---

## 📎 APPENDIX

### A. Commands used to diagnose the queued state

```powershell
# recent runs and their status
gh run list --repo diginsight/components --limit 12 `
  --json databaseId,displayTitle,workflowName,status,conclusion,createdAt

# was the job ever assigned to a runner?
gh api repos/diginsight/components/actions/runs/<runId>/jobs `
  --jq '.jobs[] | {name, status, labels, runner_name, runner_group_name}'

# which runner ran the last successful build?
gh api repos/diginsight/components/actions/runs/26357642375/jobs `
  --jq '.jobs[] | {name, conclusion, runner_name}'
```

### B. Commands used to inspect the build agent

```powershell
# runner services and the account they run as
Get-CimInstance Win32_Service -Filter "Name LIKE '%actions.runner%'" |
  Select-Object Name, StartName, PathName

# which repository each runner installation is bound to
Get-ChildItem 'C:\actions-runner' -Directory | ForEach-Object {
    $cfg = Join-Path $_.FullName '.runner'
    if (Test-Path $cfg) {
        [pscustomobject]@{
            Folder    = $_.Name
            GitHubUrl = (Get-Content $cfg -Raw | ConvertFrom-Json).gitHubUrl
        }
    }
}

# is the tool cache writable by NETWORK SERVICE?
(Get-Acl 'C:\actions-runner\components\_work\_tool').Access |
  Select-Object IdentityReference, FileSystemRights, AccessControlType
```

### C. Benign log noise that is *not* a failure

```text
Node 20 is being deprecated. This workflow is running with Node 24 by default.
(node:36724) [DEP0040] DeprecationWarning: The `punycode` module is deprecated.
```

Both come from the runner and the actions' Node bundles. They appear immediately before the real
error and are unrelated to it. No `ACTIONS_ALLOW_USE_UNSECURE_NODE_VERSION` override is needed.

### D. Signature of a called workflow that fails before it starts

```text
name         : publishPackages / Publish packages to nuget.org
status       : completed
conclusion   : failure
started_at   : 01/09/2026 09:43:39
completed_at : 01/09/2026 09:43:42     # 3 seconds
runner_name  :                         # never assigned
steps        :                         # empty
```

Zero steps, no runner, and a few seconds of wall clock mean the *call* was rejected, not the work.
For a reusable workflow the usual cause is a `secrets:` entry declared `required: true` that the
caller cannot supply — `secrets: inherit` forwards only secrets that actually exist.

### E. Design constraints that must survive any future change

| Constraint | Reason |
|------------|--------|
| `buildPackages` stays on a **Windows** runner | `Diginsight.Components.Presentation` uses WPF (`net*-windows`) |
| `publishPackages` stays on a **GitHub-hosted** runner | The self-hosted agent cannot reach `api.nuget.org` |
| `publishPackages` keeps `permissions: contents: read` | The publish job must not be able to create or mutate a release |
| The NuGet push stays gated on `releaseVerified == 'true'` | Only remotely verified bytes may be published |
| `artifacts/packages/.gitkeep` stays tracked | A missing folder yields `NU1301`, which cannot be suppressed |
| The `local-release` source stays in `src/NuGet.Config` | The nearer config wins and its `<clear />` applies; a root config would be ignored under `src/` |

---

**Document Version:** 1.3  
**Last Updated:** 2026-09-01  
**Next Review:** after `publishPackages` completes a NuGet.org push
