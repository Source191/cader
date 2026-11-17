# Tasks

## 1. Reading the handle count

- [x] 1.1 Red then green: `ProcessRegistryIntegrationShould.ReadThisProcesssHandleCount` (a count above zero and within 10% of `Process.GetCurrentProcess().HandleCount`) and `ReportNoHandleCountForAProcessThatDoesNotExist`, with `IProcessRegistry.HandleCountOf` added and `ProcessRegistry` answering through `Opened<T>` and a `GetProcessHandleCount` import (design decision 1). `FakeProcessRegistry` answers a set value. Verify the class passes. Commit per step.

## 2. The reading in the counts

- [x] 2.1 Red then green: `OwnProcessMeterShould` reads a `FakeProcessRegistry`'s handles and memory into an `OwnHolding`, and each figure Windows refuses as `Unknown` (decision 2). Commit per step.
- [x] 2.2 Red then green: `DiagnosticLineShould` renders the "process now" line after the stalls line, with thousands separators, MB and `unknown` (decision 3), and the existing count lines are unchanged. Commit per step.
- [x] 2.3 Wire `OwnProcessMeter` into `PerformanceCounts` where it is built, and into `SceneHost` (decision 4). Verify `dotnet build -c Debug`, and that `PeriodicCountsShould`, the crash recorder's and the problem report's tests and `DiagnosticsPrivacyShould` pass. Commit.

## 3. Record and release

- [x] 3.1 Update `security.md`'s Diagnostics paragraph (the counts now carry Cader's own handles and private memory) and the user guide's description of the log if it lists the count lines. Verify both read clearly.
- [ ] 3.2 Raise `Version` with `.github\scripts\Step-Version.ps1` and add an Added entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed, linking the Learn page for `GetProcessHandleCount`.
