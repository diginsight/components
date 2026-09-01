---
title: "Fix Diginsight Components CI/CD for GitHub Release based package integration"
author: "Dario Airoldi"
date: "2026-08-31"
categories: [cicd, packaging, nuget, github-actions]
---

# PLAN: restore from releases, publish as releases

**Date:** 2026-08-31
**Author:** Dario Airoldi
**Status:** ✅ Implemented and verified locally on top of `df25628`. ⏳ Six items remain, all of which
can only be exercised by an actual CI run.
**Repository:** `diginsight/components` (working copy `C:\dev\darioa\Diginsight\components.02`)
**Upstreams:** `diginsight/telemetry` **and** `diginsight/smartcache`
**Authoritative pipeline:** `.github/workflows/v2_99.Package.CICD.yml`
**Related issue:** [SmartCache — Dependent package builds blocked by NuGet feed propagation latency](../../../../../../smartcache.02/src/docs/90.%20Issues/202608/20260831.01-howtobuild-latestpackage/00.%20ISSUE%20Overview.md)

---

## 📋 Table of Contents

1. [🎯 Objective](#-objective)
2. [🔒 Locked decisions](#-locked-decisions)
3. [📌 Upstream contract recap](#-upstream-contract-recap)
4. [🔍 Findings in the components repo](#-findings-in-the-components-repo)
5. [🧭 Design](#-design)
6. [🛠️ Implementation plan](#-implementation-plan)
7. [🔧 Upstream API migration](#-upstream-api-migration)
8. [✅ Acceptance criteria](#-acceptance-criteria)
9. [⚠️ Risks and mitigations](#-risks-and-mitigations)
10. [🚦 Verification gates](#-verification-gates)
11. [📖 References](#-references)

---

## 🎯 Objective

| # | Goal | Role of components |
|---|------|--------------------|
| 1 | **Restore from releases** — build against pinned `Diginsight.*` versions even when they have not propagated to the corporate NuGet proxy | **Consumer** of `diginsight/telemetry` **and** `diginsight/smartcache` releases |
| 2 | **Publish as releases** — attach the exact `.nupkg`/`.snupkg` bytes to a durable, verified GitHub Release **before** pushing to NuGet.org | **Producer** for consumers of `Diginsight.Components.*` |
| 3 | **Fully automatic** — no manual tagging, no manual version bump, no manual release step | Push to `main` builds, releases, and publishes |

Out of scope: runtime behaviour of any component, and the sample projects under `src/Samples`.

---

## 🔒 Locked decisions

| # | Question | **Decision** |
|---|----------|--------------|
| 1 | Authoritative pipeline | **`v2_99.Package.CICD.yml`** — run-number versioning, automatic on push to `main`. The release contract is bolted onto it. |
| 2 | `v3.yml` | **Delete it.** Components does not use tag-triggered releases. |
| 3 | Version | **Computed automatically**: `VERSION_PREFIX` (default `0.8.0`) `.` (`github.run_number` + `BUILD_NUMBER_OFFSET`) → e.g. `0.8.0.123`. No manual tag push. |
| 4 | `DiginsightCoreVersion` | **`3.8.0.1`** |
| 5 | `DiginsightSmartCacheVersion` | **`3.8.0.1`** — kept and now actively used |
| 6 | NuGet auth | **`secrets.NUGET_API_KEY_V3`** (SmartCache model). Trusted publishing deferred. |
| 7 | SmartCache consumption | **Yes** — SmartCache releases must be downloaded alongside telemetry releases |
| 8 | `src/Samples` | **Unchanged** for now — excluded from the release pipeline (already absent from `Diginsight.Components.Build.slnx`) |

### ⚡ Consequence of decision 1: the tag is an *output*, not an *input*

Upstream, the git tag is the human-authored trigger. Here the version is computed, so the pipeline
**creates** the tag as part of publishing the release:

```text
push to main
  → v2_01 computes assemblyVersion = 0.8.0.123  and  sourceTag = v0.8.0.123
  → v2_03 builds once, stages, creates release v0.8.0.123 at github.sha, verifies it
  → v2_04 pushes the same bytes to NuGet.org
```

`gh release create <tag> --target $GITHUB_SHA` creates the git tag itself, so nothing has to be
pushed by hand. **`--verify-tag` must NOT be used** (unlike telemetry/smartcache, where the tag
already exists). Everything else in the contract — `v`-prefixed four-part tag, normalisation,
`Resolve-ReleaseTag` candidate probing — works unchanged.

### ⚡ Consequence of decision 7: two upstreams into one local source

This is the single most important new technical constraint, and it **breaks the upstream script as
written** — see [🔍 Findings in the components repo](#-findings-in-the-components-repo), F15.

---

## 📌 Upstream contract recap

Telemetry and SmartCache split distribution into two channels fed by **one** build:

```text
                              ┌→ GitHub Release assets   (immediate, maintainer-to-maintainer)
version → build/pack once ────┤
                              └→ NuGet.org               (eventual, consumer-facing)
```

- **Producer** — `eng/Publish-Packages.ps1` (`ResolveVersion`, `Stage`, `Validate`, `Compare`,
  `PublishNuGet`) + `eng/package-manifest.json` (tracked package inventory) + a workflow where the
  NuGet push job `needs:` the release job and is gated on `release-verified == 'true'`.
- **Release assets** — every `.nupkg`, every `.snupkg`, `SHA256SUMS`, and `release-manifest.json`
  (schema 1). Individual files, never an archive.
- **Consumer** — `eng/Download-PackageRelease.ps1` resolves the release matching the pinned MSBuild
  version property, verifies manifest + `SHA256SUMS` + embedded `.nuspec` identity, then publishes
  into `artifacts/packages`, a **committed** package source.
- **Load-bearing marker** — `artifacts/packages/.gitkeep`. A missing folder yields `NU1301`, which is
  **not suppressible** by `NoWarn` / `RestoreNoWarn` / `WarningsNotAsErrors`.
- **Build-once** — the same staged bytes go to the release and to NuGet.org; nothing is repacked.

---

## 🔍 Findings in the components repo

Investigated: `.github/workflows/{v3,v2_99,v2_00,v2_01,v2_03,v2_04}.yml`, `src/NuGet.Config`,
`src/Directory.Build.props`, `.gitignore`, both `.slnx` files, all 8 `*.csproj`, and the smartcache
and telemetry `eng/` tooling.

### 🚩 F1 — The build cannot move to `ubuntu-latest`

`Diginsight.Components.Presentation` targets `net8.0-windows;net9.0-windows;net10.0-windows` with
`<UseWPF>true</UseWPF>`. The build job must stay on the **self-hosted Windows** runner.

### 🚩 F2 — The self-hosted runner cannot reach `api.nuget.org`

Stated explicitly in `v2_04.PublishPackages.yml`. The existing two-runner split (build on
self-hosted, push on `ubuntu-latest`) is correct and must be preserved; only its semantics change.
GitHub API access from the self-hosted runner is already proven — `v2_03` creates a release from it.

### 🚩 F3 — `NuGet.Config` lives at `src/`, not at the repository root

Telemetry and SmartCache both have it at the root; components has `src/NuGet.Config` (plus an
unrelated `scripts/Config/NuGet.Config`). Measured NuGet behaviour #7 upstream: *the nearer config
wins, and its `<clear />` applies*. A root config would be **silently ignored** for everything under
`src/`. The `local-release` source must go in `src/NuGet.Config` as `../artifacts/packages`.

### 🚩 F4 — The upstream versions are pinned to floating ranges

```xml
<DiginsightCoreVersion>3.7.*</DiginsightCoreVersion>
<DiginsightSmartCacheVersion>3.7.*</DiginsightSmartCacheVersion>
```

A floating range cannot be mapped to a release tag and defeats `--locked-mode` determinism. Both
must become exact `3.8.0.1`.

### 🚩 F5 — `.gitignore` contains both documented traps

| Line | Rule | Problem |
|------|------|---------|
| 62 | `artifacts/` | Blanket exclusion of the parent — git **cannot** re-include a path underneath it |
| 190 | `**/[Pp]ackages/*` | Matches `artifacts/packages/`; any negation must come **after** it |

### 🚩 F6 — `v3.yml` is dead code

It builds `src/Diginsight.slnx` (does not exist here), has no `permissions:` block, and pushes to the
**corporate proxy** rather than NuGet.org. **Deleted** per decision 2.

### 🚩 F7 — Version scheme is compatible with the contract

`0.8.0.<run+offset>` is a valid four-part NuGet version. `ConvertTo-NormalizedPackageVersion` drops a
**zero** fourth component only, so `v0.8.0.0` → `0.8.0` and `v0.8.0.123` → `0.8.0.123`;
`Resolve-ReleaseTag` probes both spellings. No change is needed to the normalisation logic.

### 🚩 F8 — `v2_03` rewrites `<Version>` in every `.csproj` before building

```powershell
$csprojFiles | ForEach-Object { (Get-Content $_.FullName) -replace '<Version>.*<\/Version>', "<Version>$version</Version>" | Set-Content $_.FullName }
```

Mutates the working tree (incompatible with the *verify working tree unchanged* invariant), rewrites
file encodings as a side effect, and also hits `src/Samples` and any non-packable project. Replace
with `-p:Version=` on `dotnet build`.

### 🚩 F9 — `v2_04` deletes the release after publishing

```yaml
- name: Remove transport Release
  run: gh release delete "$RELEASE_TAG" --repo "$GITHUB_REPOSITORY" --yes --cleanup-tag
```

The release exists today purely as internal transport and is destroyed on success. The new model
needs the exact opposite: a **durable, immutable, verified** release. This step is removed, and the
tag naming changes from `packages-v<version>` to `v<version>`.

### 🚩 F10 — Contradictory restore flags in `v2_03`

`--locked-mode --force --force-evaluate --interactive` — `--force-evaluate` can rewrite the lock file
(defeating `--locked-mode`) and `--interactive` can block a headless runner on a credential prompt.

### 🚩 F11 — Solution file hygiene

`src/Diginsight.Components.slnx` references an out-of-repository file
(`../../telemetry_samples/.filenesting.json`). `src/Diginsight.Components.Build.slnx` is clean and
contains exactly the 8 packable projects. CI already uses `Build.slnx`; keep it.

### 🚩 F12 — Package inventory is 8 projects

`Diginsight.Components`, `.Abstractions`, `.Azure`, `.Azure.Abstractions`, `.Configuration`,
`.Configuration.Abstractions`, `.Presentation`, `.Presentation.Abstractions`.
`IncludeSymbols` + `SymbolPackageFormat=snupkg` are set repo-wide → `symbolsRequired: true` for all 8.

### 🚩 F13 — Consumed upstream packages

Six of telemetry's eleven packages are referenced today: `Diginsight.Core`, `.Diagnostics`,
`.Diagnostics.AspNetCore`, `.Diagnostics.AspNetCore.OpenTelemetry`, `.Diagnostics.Log4Net`,
`.Diagnostics.OpenTelemetry`. **Zero** SmartCache packages are referenced yet, although
`DiginsightSmartCacheVersion` exists. Per decision 7 the SmartCache release is downloaded regardless;
`PackageReference`s can be added later without further pipeline changes.

### 🚩 F14 — Toolchain prerequisites on the self-hosted runner (unverified)

The upstream scripts declare `#requires -Version 7.0` and shell out to `gh`. `v2_03` calls **no**
`setup-dotnet` at all, and `v2_00` installs only `9.0.x` while projects target `net10.0`. There is
also **no `src/global.json`** to pin the SDK.

### 🚩 F15 — 🔴 BLOCKING: `Download-PackageRelease.ps1` wipes the destination

```powershell
function Publish-LocalSource {
    $null = New-Item -ItemType Directory -Path $DestinationPath -Force
    foreach ($existing in @(Get-ChildItem -LiteralPath $DestinationPath -File)) {
        if ($existing.Name -ne $KeepFileName) {
            Remove-Item -LiteralPath $existing.FullName -Force   # <-- deletes everything
        }
    }
    ...
}
```

SmartCache only ever had **one** upstream, so a full replace was correct there. Calling the script
twice in components — once for telemetry, once for smartcache — would **erase the telemetry packages
when the smartcache run publishes**. The script must be made multi-upstream aware before decision 7
can be implemented. Design in [🧭 Design](#-design).

### 🚩 F16 — Path filters would not trigger on the changes this plan makes

`v2_99` triggers only on `src/Diginsight.Components*/**`. Changes to `src/Directory.Build.props`
(the version pin), `src/NuGet.Config`, `eng/**`, or the workflows themselves would **not** start a
build. Repinning an upstream version would silently not release.

### 🚩 F17 — Self-hosted workspace is shared and `v2_00` writes into it

`v2_00.InstallActions.yml` writes `01.GITHUB_CONTEXT.log` … `05.STRATEGY_CONTEXT.log` into the
working directory. `actions/checkout@v4` cleans by default, so these should not survive into `v2_03`
— but the *verify working tree unchanged* step must be introduced with this in mind, and the
self-hosted runner's shared workspace is a general contamination risk.

### 🚩 F18 — Every push to `main` publishes to NuGet.org

This is the **current** behaviour of `v2_99` and is preserved by decision 1. Worth stating
explicitly: with automatic versioning there is no "release moment"; every merged change ships a new
version to both channels.

### 🚩 F19 — 🔴 A source named `nuget.org` is disabled by the user-level config

Found while reapplying this work on top of `df25628`, which renamed the proxy source in
`src/NuGet.Config` from `azure-default` to `nuget.org`. The user-level `NuGet.Config` contains:

```xml
<disabledPackageSources>
  <add key="nuget.org" value="true" />
</disabledPackageSources>
```

`disabledPackageSources` matches **by key** and is **not** reset by `<clear />`, which only clears
`packageSources`. So the renamed source inherited the disable:

```text
1. local-release [Enabled]
2. nuget.org     [Disabled]   <- every third-party package unresolvable
```

Symptom: `NU1101` for `Newtonsoft.Json`, `Scrutor`, `RestSharp`, `Microsoft.SourceLink.GitHub` and
others, with the sources listed as only `library-packs, local-release`. It also silently regenerated
eight lock files with roughly 7 500 lines removed.

**Rule: never name a repository package source `nuget.org` unless it really is nuget.org.** The key
is now `azure-default` again, matching the root `NuGet.Config` added by the same commit, and the
constraint is recorded as a comment in the file.

---

## 🧭 Design

### Overall shape

```text
v2_99.Package.CICD.yml            (push to main / PR / workflow_dispatch)
│
├─ installActions        self-hosted   permissions: contents: read
├─ getCompositeVariables self-hosted   permissions: {}
│     outputs: assemblyVersion=0.8.0.123, sourceTag=v0.8.0.123
├─ buildPackages         self-hosted   permissions: contents: write     ← WPF needs Windows
│     download upstream releases (telemetry + smartcache)
│     restore --locked-mode → build -p:Version → Stage → upload-artifact
│     create release v0.8.0.123 --target $GITHUB_SHA → re-download → Validate + Compare
│     outputs: release-verified=true
└─ publishPackages       ubuntu-latest permissions: contents: read      ← api.nuget.org reachable
      if: github.event_name != 'pull_request' && needs.buildPackages.outputs.release-verified == 'true'
      download-artifact → Validate → PublishNuGet
```

`permissions: {}` at the top level of every workflow file; each job re-grants only what it needs.
The publish job cannot create or modify a release, and the build job never sees `NUGET_API_KEY_V3`.

### Multi-upstream download (resolves F15)

**Rejected:** a `-NoClean` / `-Append` switch. A failure while downloading the second upstream would
leave a half-populated local source — violating the *fail closed, never fall back silently*
invariant.

**Selected:** stage **all** upstreams into a temporary directory, validate each against **its own**
repository/tag/version, then replace `artifacts/packages` **once**, atomically.

`eng/upstream-releases.json` — the repository-to-property mapping only:

```json
{
  "schemaVersion": 1,
  "upstreams": [
    { "repository": "diginsight/telemetry",  "versionProperty": "DiginsightCoreVersion" },
    { "repository": "diginsight/smartcache", "versionProperty": "DiginsightSmartCacheVersion" }
  ]
}
```

> This does **not** repeat the upstream lesson *"do not encode a value in two places"*. No version
> appears here — the version remains solely in `src/Directory.Build.props` and is read via
> `dotnet msbuild -getProperty:`. Only the mapping lives in this file, and it has no other home.

Script changes to `eng/Download-PackageRelease.ps1`:

| Change | Reason |
|--------|--------|
| `-RepositoryUrl` accepts an array; with no argument, read `eng/upstream-releases.json` | One command bootstraps everything: `./eng/Download-PackageRelease.ps1` |
| `-VersionProperty` parameter (default `DiginsightCoreVersion`) | Telemetry and SmartCache pin through different properties |
| Per-upstream staging subfolder + per-upstream `Test-ReleaseDownload` | Each release is validated against its own `repository`, `sourceTag`, and `packageVersion` |
| Cross-upstream duplicate filename check, failing closed | Two upstreams must never claim the same `.nupkg` name |
| `Publish-LocalSource` called **once**, after all upstreams validate | Atomic; never leaves a partial source |
| `-Version` repin extended to target a named upstream | Repinning must write to the right MSBuild property |

Everything else — SHA-256 against both `release-manifest.json` and `SHA256SUMS`, embedded `.nuspec`
identity, `.gitkeep` preservation, `NU1301` avoidance — is unchanged.

### Version and tag

| Item | Value |
|------|-------|
| `assemblyVersion` | `${VERSION_PREFIX}.${github.run_number + BUILD_NUMBER_OFFSET}` (e.g. `0.8.0.123`) |
| `sourceTag` | `v${assemblyVersion}` (e.g. `v0.8.0.123`) |
| Release creation | `gh release create $sourceTag --target $GITHUB_SHA` — **no** `--verify-tag` |
| Applied to the build | `dotnet build -c Release -p:Version=$assemblyVersion` — **no** csproj rewriting |

### Local package source

| Item | Value |
|------|-------|
| On-disk location | `artifacts/packages/` (repository root) |
| Declared in | `src/NuGet.Config` as `<add key="local-release" value="../artifacts/packages" />`, **before** `azure-default` |
| Marker | `artifacts/packages/.gitkeep` (load-bearing) |
| Contents after bootstrap | 11 telemetry `.nupkg` + 6 smartcache `.nupkg`; symbol packages and metadata files are not copied into the source |

---

## 🛠️ Implementation plan

### Phase 0 — Prerequisite verification (blocking)

- ✅ **0.1** On the self-hosted Windows runner confirm: `pwsh` **7+** on `PATH`; `gh` on `PATH`;
      .NET SDK **10.x** installed (projects target `net10.0` / `net10.0-windows`).
      *Verified on the development machine: pwsh 7.6.5, gh 2.88.1, SDK 10.0.111 / 10.0.400. Still to
      be confirmed on the runner itself.*
- ✅ **0.2** Confirm `gh release download` works from the self-hosted runner against the public
      `diginsight/telemetry` and `diginsight/smartcache` repositories (upload is already proven).
      *Verified locally; 17 packages downloaded from the two repositories.*
- ✅ **0.3** Confirm `diginsight/telemetry` `v3.8.0.1` and `diginsight/smartcache` `v3.8.0.1` both
      exist and carry `release-manifest.json` + `SHA256SUMS`. *24 and 14 assets respectively.*

### Phase 1 — Consumer side: restore from telemetry **and** smartcache releases

- ✅ **1.1** Create `artifacts/packages/.gitkeep`, annotated: deleting it breaks **every** restore
      in the repository with `NU1301`, for everyone, always.
- ✅ **1.2** Fix `.gitignore` (F5). Replace line 62 `artifacts/` with:
      ```gitignore
      /artifacts/*
      !/artifacts/packages/
      /artifacts/packages/*
      !/artifacts/packages/.gitkeep
      ```
      and add `!/artifacts/packages/.gitkeep` **after** line 190 `**/[Pp]ackages/*`.
      Verify with `git check-ignore -v artifacts/packages/.gitkeep` (must report *not ignored*).
      *Verified: `git status -uall artifacts` lists only `artifacts/packages/.gitkeep`.*
- ✅ **1.3** Add to `src/NuGet.Config`, **before** `azure-default`:
      ```xml
      <!-- Populated by eng/Download-PackageRelease.ps1. Must exist even when empty, or restore fails with NU1301. -->
      <add key="local-release" value="../artifacts/packages" />
      ```
      Confirm the `../` form resolves from `src/` — this is new relative to both upstream repos.
      *Verified with a cold `NUGET_PACKAGES`: all 9 `Diginsight.*` packages resolved from
      `components.02/artifacts/packages`.*
- ✅ **1.4** Copy `eng/Publish-Packages.ps1` from smartcache verbatim. It is dot-sourced by the
      download script for `Get-FullPath`, `ConvertTo-NormalizedPackageVersion`, `Get-Sha256`, and
      `Get-PackageArchiveMetadata`.
- ✅ **1.5** Copy `eng/Download-PackageRelease.ps1` and apply the multi-upstream design:
      array `-RepositoryUrl`, `-VersionProperty`, `eng/upstream-releases.json` fallback, per-upstream
      staging and validation, cross-upstream duplicate detection, single atomic `Publish-LocalSource`.
      Also update `$VersionProjectPath` to `src/Diginsight.Components/Diginsight.Components.csproj`
      and the final hint to `dotnet restore src/Diginsight.Components.Build.slnx --force-evaluate`.
- ✅ **1.6** Create `eng/upstream-releases.json` with the two-entry mapping.
- ✅ **1.7** Copy and adapt `eng/tests/Download-PackageRelease.Tests.ps1` and
      `eng/tests/Publish-Packages.Tests.ps1`. **Add new cases** for the multi-upstream behaviour:
      - both upstreams present after one invocation;
      - a failure in the second upstream leaves `artifacts/packages` untouched;
      - duplicate `.nupkg` filename across upstreams fails;
      - `.gitkeep` survives.

      *18 download tests and 8 publish tests pass.*
- ✅ **1.8** Pin in `src/Directory.Build.props`:
      ```xml
      <DiginsightCoreVersion>3.8.0.1</DiginsightCoreVersion>
      <DiginsightSmartCacheVersion>3.8.0.1</DiginsightSmartCacheVersion>
      ```
      Commit this **separately** and build locally first — the exact pin may surface API changes that
      `3.7.*` was hiding. **It did** — see [🔧 Upstream API migration](#-upstream-api-migration).
- ✅ **1.9** Run `./eng/Download-PackageRelease.ps1` (no arguments), then regenerate all 8
      `packages.lock.json` with `--force-evaluate` **against the downloaded release bytes** — never
      against a locally built telemetry/smartcache, which is the documented `NU1403` trap.
      *Verified: a cold `--locked-mode` restore of all 8 projects rewrites zero lock files.*
- ✅ **1.10** Verify a **plain** `dotnet restore` and a **Visual Studio** build both succeed with no
      extra arguments and no IDE configuration. The IDE is a first-class consumer.
      *`dotnet restore src/Diginsight.Components.slnx` (the IDE solution) succeeds with no extra
      arguments. Visual Studio itself was not launched.*

### Phase 2 — Producer side: automatic verified releases from `v2_99`

- ✅ **2.1** Delete `.github/workflows/v3.yml` (F6, decision 2).
- ✅ **2.2** Author `eng/package-manifest.json`:
      ```json
      {
        "schemaVersion": 1,
        "repository": "diginsight/components",
        "solution": "src/Diginsight.Components.Build.slnx",
        "stagingPath": "artifacts/release",
        "packages": [ /* the 8 ids from F12, each symbolsRequired: true */ ]
      }
      ```
- ✅ **2.3** `v2_01.GetCompositeVariables.yml`:
      - add output `sourceTag` = `v${assemblyVersion}`;
      - replace the deprecated `::set-output` calls with `$GITHUB_OUTPUT`;
      - set `permissions: {}` (it needs none).
- ✅ **2.4** `v2_03.BuildPackages.yml` — becomes **Build and Release**:
      - `permissions: contents: write`; add `outputs:` for `sourceTag`, `artifact-name`,
        `release-verified`;
      - add `actions/setup-dotnet@v4` with `8.x`/`9.x`/`10.x` (F14);
      - **remove** the csproj `<Version>` rewrite (F8);
      - fix restore to `dotnet restore $env:SOLUTION_SLN --locked-mode -v minimal` (F10);
      - insert, in order: run both `eng/tests/*.ps1` suites → remove previous `**/bin/Release`
        outputs → **`./eng/Download-PackageRelease.ps1`** → restore → *verify working tree unchanged*
        → `dotnet build -c Release --no-restore -p:Version=$assemblyVersion` →
        `Publish-Packages.ps1 -Command Stage -Tag $sourceTag -SourceRoot ./src -StagePath artifacts/release/$sourceTag`
        → `upload-artifact` → create/complete/verify release → set `release-verified`;
      - release step: `gh release create $sourceTag --target $GITHUB_SHA --title $sourceTag`
        (**no** `--verify-tag`), then re-download all assets and run
        `Publish-Packages.ps1 -Command Validate` and `-Command Compare`;
      - keep `if: github.event_name != 'pull_request'` on the release step so PRs still build and
        validate without publishing;
      - drop the separate Debug build, or keep it only as a PR-time check — it produces no packages.
- ✅ **2.5** `v2_04.PublishPackages.yml`:
      - `permissions: contents: read`;
      - consume the **artifact** (not the release) via `actions/download-artifact@v4`;
      - `Publish-Packages.ps1 -Command Validate` then `-Command PublishNuGet` against
        `https://api.nuget.org/v3/index.json` using `secrets.NUGET_API_KEY_V3`;
      - **delete** the `Remove transport Release` step (F9).
- ✅ **2.6** `v2_99.Package.CICD.yml`:
      - `permissions: {}` at the top level;
      - gate `publishPackages` with
        `if: github.event_name != 'pull_request' && needs.buildPackages.outputs.release-verified == 'true'`;
      - extend the `paths:` filters (F16) to include `src/Directory.Build.props`,
        `src/Directory.Build.targets`, `src/NuGet.Config`, `src/*.slnx`, `eng/**`,
        `.github/workflows/**`;
      - add a `workflow_dispatch` dry-run input that builds, stages, and validates without creating
        a release or publishing.

### Phase 3 — Documentation and validation

- ✅ **3.1** Add `eng/README.md`: the maintainer runbook — bootstrap command, upstream repinning,
      dry runs, rerun/recovery semantics, and the `.gitkeep` warning.
- ✅ **3.2** Update the repository `README.md` with the one-command bootstrap
      (`./eng/Download-PackageRelease.ps1` then `dotnet restore`).
- ✅ **3.3** Dry run: confirm 8 `.nupkg` + 8 `.snupkg` + `SHA256SUMS` +
      `release-manifest.json` stage and validate, with no release created.
      *Run locally end to end: tests → bootstrap → locked restore → clean-tree check → build → stage
      → validate → compare. 18 assets validated, 18 compared byte for byte. The `workflow_dispatch`
      dry run still has to be triggered on the runner.*
- ⏳ **3.4** Real run on a merge to `main`: confirm release `v0.8.0.<n>` with 18 assets, then
      confirm the NuGet push. *Requires a push; not done.*
- ✅ **3.5** Prove the goal: verify the corporate proxy does **not** yet have a pinned
      `Diginsight.*` version, and that the components build succeeds anyway.
      *Proxy has neither `Diginsight.Core 3.8.0.1` nor `Diginsight.SmartCache 3.8.0.1`. With an
      isolated package cache: empty local source → `NU1102` ("Found 135 version(s) in azure-default
      [ Nearest version: 3.8.0-alpha.5 ]", the exact failure from the upstream issue); populated
      local source → restore and build succeed.*
- ⏳ **3.6** Write the outcome into `overview.md` in this folder (currently empty).

---

## 🔧 Upstream API migration

Gate G3 failed exactly as [R10](#-risks-and-mitigations) predicted: moving from floating `3.7.*` to
exact `3.8.0.1` surfaced a breaking change that the floating range had been hiding. Diginsight.Core
3.8.0.1 removed its pre-keyed-DI named-service shim:

| Removed in 3.8.0.1 | Confirmed by |
|--------------------|--------------|
| `IServiceProvider.GetNamedService<T>(name)` | Present in `diginsight.core` up to 3.7.1.13, absent from every 3.8.0.1 assembly |
| `IServiceCollection.AddNamedSingleton<T,TImpl>(name, factory)` | Same |
| `NamedOptionsMonitor<T>` | Same |
| `OptionsBasedMetricRecording*Options.MetricName` | Replaced by named options resolved from `instrument.Name` |

The replacement is a **simplification**, not a shim: since 3.8.0 `OptionsBasedMetricRecordingFilter`
and `OptionsBasedMetricRecordingEnricher` look up their options with `.Get(instrument.Name)`, so one
non-keyed registration serves every metric.

| File | Change |
|------|--------|
| `src/Diginsight.Components.Azure/Metrics/QueryCostMetricRecorder.cs` | Dropped the `IServiceProvider` named lookup for optional constructor injection of `IMetricRecordingFilter` / `IMetricRecordingEnricher`, matching telemetry's own `SpanDurationMetricRecorder`. Also removed the then-unused `openTelemetryOptionsMonitor` parameter. |
| `src/Diginsight.Components.Configuration/Hosting/Extensions/ObservabilityExtensions.cs` | Replaced eight `AddNamedSingleton` registrations with one `TryAddSingleton` per interface; the per-metric `services.Configure<...>(metricName, ...)` named options are unchanged, and now mutate the collections instead of assigning them. |
| `src/Diginsight.Components.Configuration/Hosting/MetricRecordingConfigurationEntries.cs` | New. Two small internal types carrying `MetricName`, so the `MetricSpecificSpanMeasuredActivityNames` and `MetricSpecificTags` **appsettings schema is unchanged** even though the upstream options no longer expose `MetricName`. |

Net effect: the `appsettings` contract and the observable behaviour are preserved; roughly 25 lines
of registration code were deleted.

---

## ✅ Acceptance criteria

**Consumer**

- ✅ A single `./eng/Download-PackageRelease.ps1` with **no arguments** populates
      `artifacts/packages` with **both** the telemetry and the smartcache release packages.
      *17 packages: 11 telemetry + 6 smartcache.*
- ✅ A **plain** `dotnet restore` then succeeds while the pinned versions are absent from the
      corporate proxy; third-party packages still come from the proxy.
- ✅ A Visual Studio / VS Code build succeeds with no `--source` arguments and no IDE configuration.
      *`dotnet restore src/Diginsight.Components.slnx` succeeds unaided; VS itself not launched.*
- ⏳ A fresh clone with an empty `artifacts/packages` restores normally when the pinned versions
      *are* on the proxy. *Not verifiable yet — the proxy has no 3.8.0.1.*
- ✅ A failure on the **second** upstream leaves `artifacts/packages` byte-identical to its prior
      state — no partial population.
- ✅ Tampered bytes, wrong repository, wrong version, missing or undeclared assets each **fail
      closed** before anything reaches the local source.
- ✅ CI restore runs in `--locked-mode` and the working tree is unchanged afterwards.
      *Cold locked restore of all 8 projects rewrote zero lock files.*

**Producer**

- ⏳ A push to `main` automatically produces a GitHub Release `v<VERSION_PREFIX>.<n>` containing
      8 `.nupkg`, 8 `.snupkg`, `SHA256SUMS`, and `release-manifest.json` — with **no manual tagging**.
      *Staging verified locally (18 assets); the CI run is pending.*
- ⏳ The release tag is created by the pipeline at `github.sha`. *Implemented; CI run pending.*
- ✅ Assets are re-downloaded and hash-compared against the staged bytes **before** the NuGet job
      starts. *`Validate` + `Compare` exercised locally: 18 assets, all bytes match.*
- ✅ `publishPackages` cannot run unless `release-verified == 'true'`, runs on `ubuntu-latest`, and
      has `contents: read` only.
- ✅ Releases are **never deleted** by the pipeline. *The `Remove transport Release` step is gone.*
- ✅ Pull requests build, stage, and validate — but create no release and publish nothing.
- ✅ A workflow re-run is idempotent: byte-identical assets are kept, missing ones uploaded, a byte
      mismatch fails the run.
- ⏳ A downstream repository can consume `diginsight/components` releases using the same
      `Download-PackageRelease.ps1` contract, pinning a `DiginsightComponentsVersion` property.
      *Requires a published components release.*

---

## ⚠️ Risks and mitigations

| # | Risk | Impact | Mitigation |
|---|------|--------|------------|
| R1 | `pwsh` 7 / `gh` / .NET 10 SDK missing on the self-hosted runner | Pipeline cannot run at all | Phase 0.1 gate before any code change; add `setup-dotnet` in `v2_03` |
| R2 | `../artifacts/packages` misresolves from `src/NuGet.Config` | Silent fallback to the proxy, or `NU1301` | Test with a **cold** `NUGET_PACKAGES` cache — a warm cache short-circuits source resolution entirely (upstream finding #6) |
| R3 | Warm global package cache masks a broken source configuration | False green | Validate with an isolated `NUGET_PACKAGES` directory |
| R4 | `NU1403` after regenerating lock files | CI `--locked-mode` restore fails | Purge `~/.nuget/packages/diginsight.*/3.8.0.1` and regenerate from released bytes only (Phase 1.9) |
| R5 | Multi-upstream download implemented as `-NoClean` instead of atomic staging | Half-populated source on partial failure; silent proxy fallback | Design mandates a single atomic publish; test 1.7 asserts it |
| R6 | Re-running an old workflow run reuses `github.run_number`, giving the same version with possibly different bytes | Release immutability violation, run fails | Intended fail-closed behaviour; document that a new push is required, never a forced overwrite |
| R7 | Changing `vars.BUILD_NUMBER_OFFSET` or `vars.VERSION_PREFIX` produces a colliding version | Same as R6 | Treat both as append-only; document in `eng/README.md` |
| R8 | Stale `bin/Release` outputs on the self-hosted runner | Wrong or duplicate packages staged | Keep the "remove previous Release outputs" step; `Stage` also filters by expected version and fails on unexpected ids |
| R9 | `.gitignore` negation placed before `**/[Pp]ackages/*` | `.gitkeep` silently untracked, then `NU1301` for everyone | Follow SmartCache ordering exactly; verify with `git check-ignore -v` |
| R10 | Exact `3.8.0.1` pin surfaces API changes hidden by `3.7.*` | Build breaks | Pin and build locally as a **separate** commit (Phase 1.8) before touching CI |
| R11 | Release created but NuGet push fails | Partially published version | Documented recoverable state: re-run; `--skip-duplicate` makes the push idempotent, and the release already unblocks downstream consumers |
| R12 | Self-hosted shared workspace contamination (`v2_00` log files, leftover artifacts) | *Verify working tree unchanged* fails spuriously | `actions/checkout@v4` cleans by default; if that proves unreliable, stop writing logs into the workspace in `v2_00` |
| R13 | Every push to `main` ships a new public NuGet version (F18) | Version churn on NuGet.org | Pre-existing behaviour, explicitly retained by decision 1 |

---

## 🚦 Verification gates

The plan is **actionable with no remaining decisions**. What remains are three empirical gates —
things the repository cannot tell us and that must be measured, not assumed:

| Gate | Question | Outcome |
|------|----------|---------|
| G1 | Does the self-hosted Windows runner have `pwsh` 7, `gh`, and the .NET 10 SDK? | ✅ on the dev machine (pwsh 7.6.5, gh 2.88.1, SDK 10.0.400). ⚠️ still unconfirmed **on the runner** |
| G2 | Does `<add key="local-release" value="../artifacts/packages" />` in `src/NuGet.Config` resolve correctly with a cold package cache? | ✅ all 9 `Diginsight.*` packages resolved from `artifacts/packages` with an isolated `NUGET_PACKAGES` |
| G3 | Does pinning `3.8.0.1` (from `3.7.*`) build cleanly? | ❌ — 6 compile errors. Resolved by the [🔧 Upstream API migration](#-upstream-api-migration) |

One non-blocking note: components has **no** `PackageReference` to any `Diginsight.SmartCache*`
package yet (F13). Downloading the SmartCache release is still correct and harmless — the folder
source simply carries packages nothing consumes — so decision 7 is implemented now, and the
`PackageReference`s can be added later with no further pipeline work.

### What is left

Everything that can be verified on a developer machine has been. The six open items all require the
pipeline to actually run:

| Item | Blocked on |
|------|-----------|
| 3.4 real run on `main` | A push |
| 3.6 write up `overview.md` | The first real run |
| Release produced automatically with 18 assets | A push |
| Tag created by the pipeline at `github.sha` | A push |
| Fresh clone restores from the proxy alone | The proxy receiving `3.8.0.1` |
| Downstream consumes a components release | A published components release |

And one carried-over caveat: **G1 is verified on the development machine, not on the self-hosted
runner.** Confirm `pwsh` 7, `gh`, and the .NET 10 SDK there before the first run.

---

## 📖 References

### Upstream implementation

| Repo | File | Role |
|------|------|------|
| telemetry | `eng/Publish-Packages.ps1`, `eng/package-manifest.json`, `eng/tests/`, `eng/README.md` | Producer contract |
| smartcache | `.github/workflows/v3.yml` | Gated release, API-key publishing, upstream download step |
| smartcache | `eng/Download-PackageRelease.ps1`, `eng/tests/Download-PackageRelease.Tests.ps1` | Consumer contract (single upstream) |
| smartcache | `NuGet.Config`, `artifacts/packages/.gitkeep`, `.gitignore` | Local source wiring |

### Components files this plan touches

| File | Change |
|------|--------|
| [.github/workflows/v3.yml](../../../../../.github/workflows/v3.yml) | **Delete** |
| [.github/workflows/v2_99.Package.CICD.yml](../../../../../.github/workflows/v2_99.Package.CICD.yml) | Least-privilege permissions, release gate, path filters, dry-run input |
| [.github/workflows/v2_01.GetCompositeVariables.yml](../../../../../.github/workflows/v2_01.GetCompositeVariables.yml) | Add `sourceTag` output, modernise `set-output` |
| [.github/workflows/v2_03.BuildPackages.yml](../../../../../.github/workflows/v2_03.BuildPackages.yml) | Upstream download, `-p:Version`, restore flags, stage/validate, durable verified release |
| [.github/workflows/v2_04.PublishPackages.yml](../../../../../.github/workflows/v2_04.PublishPackages.yml) | Artifact-based publish, stop deleting the release, `contents: read` |
| [src/NuGet.Config](../../../../NuGet.Config) | Add `local-release` source |
| [src/Directory.Build.props](../../../../Directory.Build.props) | Pin `DiginsightCoreVersion` and `DiginsightSmartCacheVersion` to `3.8.0.1` |
| `.gitignore` | Scoped artifacts rules plus a post-`**/[Pp]ackages/*` negation |
| `eng/Publish-Packages.ps1` | New — producer tooling |
| `eng/Download-PackageRelease.ps1` | New — **multi-upstream** consumer tooling |
| `eng/package-manifest.json` | New — 8-package inventory |
| `eng/upstream-releases.json` | New — repository-to-version-property mapping |
| `eng/tests/**` | New — tooling tests, including multi-upstream cases |
| `eng/README.md` | New — maintainer runbook |
| `artifacts/packages/.gitkeep` | New — load-bearing marker |
| `src/**/packages.lock.json` | Regenerate against released bytes |
| `src/Diginsight.Components.Azure/Metrics/QueryCostMetricRecorder.cs` | Named-service lookup replaced by optional injection |
| `src/Diginsight.Components.Configuration/Hosting/Extensions/ObservabilityExtensions.cs` | Named registrations replaced by a single registration per interface |
| `src/Diginsight.Components.Configuration/Hosting/MetricRecordingConfigurationEntries.cs` | New — preserves the appsettings schema |
| `README.md` | Bootstrap instructions |

### Official documentation

- [Package Source Mapping](https://learn.microsoft.com/nuget/consume-packages/package-source-mapping)
- [Workflow syntax — permissions](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax#permissions)
- [Reusable workflow outputs](https://docs.github.com/en/actions/sharing-automations/reusing-workflows#using-outputs-from-a-reusable-workflow)

---

**Document Version:** 2.2
**Last Updated:** 2026-08-31
**Supersedes:** v1.0 (tag-driven `v3.yml` proposal, single upstream)
**Next Review:** After the first CI run on the self-hosted runner
