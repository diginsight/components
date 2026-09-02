---
title: "Update aicm CI/CD to restore Diginsight dependencies from GitHub Releases"
author: "Dario Airoldi"
date: "2026-09-02"
categories: [cicd, github-actions, self-hosted-runner, packaging, nuget, aicm]
---

# PLAN: bring the release-based restore model to aicm

**Date:** 2026-09-02
**Author:** Dario Airoldi
**Status:** ✅ Implemented and verified — aicm run [`33609541442`](https://github.com/darioa_microsoft/aicm/actions/runs/33609541442) is green end to end
**Repository:** `darioa_microsoft/aicm` (working copy `C:\dev\darioa\darioa_microsoft\aicm`)
**Upstreams:** `diginsight/telemetry`, `diginsight/smartcache`, `diginsight/components`
**Reference implementations:** `diginsight/components` (producer + consumer), `diginsight/smartdocs` (consumer)
**Related:** [CI/CD rework for NuGet.org independence](overview.md)

---

## 📋 Table of Contents

1. [🎯 Objective](#-objective)
2. [❓ Request assessment](#-request-assessment)
3. [🔍 Findings in the aicm repo](#-findings-in-the-aicm-repo)
4. [🧭 Design](#-design)
5. [🛠️ Implementation plan](#-implementation-plan)
6. [🚧 Independent blocker: the deployment is down](#-independent-blocker-the-deployment-is-down)
7. [✅ Acceptance criteria](#-acceptance-criteria)
8. [⚠️ Risks and mitigations](#-risks-and-mitigations)
9. [🚦 Verification gates](#-verification-gates)
10. [🔗 References](#-references)

---

## 🎯 Objective

Make `aicm` able to build against a **just-released** `Diginsight.*` version without waiting for
NuGet.org indexing or corporate-proxy mirroring — the same capability already delivered in
`components` and `smartdocs`.

| # | Goal | Scope for aicm |
|---|------|----------------|
| 1 | **Restore from releases** | Consume `diginsight/telemetry`, `diginsight/smartcache` **and** `diginsight/components` release assets |
| 2 | **No developer friction** | A plain `dotnet restore` / IDE build must keep working, with no wrapper script |
| 3 | **Fail closed** | A missing or corrupt upstream release stops the build; never silently fall back to a stale proxy copy |
| 4 | **Deterministic** | Exact version pins; `packages.lock.json` stays authoritative |

**Out of scope:** aicm publishes no NuGet packages, so the **producer** half of the model
(`Publish-Packages.ps1`, `package-manifest.json`, GitHub Releases) is **not** ported. Consumer only.

---

## ❓ Request assessment

**Is the request clear, unambiguous and fully actionable?** — Substantially yes. The goal, the
upstreams and the target repository are unambiguous, and the pattern is already proven twice.

Three points need a decision before implementation; none of them blocks starting:

| # | Decision needed | Outcome |
|---|-----------------|---------|
| D1 | The pins are **floating** (`3.8.*`, `3.8.*`, `1.*`). A floating range could not previously be mapped to a release tag. | ✅ **Resolved by extending the tooling instead of the pins.** `Download-PackageRelease.ps1` now resolves a floating pin to the newest matching **stable** release (prereleases are never selected). aicm therefore keeps `3.8.*` / `3.8.*` / `1.*` unchanged, and automatically follows new upstream releases. |
| D2 | Deployment was **broken for an unrelated reason** (see [🚧 blocker](#-independent-blocker-the-deployment-is-down)). | ✅ **Fixed** — `publicNetworkAccess` re-enabled; the SCM endpoint returns 200. |
| D3 | Should the deploy also move off basic auth, as smartdocs did? | ✅ **Done** — `azure/webapps-deploy@v3` replaced with the Entra/Kudu OneDeploy publish. Basic auth stays disabled. |

### ⚡ Consequence of the floating decision

Supporting floating pins removed two planned steps outright: aicm needs **no version change** (1.4)
and **no pin-driven lock-file regeneration**. The lock files still had to be refreshed, but for an
unrelated reason — they were already stale (see G3 below).

---

## 🔍 Findings in the aicm repo

Investigated: both workflows, `NuGet.Config`, `global.json`, `src/Directory.Build.props`,
`.gitignore`, the project/lock-file inventory, and the live Azure state of the deployment target.

### 🚩 F1 — aicm is a consumer only

~50 projects across 7 Bronze connectors, `Aicm.Common` and `Aicm.Silver`, all `net10.0`. The
pipeline publishes **8 self-contained WebJobs** into a single zip. Nothing is packed, so only
`Download-PackageRelease.ps1` is needed.

### 🚩 F2 — Version pins are floating

```xml
<DiginsightCoreVersion>3.8.*</DiginsightCoreVersion>
<DiginsightSmartcacheVersion>3.8.*</DiginsightSmartcacheVersion>
<DiginsightComponentsVersion>1.*</DiginsightComponentsVersion>
```

Must become exact. Note the property is spelled **`DiginsightSmartcacheVersion`** (lowercase `c`),
matching smartdocs but **differing** from components' `DiginsightSmartCacheVersion` — the upstream
inventory must use aicm's exact spelling.

### 🚩 F3 — `NuGet.Config` is at the repository root and has **no** nuget.org

```xml
<packageSources>
  <clear />
  <add key="azure-default" value="https://packagefeedproxy.microsoft.io/nuget/v3/index.json" protocolVersion="3" />
</packageSources>
```

Only the corporate proxy. The `local-release` source must be added **here** (root), so its relative
value is `artifacts/packages` — not `../artifacts/packages` as in components, whose config lives at
`src/`. The proxy key is already safely named (not `nuget.org`), so the `disabledPackageSources`
trap does not apply.

### 🚩 F4 — The default shell is **Windows PowerShell 5.1**, not `pwsh`

```yaml
defaults:
  run:
    shell: 'powershell -NoProfile -ExecutionPolicy Bypass -Command "..."'
```

Chosen deliberately because the runner's `NETWORK SERVICE` account has an effective `Restricted`
execution policy. But `Download-PackageRelease.ps1` declares `#requires -Version 7.0`, so **the
download step must opt into `shell: pwsh` explicitly**. This is the single most likely thing to be
missed when copying the smartdocs step verbatim.

### 🚩 F5 — `.gitignore` contains **both** documented traps

| Line | Rule | Problem |
|------|------|---------|
| 88 | `artifacts/` | Blanket exclusion of the parent — git **cannot** re-include a path underneath it |
| 230 | `**/[Pp]ackages/*` | Matches `artifacts/packages/`; any negation must come **after** it |

Both must be handled, exactly as in `components` and `smartdocs`.

### 🚩 F6 — `setup-dotnet` is already avoided

The workflow prepends `C:\Program Files\dotnet` and the Azure CLI directories to `GITHUB_PATH`
instead of using `actions/setup-dotnet`, with a comment naming the same `NETWORK SERVICE` /
`C:\Program Files\dotnet` write failure that broke `components`. **No change needed** — and no
`DOTNET_INSTALL_DIR` work is required here.

### 🚩 F7 — `global.json` pins a preview SDK

```json
{ "sdk": { "version": "11.0.100-preview.7.26381.103", "rollForward": "latestPatch", "allowPrerelease": true } }
```

At the repository root, so `dotnet` invoked from the root resolves it. Projects target `net10.0`,
which SDK 11 builds fine. The `DOTNET_VERSION: '10.0.x'` workflow env is **vestigial** — nothing
consumes it since `setup-dotnet` was removed. Leave the pin alone; it builds today.

### 🚩 F8 — ~50 lock files must be regenerated

`RestorePackagesWithLockFile=true` with ~50 `packages.lock.json`. Changing the pins from `3.8.*` to
an exact version regenerates all of them — a large but mechanical diff that must be committed
together with the props change.

### 🚩 F9 — The deploy step uses basic authentication

```yaml
- name: Deploy WebJobs to App Service
  uses: azure/webapps-deploy@v3
```

`azure/webapps-deploy` publishes through Kudu with basic credentials, which are **disabled** on the
target (`scm` and `ftp` `allow: false`). Even once the network blocker is cleared, this returns 401.

### 🚩 F10 — `alwaysOn` is `False`

The workflow header states *"Always On = On → so triggered WebJobs run on schedule"*, but the live
site reports `alwaysOn: False`. Pre-existing configuration drift, unrelated to this work, but it
means the scheduled WebJobs may not be firing.

---

## 🧭 Design

Identical in shape to the two working implementations, minus the producer half.

```text
src/Directory.Build.props            ← the ONLY place a version is written (exact pins)
        │  read via: dotnet msbuild -getProperty:
        ▼
eng/upstream-releases.json           ← repository → version-property inventory (3 upstreams)
        │
        ▼
eng/Download-PackageRelease.ps1      ← gh release download + verify + atomic swap
        │
        ▼
artifacts/packages/                  ← local folder source (tracked .gitkeep)
        │  declared in NuGet.Config as "local-release"
        ▼
dotnet restore / dotnet publish      ← plain restore, no wrapper, no --source
```

**Verification the script performs per upstream:** release manifest ↔ `SHA256SUMS` agreement, the
SHA-256 of every `.nupkg`, and each package's embedded `.nuspec` identity. Only when **all three**
upstreams validate is `artifacts/packages` replaced, in one atomic step.

**Why `local-release` is listed first:** it wins for the pinned versions, while everything else
continues to resolve from the corporate proxy unchanged.

---

## 🛠️ Implementation plan

### Phase 1 — dependency plumbing (the actual request)

| # | Step | Detail |
|---|------|--------|
| ✅ 1.1 | Copy `eng/Publish-Packages.ps1` | Verbatim from `components`. Not used for publishing, but `Download-PackageRelease.ps1` dot-sources it for `Get-FullPath`, `ConvertTo-NormalizedPackageVersion`, `Get-Sha256`, `Get-PackageArchiveMetadata`. Its command dispatch is guarded by `if ($MyInvocation.InvocationName -eq '.') { return }`, so dot-sourcing is safe. |
| ✅ 1.2 | Copy `eng/Download-PackageRelease.ps1` | Adapted `$VersionProjectPath` to `src/Aicm.Silver/Aicm.Silver.Webjob/Aicm.Silver.Webjob.csproj` and the closing hint to `src/Aicm.slnx`. |
| ✅ 1.3 | Create `eng/upstream-releases.json` | Three upstreams, using aicm's `DiginsightSmartcacheVersion` spelling (F2). |
| ⬜️ 1.4 | ~~Pin exact versions~~ | **Not needed** — floating pins are now resolved by the tooling (D1). |
| ✅ 1.5 | Add the `local-release` source | In the **root** `NuGet.Config`, value `artifacts/packages`, listed **before** the proxy. |
| ✅ 1.6 | Create `artifacts/packages/.gitkeep` | Load-bearing: a missing folder source yields `NU1301`, which `NoWarn` / `RestoreNoWarn` / `WarningsNotAsErrors` cannot suppress. |
| ✅ 1.7 | Fix `.gitignore` | Replaced the blanket `artifacts/` with the scoped set, **and** added `!/artifacts/packages/.gitkeep` *after* the `**/[Pp]ackages/*` rule (F5). |
| ✅ 1.8 | Regenerate lock files | 40 files. They were **already stale** — recording `3.7.1.13` / `1.0.0.104` while the props said `3.8.*` / `1.*`. Regenerated in **package mode** (`-p:*DirectImport=false`), never from the project-reference graph. |

### Phase 2 — workflow wiring

| # | Step | Detail |
|---|------|--------|
| ✅ 2.1 | Add the download step | Inserted **before** the first `dotnet publish` and after the PATH step, with `shell: pwsh` (F4) and `env: GH_TOKEN: ${{ github.token }}`. |
| ✅ 2.2 | Verify `gh` on the runner | Added `C:\Program Files\GitHub CLI` to the existing "Ensure dotnet and az on PATH" step. Confirmed working in CI. |
| ⬜️ 2.3 | Narrow the push trigger | **Deferred** — aicm has no separate docs pipeline, so there is no cross-firing to prevent today. |

### Phase 3 — deployment (see the blocker section)

| # | Step | Detail |
|---|------|--------|
| ✅ 3.1 | Restore App Service reachability | `publicNetworkAccess` set to `Enabled`; SCM returns 200. |
| ✅ 3.2 | Replace `azure/webapps-deploy@v3` | Entra/Kudu OneDeploy POST (`async=true` + status polling), so basic auth stays disabled (F9). |
| ⬜️ 3.3 | Re-check `alwaysOn` | **Still open** — see F10. The site hosts only WebJobs, and triggered WebJobs need Always On to fire on schedule. |

---

## 🚧 Independent blocker: the deployment is down

> ✅ **Resolved 2026-09-02.** `publicNetworkAccess` re-enabled and the deploy moved to the Entra
> token. Note that this App Service hosts **only WebJobs, no site**, so the main endpoint is not
> expected to serve content — only the SCM endpoint matters, and it now returns 200.

**This was not caused by the dependency work, and it blocked verification of it.**

The most recent runs (`33494081817`, `33493733609`) fail at the last step, after all 11 build steps
succeed:

```text
##[error]Failed to deploy web package using OneDeploy to App Service.
Ip Forbidden (CODE: 403)
```

Live state of `aicm-testmc-app-itn-01` (resource group `aicm-testmc-rg-itn-01`):

| Property | Value |
|----------|-------|
| `publicNetworkAccess` | **Disabled** |
| `scm` / `ftp` basic auth | **`allow: false`** |
| `alwaysOn` | `False` |
| `https://aicm-testmc-app-itn-01.azurewebsites.net/` | **HTTP 403** |
| `https://aicm-testmc-app-itn-01.scm.azurewebsites.net/` | **HTTP 403** |

This is the **same** subscription-wide condition that took the two smartdocs sites offline on
2026-09-01, with no corresponding entry in the activity log. Remediation is identical and already
proven:

```powershell
az webapp update -g aicm-testmc-rg-itn-01 -n aicm-testmc-app-itn-01 --set publicNetworkAccess=Enabled
```

then replace the basic-auth deploy with the Entra token POST (step 3.2), which needs no storage
staging, no SAS and no additional RBAC.

---

## ✅ Acceptance criteria

- [x] `./eng/Download-PackageRelease.ps1` fills `artifacts/packages` with verified assets from **all three** upstreams
- [x] A plain `dotnet restore` resolves the pinned `Diginsight.*` versions from `local-release`
- [x] `artifacts/packages/.gitkeep` is tracked; the downloaded `.nupkg` files are **not**
- [x] The full solution builds with 0 errors after the pin change
- [x] CI runs the download step successfully on the self-hosted runner (proves `pwsh` + `gh` + `GH_TOKEN`)
- [x] A floating pin resolves to the newest matching stable release, and a non-matching pattern fails closed
- [x] Deployment succeeds and the WebJobs are present under `App_Data/jobs/triggered/`
- [ ] `alwaysOn` enabled so the triggered WebJobs fire on schedule (F10, still open)

---

## ⚠️ Risks and mitigations

| # | Risk | Mitigation |
|---|------|------------|
| R1 | The download step runs under Windows PowerShell 5.1 and fails on `#requires -Version 7.0` | Set `shell: pwsh` on that step only (F4) |
| R2 | `.gitkeep` silently ignored by the later `**/[Pp]ackages/*` rule → `NU1301` for everyone | Verify with `git check-ignore -v` and `git status --porcelain -uall -- artifacts` (F5) |
| R3 | The ~50-file lock diff hides a real resolution change | Regenerate in one commit, review the `Diginsight.*` entries specifically |
| R4 | Pinning to a version not yet on NuGet.org makes the download step load-bearing | D1 picks versions already published; the step stays a fast path |
| R5 | Local builds with `Directory.build.props.user` direct-import rewrite lock files | Never commit those diffs; CI must restore the committed lock files |
| R6 | Central governance re-disables `publicNetworkAccess` | Track separately; the durable answer is the private-endpoint + `privatelink.azurewebsites.net` pattern already used by storage, Key Vault, SQL and Cosmos in this subscription |

---

## 🚦 Verification gates

| Gate | Command / check | Result |
|------|-----------------|--------|
| G1 — tooling | `pwsh -File ./eng/Download-PackageRelease.ps1` | ✅ 3 upstreams, 25 packages; `1.*` resolved to `1.0.0.111` |
| G2 — gitignore | `git status --porcelain -uall -- artifacts` | ✅ lists only `artifacts/packages/.gitkeep` (25 `.nupkg` ignored) |
| G3 — restore | package-mode restore (`-p:*DirectImport=false`) | ✅ succeeds; 40 lock files refreshed from stale `3.7.1.13`/`1.0.0.104` to `3.8.0.1`/`1.0.0.111` |
| G4 — build | `dotnet build src/Aicm.slnx -c Release` in package mode | ✅ **0 errors**, 17 warnings |
| G5 — CI | Download step on the self-hosted runner | ✅ step 4 succeeded |
| G6 — deploy | Workflow reaches and passes the deploy step | ✅ run `33609541442` green; Kudu `status=4`, all 8 triggered WebJobs present |
| G7 — regression | `eng/tests/*.Tests.ps1` in components after the floating change | ✅ 18 + 8 tests pass |

---

## 🔗 References

- [CI/CD rework for NuGet.org independence, and the four defects that blocked its first runs](overview.md) — the components implementation and the incident that shaped it
- [Fix Diginsight Components CI/CD for GitHub Release based package integration](../../202608/20260831.01-restore-by-release/01-fix-cicd-for-release-integration.plan.md) — the original plan, including findings F1–F13
- [eng/README.md](../../../../../eng/README.md) — authoritative description of the release tooling
- [eng/Download-PackageRelease.ps1](../../../../../eng/Download-PackageRelease.ps1) — the consumer script to port
- [eng/upstream-releases.json](../../../../../eng/upstream-releases.json) — inventory format

---

**Document Version:** 2.0
**Last Updated:** 2026-09-02
**Next Review:** after `alwaysOn` is decided (F10)
