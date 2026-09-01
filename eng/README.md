# Release engineering

This folder holds the tooling that makes `diginsight/components` independent of NuGet.org and
corporate proxy propagation latency, in both directions:

| Direction | Script | Purpose |
|-----------|--------|---------|
| **Consume** | `Download-PackageRelease.ps1` | Download and verify upstream Diginsight releases into the local package source |
| **Produce** | `Publish-Packages.ps1` | Stage, validate, verify, and publish this repository's packages |

Both require PowerShell 7 or later. The consumer script also requires the GitHub CLI (`gh`).

## Developer flow

```powershell
./eng/Download-PackageRelease.ps1
dotnet restore src/Diginsight.Components.Build.slnx
```

The first command fills `artifacts/packages` with the `.nupkg` files of the pinned upstream
versions. That folder is declared as the `local-release` package source in `src/NuGet.Config`, so a
**plain** `dotnet restore` — and any Visual Studio or VS Code build — resolves `Diginsight.*`
locally while everything else continues to come from the corporate proxy. No wrapper script, no
`--source` arguments, no IDE configuration.

You only need the first command when the pinned version has not yet reached the proxy. When it has,
a plain restore works with an empty `artifacts/packages`.

## Upstreams

`upstream-releases.json` maps each upstream repository to the MSBuild property that pins it:

| Repository | Property |
|------------|----------|
| `diginsight/telemetry` | `DiginsightCoreVersion` |
| `diginsight/smartcache` | `DiginsightSmartCacheVersion` |

The versions themselves live **only** in `src/Directory.Build.props` and are read with
`dotnet msbuild -getProperty:`. They are never duplicated here.

Every upstream is downloaded and fully verified before `artifacts/packages` is touched, so a failure
on any upstream leaves the local source exactly as it was.

### Repinning an upstream

```powershell
./eng/Download-PackageRelease.ps1 https://github.com/diginsight/telemetry -Version 3.8.0.2
dotnet restore src/Diginsight.Components.Build.slnx --force-evaluate
```

This rewrites the property in `src/Directory.Build.props`, downloads that release, and regenerates
the lock files. Commit the props change and all `packages.lock.json` files together.

Regenerate lock files **against the downloaded release bytes**, never against a locally built
upstream — otherwise CI's `--locked-mode` restore fails with `NU1403`. If that happens:

```powershell
Remove-Item "$env:USERPROFILE\.nuget\packages\diginsight.*\<version>" -Recurse -Force
dotnet restore src/Diginsight.Components.Build.slnx --force-evaluate
```

## Versioning

Versions are computed, not hand-authored:

```text
assemblyVersion = VERSION_PREFIX . (github.run_number + BUILD_NUMBER_OFFSET)   e.g. 0.8.0.123
sourceTag       = v{assemblyVersion}                                           e.g. v0.8.0.123
```

`VERSION_PREFIX` and `BUILD_NUMBER_OFFSET` are repository variables. **Treat both as append-only.**
Lowering either can produce a version that was already published, which fails closed rather than
overwriting a release.

The pipeline creates the git tag itself via `gh release create --target <sha>`; nothing is tagged by
hand. NuGet drops a zero fourth component, so `v0.8.0.0` yields version `0.8.0` while `v0.8.0.123`
yields `0.8.0.123`. Both spellings are resolved when consuming a release.

## Release pipeline

`.github/workflows/v2_99.Package.CICD.yml` runs on every push to `main`:

| Job | Runner | Permissions | Role |
|-----|--------|-------------|------|
| `installActions` | self-hosted | `contents: read` | Toolchain setup |
| `getCompositeVariables` | self-hosted | none | Compute `assemblyVersion` and `sourceTag` |
| `buildPackages` | self-hosted (Windows) | `contents: write` | Build once, stage, create and verify the GitHub Release |
| `publishPackages` | `ubuntu-latest` | `contents: read` | Push the same bytes to NuGet.org |

The build cannot run on a Linux runner because `Diginsight.Components.Presentation` targets
`net*-windows` with WPF. The push cannot run on the self-hosted runner because `api.nuget.org` is
unreachable from the corporate network. Hence the split.

`publishPackages` is gated on `releaseVerified == 'true'` and holds `contents: read`, so it cannot
run before the release is verified and cannot modify a release. The NuGet credential exists only in
that job; the build job never sees it.

Pull requests build, stage, and validate — but create no release and publish nothing.

### Dry run

Trigger the workflow manually with `dryRun: true`. It builds, stages, and validates the full
release set without creating a release or publishing.

Locally:

```powershell
./eng/Publish-Packages.ps1 -Command Stage -Tag v0.8.0.999 -SourceRoot ./src -StagePath artifacts/release/v0.8.0.999
```

## Release contents

Each release carries individual files — never only an archive, because NuGet needs `.nupkg` files
directly in a folder source:

- 8 `.nupkg` and 8 `.snupkg` (see `package-manifest.json`);
- `SHA256SUMS`, for humans and standard tooling;
- `release-manifest.json`, the machine-readable authority.

The two metadata files must agree, and validation checks both. Expected package IDs come from the
tracked `package-manifest.json` rather than a wildcard search, so a package that silently stops
being produced fails the release instead of shipping an incomplete set.

## Recovery

| Failure point | State | Recovery |
|---------------|-------|----------|
| Tests, bootstrap, restore, build, staging, or validation | Nothing published | Fix and re-run |
| Release created, **NuGet push failed** | Release already unblocks dependent builds | Re-run; `--skip-duplicate` makes the push idempotent |
| Release upload interrupted | Some assets present | A re-run keeps byte-identical assets, uploads the rest, and re-verifies the whole inventory |
| Upstream release missing or corrupt | Nothing entered the local source | The build fails **before** restore, naming the repository, tag, and missing assets |

Two invariants make re-runs safe:

- **Never replace a published version.** If a re-run's staged bytes disagree with an existing asset
  of the same name, the run fails. Package versions are immutable.
- **Never fall back silently.** A missing upstream release fails closed rather than reverting to the
  corporate proxy, which would reintroduce the latency this design removes.

## Tests

```powershell
./eng/tests/Publish-Packages.Tests.ps1      # 8 tests
./eng/tests/Download-PackageRelease.Tests.ps1   # 18 tests
```

Both suites run in CI before anything is built.

## `artifacts/packages/.gitkeep` is load-bearing

NuGet fails with `NU1301` when a configured folder source does not exist, and that error **cannot**
be suppressed with `NoWarn`, `RestoreNoWarn`, or `WarningsNotAsErrors`. Deleting the marker breaks
every restore in the repository, for everyone, including people who never use this tooling.

`.gitignore` keeps the marker tracked with a scoped rule set. Note that a blanket `artifacts/`
exclusion makes re-inclusion impossible — git cannot re-include a path whose parent directory is
excluded — and that the Visual Studio template's `**/[Pp]ackages/*` rule matches
`artifacts/packages/`, so the negation must come **after** it.
