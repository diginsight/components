# ISSUE: Debug solution fails because the selected SDK cannot target .NET 11

**Date:** 2026-09-01  
**Author:** Dario Airoldi  
**Status:** Resolved and validated locally  
**Severity:** Medium  
**Component:** Diginsight Components debug solution and direct-import dependencies  
**Target Framework:** .NET 8.0, .NET 9.0, .NET 10.0, and .NET 11.0 Preview  

---

## 📋 Table of Contents

1. [📝 Description](#-description)
2. [🔍 Context Information](#-context-information)
3. [🔬 Analysis](#-analysis)
4. [🔄 Reproduction Steps](#-reproduction-steps)
5. [✅ Solution Implemented](#-solution-implemented)
6. [📚 Additional Information](#-additional-information)
7. [🔗 References](#-references)
8. [✔️ Resolution Status](#-resolution-status)
9. [🎓 Lessons Learned](#-lessons-learned)
10. [📎 Appendix](#-appendix)

---

## 📝 DESCRIPTION

Building the debug solution initially failed when the build host selected a .NET SDK that did not support a directly referenced project's `net11.0` target. After selecting SDK 11, compilation exposed a second failure: a locally modified Components metric-registration block used Telemetry APIs removed in July 2026. The Components projects themselves target up to .NET 10, but the debug solution also loads projects from the adjacent Telemetry and SmartCache repositories.

The Components repository had no `global.json`, so SDK selection depended on the invoking host. The stale local metric code then attempted to access the removed `MetricName` option property and `AddNamedSingleton` helper. Finally, assets restored by the older SDK did not contain complete .NET 11 package references and had to be regenerated under SDK 11.

### Error Message

```text
The current .NET SDK does not support targeting .NET 11.0. Either target
.NET 10.0 or lower, or use a version of the .NET SDK that supports .NET 11.0.
Download the .NET SDK from https://aka.ms/dotnet/download
```

```text
'BinderOptions' does not contain a definition for 'MetricName'.
'OptionsBasedMetricRecordingFilterOptions' does not contain a definition for 'MetricName'.
'OptionsBasedMetricRecordingEnricherOptions' does not contain a definition for 'MetricName'.
'IServiceCollection' does not contain a definition for 'AddNamedSingleton'.
```

### Impact

- The debug solution could not be reliably restored or built.
- Local development using direct source imports was blocked when an SDK 10 host was selected.
- Different developers or build hosts could obtain different results from the same checkout.
- The package-based Components build remained conceptually unaffected because the .NET 11 target originated in a directly imported external project.

### Evidence Sources

| Information | Source |
|-------------|--------|
| Reported error and affected solution | User report in the current conversation |
| SDK inventory and selected SDK | Diagnostic commands run during the conversation |
| Direct-import configuration | `src/Directory.Build.props.user` |
| External project composition | `src/Diginsight.Components.Debug.slnx` |
| .NET 11 target | `telemetry/src/Diginsight.Diagnostics.Log4Net/Diginsight.Diagnostics.Log4Net.csproj` |
| Implemented fix | New `src/global.json` |
| Follow-up compiler errors | Targeted and full solution builds run during the conversation |
| Telemetry API change | Commit `2b947e9` in the Telemetry repository |

---

## 🔍 CONTEXT INFORMATION

### Environment Details

| Property | Value |
|----------|-------|
| **Repository** | `diginsight/components` |
| **Branch** | `main` |
| **Solution** | `src/Diginsight.Components.Debug.slnx` |
| **Operating System** | Windows |
| **Installed SDKs** | 9.0.317, 10.0.111, 10.0.400, 11.0.100-preview.7.26381.103 |
| **Required SDK capability** | Support for the `net11.0` target framework |
| **Selected SDK after fix** | 11.0.100-preview.7.26381.103 |
| **Telemetry SDK policy** | Existing `telemetry/src/global.json`, SDK 11 Preview with prerelease enabled |
| **SmartCache SDK policy** | Updated `smartcache/src/global.json`, SDK 11 Preview with prerelease enabled |
| **Components targets** | net8.0, net9.0, net10.0; Windows variants where applicable |
| **External Telemetry targets** | Includes net11.0 in directly referenced projects |

### Exception Details

| Property | Value |
|----------|-------|
| **Exception Type** | MSBuild target-framework compatibility error, commonly `NETSDK1045` |
| **HTTP Status Code** | Not applicable |
| **Activity ID** | Not applicable |
| **Data Loss** | None |

### Relevant Build Path

```text
Diginsight.Components.Debug.slnx
	-> external Telemetry projects
		 -> Diginsight.Diagnostics.Log4Net.csproj
				-> TargetFrameworks includes net11.0
					 -> SDK 10 resolver rejects net11.0
```

### Configuration State Before the Fix

```text
global.json: absent
DiginsightCoreDirectImport: true
DiginsightSmartCacheDirectImport: true
Latest installed SDK: 11.0.100-preview.7.26381.103
Failing host behavior: selected an SDK that only supported .NET 10 or lower
```

### Relevant External Project Declaration

```xml
<TargetFrameworks>netstandard2.0;netstandard2.1;net8.0;net9.0;net10.0;net11.0</TargetFrameworks>
```

---

## 🔬 ANALYSIS

### Root Cause Analysis

#### Primary Cause: Unpinned SDK Resolution

The Components repository did not contain a `global.json`. Without an SDK policy, `dotnet` and IDE build hosts independently resolve an installed SDK. A host that resolves .NET SDK 10 cannot evaluate or build a `net11.0` target and emits the reported compatibility error.

#### Why a Components Build Requested .NET 11

The Components project files target no higher than .NET 10. However, the local `Directory.Build.props.user` enables direct imports from sibling Telemetry and SmartCache source trees. The debug solution also lists those external projects explicitly. The effective build graph therefore includes Telemetry projects with `net11.0` targets.

#### Why Installing SDK 11 Was Not Sufficient

SDK 11 Preview was already installed, but prerelease SDK selection can vary by host. In particular, an IDE may decline to use a preview SDK unless prerelease usage is enabled. An explicit repository-level SDK policy removes that ambiguity and communicates the build prerequisite to every compatible host.

#### Secondary Cause: Locally Reintroduced Obsolete Telemetry APIs

The local `ObservabilityExtensions.cs` differed from the current Components `main` implementation. It bound configuration directly into `OptionsBasedMetricRecordingFilterOptions` and `OptionsBasedMetricRecordingEnricherOptions`, expected both types to expose `MetricName`, and called `AddNamedSingleton`. Telemetry commit `2b947e9` removed those members and the `NamedOptionsMonitor` helper.

The compatible Components implementation binds metric names into the local `MetricRecordingFilterConfiguration` and `MetricRecordingEnricherConfiguration` DTOs, configures named option instances, and registers the filter and enricher through `TryAddSingleton`.

#### Tertiary Cause: Stale Restore Assets

The first complete `--no-restore` build under SDK 11 reached `net11.0` but could not resolve several Microsoft.Extensions types in `Diginsight.Core`. Running a full solution restore under SDK 11 regenerated the .NET 11 assets. The subsequent complete build exited with code 0.

#### Error Manifestation

```text
1. Open or build Diginsight.Components.Debug.slnx.
2. Load external source projects through the debug solution/direct-import settings.
3. Evaluate a Telemetry project containing net11.0.
4. Resolve SDK 10 instead of the installed SDK 11 Preview.
5. Fail target-framework validation before normal compilation.
6. After selecting SDK 11, compile the stale local metric block and encounter removed Telemetry APIs.
7. Restore the full solution under SDK 11 to refresh the .NET 11 dependency assets.
```

### Impact Assessment

| Category | Impact | Severity |
|----------|--------|----------|
| **Functionality** | Debug solution restore/build is blocked under SDK 10 | High |
| **Data Integrity** | No runtime or persisted data is modified | None |
| **Developer Experience** | Local build behavior depends on host SDK resolution | Medium |
| **CI/Reproducibility** | Unpinned environments may select incompatible SDKs | Medium |
| **Production Runtime** | No direct production impact | None |

### Affected Workflows

1. ❌ **Debug solution build under SDK 10:** Fails while evaluating `net11.0`.
2. ❌ **IDE restore under an SDK-10 resolver:** Reports unsupported target-framework errors.
3. ⚠️ **Fresh developer setup:** Requires installation of a compatible .NET 11 prerelease SDK.
4. ✅ **Build using SDK 11 Preview:** The previously failing `net11.0` project compiles.
5. ✅ **Components-only target frameworks:** Remain net8.0 through net10.0.

---

## 🔄 REPRODUCTION STEPS

### Step-by-Step Reproduction

1. Use a machine or build host that resolves .NET SDK 10.
2. Keep direct imports enabled in `src/Directory.Build.props.user`.
3. From the `src` directory, build `Diginsight.Components.Debug.slnx`.
4. Allow MSBuild to evaluate the external Telemetry projects.
5. Observe the unsupported `.NET 11.0` target-framework error.

### Diagnostic Checks

```powershell
dotnet --version
dotnet --list-sdks
dotnet --info
dotnet build .\Diginsight.Components.Debug.slnx
```

### Affected Code and Configuration Locations

| Location | Role |
|----------|------|
| `src/Diginsight.Components.Debug.slnx` | Includes external Telemetry and SmartCache projects |
| `src/Directory.Build.props.user` | Enables direct source imports from sibling repositories |
| `telemetry/src/Diginsight.Diagnostics.Log4Net/Diginsight.Diagnostics.Log4Net.csproj` | Declares a `net11.0` target |
| `src/global.json` | Defines the compatible SDK selection policy after the fix |

This was an SDK-resolution issue rather than an application method or source-code defect, so no method name or application line number applies.

---

## ✅ SOLUTION IMPLEMENTED

### Fix Overview

A repository-level `src/global.json` was added beside the solution. It requests the installed .NET 11 Preview 7 SDK, explicitly permits prerelease SDKs, and permits roll-forward within compatible .NET 11 feature bands. The locally modified metric-registration block was restored to the current Components implementation that matches the Telemetry API, and the solution was restored again under SDK 11.

### Code Changes

#### 1. Add an SDK Selection Policy

**Location:** `src/global.json`

```json
{
	"sdk": {
		"version": "11.0.100-preview.7.26381.103",
		"rollForward": "latestFeature",
		"allowPrerelease": true
	}
}
```

### Solution Features

#### ✅ Deterministic SDK Selection

- Builds started from the solution directory resolve an SDK capable of targeting .NET 11.
- CLI and compatible IDE hosts receive the same repository-level SDK requirement.

#### ✅ Explicit Preview Opt-In

- `allowPrerelease` records that the current build graph intentionally requires a preview SDK.
- The setting avoids relying solely on machine- or IDE-level preview preferences.

#### ✅ Controlled Roll-Forward

- `latestFeature` allows use of a newer compatible .NET 11 feature band when available.
- Roll-forward does not silently downgrade the build to SDK 10.

#### ✅ Compatible Metric Configuration

- Metric names are bound through Components configuration DTOs rather than removed Telemetry option properties.
- Filters and enrichers use the currently supported singleton registration model.
- No Telemetry source change was required.

#### ✅ Aligned SmartCache Debug SDK

- SmartCache's debug solution directly includes the Telemetry projects that target `net11.0`.
- Its SDK policy was aligned with Components at `11.0.100-preview.7.26381.103`, with prerelease selection enabled.
- The SmartCache debug solution was restored and built successfully with exit code 0.

#### ✅ Refreshed .NET 11 Assets

- A complete restore under SDK 11 regenerated assets for the `net11.0` target graph.
- The complete debug solution subsequently built with exit code 0.

### Before and After

| Scenario | Before | After |
|----------|--------|-------|
| SDK policy | Host-dependent; no `global.json` | Repository requests SDK 11 Preview |
| Prerelease permission | Host-dependent | Explicitly enabled |
| `net11.0` evaluation | Fails when SDK 10 is selected | Supported by selected SDK 11 Preview |
| CLI `dotnet --version` from `src` | Not pinned | 11.0.100-preview.7.26381.103 |

---

## 📚 ADDITIONAL INFORMATION

### Testing Performed

1. Enumerated the installed SDKs and confirmed .NET 11 Preview 7 was present.
2. Confirmed that `dotnet --version` from the solution directory resolves `11.0.100-preview.7.26381.103` after adding `global.json`.
3. Started an incremental debug-solution build with the selected SDK.
4. Observed successful compilation of a `net11.0` target, including `Diginsight.Diagnostics net11.0` and the previously identified `Diginsight.Diagnostics.Log4Net net11.0` project.
5. Confirmed that the new JSON file has no editor diagnostics.
6. Reproduced eight metric API compilation errors in the Configuration project and traced them to a local implementation that used removed Telemetry APIs.
7. Restored the compatible metric-registration implementation and successfully built the Configuration project for `net10.0`.
8. Restored the complete debug solution under SDK 11.
9. Built the complete debug solution with `--no-restore --verbosity:quiet -m:1`; the process exited with code 0.
10. Aligned SmartCache's `global.json`, then restored and built `Diginsight.SmartCache.Debug.slnx`; the process exited with code 0.

### Testing Recommendations

#### Integration Tests

1. **Clean solution build**
	 - Delete or clean generated outputs as appropriate.
	 - Restore and build the complete debug solution.
	 - Expected result: zero target-framework compatibility errors.

2. **IDE reload**
	 - Close and reopen the solution after adding `global.json`.
	 - Confirm the IDE reloads its SDK/MSBuild context.
	 - Expected result: the unsupported .NET 11 error is absent.

3. **Missing SDK behavior**
	 - Test on a machine without a compatible SDK 11 installation.
	 - Expected result: a clear SDK-resolution error identifying the required version.

### Migration Considerations

#### ⚠️ Prerelease Dependency

All contributors and build agents that build the debug solution must currently have a compatible .NET 11 prerelease SDK. The SDK pin should be updated when the repository standardizes on a newer preview, release candidate, or stable .NET 11 SDK.

#### Alternative Options Considered

**Option 1: Pin SDK 11 Preview — implemented**
- Preserves the sibling Telemetry project's .NET 11 target.
- Makes the actual debug-build requirement explicit.

**Option 2: Remove `net11.0` from the external project**
- Would allow SDK 10 but changes the Telemetry repository's intended target matrix.
- Not appropriate as a Components-only fix.

**Option 3: Exclude external projects or disable direct imports**
- Avoids the .NET 11 build edge but changes the purpose of the debug solution and local source-debugging workflow.
- Package-based references may still be used when direct source debugging is not required.

### Performance Impact

| Operation | Before Fix | After Fix | Delta |
|-----------|------------|-----------|-------|
| **SDK resolution** | Host-dependent | Reads a small local JSON policy | Negligible |
| **Compilation scope** | Existing target matrix | Unchanged | None |
| **Runtime performance** | Unchanged | Unchanged | None |

### Security Considerations

- ✅ No credentials, connection strings, or secrets were added.
- ✅ No runtime security behavior changed.
- ⚠️ Preview SDK servicing and support policies should be reviewed before use on production build agents.

---

## 🔗 REFERENCES

### Official Documentation

- [.NET SDK selection overview](https://learn.microsoft.com/dotnet/core/versions/selection): SDK selection and `global.json` behavior.
- [global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json): SDK version, roll-forward, and prerelease settings.
- [.NET SDK error NETSDK1045](https://learn.microsoft.com/dotnet/core/tools/sdk-errors/netsdk1045): Unsupported target-framework troubleshooting.
- [.NET downloads](https://aka.ms/dotnet/download): Install a compatible SDK.

### Repository References

| File | Purpose |
|------|---------|
| `src/global.json` | New SDK selection policy |
| `src/Diginsight.Components.Debug.slnx` | Debug solution containing external source projects |
| `src/Directory.Build.props.user` | Local direct-import configuration |
| `src/Directory.Build.props` | Shared Components build properties |

### Conversation and Diagnostic Evidence

- The issue symptom and exact error came from the user's opening report.
- Repository searches showed Components project targets ending at `net10.0`.
- Inspection of the external Telemetry source identified projects targeting `net11.0`.
- SDK enumeration showed that a suitable .NET 11 Preview SDK was already installed.
- Post-fix diagnostics confirmed that the solution directory selects that SDK.

---

## ✔️ RESOLUTION STATUS

### 🎯 RESOLVED AND VALIDATED LOCALLY

**Resolution Date:** 2026-09-01  
**Resolved By:** Dario Airoldi with GitHub Copilot assistance  
**Resolution Type:** Build configuration change  

### Verification Checklist

- [x] **Root cause identified**
	- [x] Confirmed Components projects do not directly target .NET 11.
	- [x] Identified .NET 11 targets in direct-imported Telemetry projects.
	- [x] Confirmed SDK 11 Preview is installed.

- [x] **Configuration change implemented**
	- [x] Added `src/global.json`.
	- [x] Enabled prerelease SDK selection.
	- [x] Configured compatible feature-band roll-forward.

- [x] **Targeted validation**
	- [x] Confirmed SDK 11 Preview is selected from `src`.
	- [x] Observed successful `net11.0` project compilation.
	- [x] Confirmed no diagnostics in `global.json`.

- [x] **Complete command-line validation**
	- [x] Restored the full debug solution under SDK 11.
	- [x] Completed the full debug-solution build with exit code 0.

- [ ] **Environment validation**
	- [ ] Reload the solution in the IDE and confirm the cached SDK error is gone.
	- [ ] Validate the SDK policy on the intended CI build agent.

### Follow-up Actions

#### Immediate — Priority 1

- [ ] Reload the IDE solution so its SDK resolver re-evaluates `global.json`.
- [x] Complete one restore and build of the entire debug solution.

#### Short-term — Priority 2

- [ ] Ensure developer onboarding documentation mentions the .NET 11 prerelease prerequisite.
- [ ] Ensure CI installs or selects a compatible SDK 11 version before building this solution.

#### Long-term — Priority 3

- [ ] Update the SDK pin when moving to a newer .NET 11 preview, release candidate, or stable release.
- [ ] Consider defining a shared SDK policy across Components, Telemetry, and SmartCache if they are routinely built as one source graph.

### Success Criteria

✅ **Achieved:**
- SDK 11 Preview is selected deterministically from the solution directory.
- The build can evaluate and compile .NET 11 target projects.
- No product source code or target-framework matrix was changed.
- The complete command-line debug solution build exits successfully.

📋 **Pending Verification:**
- The IDE reports no stale target-framework compatibility errors after reload.
- The intended CI agent honors the SDK policy.

---

## 🎓 LESSONS LEARNED

### What Went Wrong

1. **The SDK requirement was implicit:** The effective multi-repository build targeted .NET 11, but the Components repository had no SDK policy.
2. **The local project targets hid the source:** Searching only Components project files suggested a maximum of .NET 10; the .NET 11 target came from sibling repositories.
3. **Installed did not mean selected:** SDK 11 Preview was present, but the failing host selected an incompatible SDK.

### What Went Right

1. **The effective solution graph was inspected:** This exposed the external projects included by the debug workflow.
2. **The SDK inventory was verified before installation:** No unnecessary SDK installation was performed.
3. **The fix was configuration-only:** The intended target matrix remained unchanged.
4. **Targeted compilation provided evidence:** A `net11.0` project compiled under the newly selected SDK.

### Improvements for Future

1. Add or update `global.json` whenever the highest target framework in the effective build graph changes.
2. Validate both direct package builds and cross-repository direct-import builds in automation.
3. Document prerelease SDK requirements before adding preview target frameworks.
4. Distinguish SDK installation from SDK selection during diagnosis.
5. After changing SDKs, restore before treating missing package-provided types as source defects.
6. Report partial versus complete build validation explicitly; a successful target project is strong evidence but is not equivalent to a completed full solution build.

---

## 📎 APPENDIX

### A. SDK Inventory Captured During Diagnosis

```text
9.0.317
10.0.111
10.0.400
11.0.100-preview.7.26381.103
```

### B. Selected SDK After the Fix

```text
11.0.100-preview.7.26381.103
```

### C. Expected Recovery Procedure

```text
1. Install a compatible .NET 11 SDK if it is absent.
2. Pull the repository-level global.json.
3. Reload the IDE or restart its build host.
4. Confirm `dotnet --version` from the src directory.
5. Restore and build Diginsight.Components.Debug.slnx.
```

### D. Scope Clarification

The issue was reported as a Components build failure, but the unsupported target was introduced through the debug solution's external source graph. This distinction matters because downgrading the Components target frameworks would not resolve the actual cause.

---

**Document Version:** 1.2  
**Last Updated:** 2026-09-01  
**Next Review:** After the next clean full-solution and IDE validation
