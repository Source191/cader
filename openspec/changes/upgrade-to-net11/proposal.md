# Proposal

## Why

.NET 11 was released on 10 November 2026 and Cader still targets .NET 10 (issue #212; the issue says Vantage, the project's earlier name). Because the winget build is self-contained, every user gets the runtime Cader is built on, so the move is both a compatibility job (every breaking change and package checked before the switch) and a chance to write current code: the process APIs that .NET 11 adds fit what Cader does most (starting and watching child programs), and C# 15's closed hierarchies let the compiler enforce what Cader's `ClosedHierarchiesShould` and `UnreachableException` arms enforce by convention today. The owner asked for the whole of the issue in one change.

## What Changes

- **Retarget:** `net11.0` in all eleven projects under `src`, `test` and `tools`; `global.json` to the 11.0.x SDK; `AnalysisLevel` in the root `Directory.Build.props` to `11.0-recommended`; the `net10.0` output paths in `claude-contract.yml`, `pipeline.yml` and `tools\HeadlessTerminal\HandoffCheck.cs`; the .NET 10 wording in `openspec\config.yaml`, the README, CONTRIBUTING and `.claude\rules`.
- **Packages:** every package moved to a release that supports .NET 11 (Terminal.Gui, System.CommandLine, Microsoft.Extensions.*, Interop.UIAutomationClient, Microsoft.CodeAnalysis.CSharp, the two analysers, xunit.v3, Shouldly, NetArchTest.Rules, Stryker.NET), each recorded in the design as moved, held or replaced with the reason. `THIRD-PARTY-NOTICES.txt` is regenerated for the new runtime assemblies.
- **Breaking changes:** each item in the .NET 11, SDK and Roslyn lists is recorded in the design as shown not to apply, or handled. New CA, IDE and Sonar diagnostics are fixed, not suppressed.
- **Process APIs:** `Process.RunAsync` or `RunAndCaptureTextAsync` for `ProcessRunner`, `StartAndForget`, `StartDetached` and the standard-handle properties for `Desktop.StartAsync`, `Process.TryGetProcessById` for the process registry, `KillOnParentExit` for short-lived captured children. Each is adopted only where it keeps `ProgramLocator`'s full paths, `InheritedEnvironment`, UTF-8 decoding, the timeout with tree kill and the `ConfirmedProcess` rules exactly as they are; otherwise declined with a reason.
- **C# 15:** `closed` on the two-case Domain and Ports hierarchies (the open ones keep their fallback arms), with the discard arms, `ClosedHierarchiesShould` and the switch rule in `CLAUDE.md` updated; union types, collection expression arguments, extension indexers and labelled `break` and `continue` adopted where they read better, otherwise declined with a reason.
- **Runtime and libraries:** runtime async checked against the diagnostics and `ExceptionSummary`; System.Text.Json and `EqualityComparer<T>.Create` adopted only where the state-file format on disk is unchanged.
- **House style:** `CLAUDE.md` and `.editorconfig` made to expect the new features (at `warning`), with reflection or source facts where a rule can be checked mechanically, and existing code brought into line in the same change.
- **Release:** a minor `Version` bump and a `CHANGELOG.md` line; the self-contained size and start-up time re-measured against the `install-with-winget` figures; the README says so if the new x86-64-v2 floor could stop Cader starting.
- **No behaviour change:** screens, keys, wording, settings and state files stay as they are, so this change declares `skip_specs: true`.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; `quiet-editor-start` owns the `folder-actions` delta for the editor key and this change must keep it true.

## Impact

- **Code:** `ProcessRunner`, `Desktop`, `OpenFileScript`, `ProcessRegistry` and `ProcessRegistryNative`; the Domain and Ports hierarchies and their `switch` expressions (59 files mention `UnreachableException`); `ClosedHierarchiesShould`; any use of the comparers and JSON options listed in the design.
- **Build and CI:** all `.csproj` files, `global.json`, `Directory.Build.props`, `.github\actions\build-test-pack\action.yml`, `pipeline.yml`, `claude-contract.yml`, `codeql.yml`, `publish.yml` (no `dotnet-version` line is there today; confirmed in the design).
- **Docs:** README, CONTRIBUTING, `src\Source191.Cader\README.md`, `docs\performance-monitoring.md`, `.claude\rules` (re-verified facts with date and version), `CLAUDE.md`, `CHANGELOG.md`.
- **Users:** a larger or smaller download and a different start-up time (measured, not assumed), and no x86/x64 machine below x86-64-v2 can start it.

## Order

This change starts only after `quiet-editor-start`, `stricter-state-files`, `mutation-testing-in-ci` and `install-with-winget` have merged (or are rebased on), because they touch `Desktop.StartAsync`, `JsonFileStore`, the Stryker workflow and the size baselines this change re-measures. It also follows `diagnostic-log-setting` and `fetch-before-branching` (both parts), because the C# 15 sweep touches the settings and launch code they change. It does not wait for `add-win-arm64-build` or `sign-the-executable`: they are workflow-only and held by owner gates, so whichever of the three merges second rebases the `pipeline.yml` output paths. `openspec\implementation-plan.md` splits this change into four pull requests.

## Out of scope

- Multiple Claude accounts, which is a later change.
- Moving to .NET 12 or any preview release; the support dates are recorded so that move can be planned.
- Native AOT, trimming or any other publish-mode change.
- Any change to what a key, screen, setting or state file does or says.
- Changes to the `ConfirmedProcess` rules in `security.md`.
