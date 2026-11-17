# Design

## Context

- **One renderer for every writer:** `DiagnosticLine.Render(PerformanceCounts, at)` gives the count lines to `DiagnosticLog.WriteCounts` (the 10-minute `PeriodicCounts` timer and the exit write), to `CrashRecorder`'s crash file and to `ProblemReports`. `PerformanceCounts` is a record of the three count stores (`ProcessCounts`, `RefreshCounts`, `StallCounts`); each line says "since start".
- **What can be read today:** `IProcessRegistry.UsageOf(int)` answers a process's start, private working set and label through a `PROCESS_QUERY_LIMITED_INFORMATION` handle, and `HeaderSettingsRow` already reads Cader's own private memory this way. Nothing reads a handle count; [`GetProcessHandleCount`](https://learn.microsoft.com/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesshandlecount) accepts the same limited right.
- **Privacy:** count lines are a maintainer's format, not events, and `DiagnosticsPrivacyShould` runs the daily flows over Northwind fixtures; a line of two numbers holds nothing personal.

## Goals / Non-Goals

**Goals:**
- Every place the counts appear shows Cader's own handles and private memory at that moment.
- One home for the reading, so no writer can leave it out.

**Non-Goals:**
- Sessions' handles, thresholds or warnings on screen.

## Decisions

1. **`IProcessRegistry.HandleCountOf(int processId)` answers the count or null, as its other reads do.** `ProcessRegistry` adds it to `Opened<T>` with a `GetProcessHandleCount` import on the `SafeProcessHandle`, so it inherits the limited right and the `using` release. `FakeProcessRegistry` answers a set value.
   - *Rejected: adding the count to `ProcessUsage`.* Every measured session process would then pay for a figure nothing shows.
   - *Rejected: `Process.GetCurrentProcess().HandleCount` in the wiring.* `System.Diagnostics.Process` stays inside `Adapters`, and the wiring would hold untested logic.
2. **`PerformanceCounts` gains a fourth member, `OwnProcessMeter`,** an `Application\Diagnostics` class holding the registry and Cader's process id, whose `Read()` answers an `OwnHolding` of a handle figure and a memory figure, each a named case (`Read` or `Unknown`), never null (NullFreeModels). `DiagnosticLine.Render(PerformanceCounts, at)` appends `{stamp} process now: {handles} handles, {memory} private memory` after the stalls line, so the log, the crash file and the problem report all get it from the one renderer.
   - *Rejected: a separate writer for the reading.* It would have to be called beside every `Render` and could be forgotten by the next writer.
3. **Wording of the line:** the same maintainer's format as the other count lines (`DiagnosticLine`, not `Wording`), numbers with invariant culture and thousands separators, memory in MB as `MemorySize` already formats it, and `unknown` for a figure not read.
4. **Wiring:** `SharedParts` (or `ConsoleComposition`, where `PerformanceCounts` is built) passes the `ProcessRegistry` and `Environment.ProcessId`; `SceneHost` copies the wiring with its scene registry.

## Risks / Trade-offs

- **[Risk] A read at exit or in a crash handler fails or is slow.** → Two limited-rights calls on Cader's own process take well under a millisecond (`ProcessRegistryIntegrationShould` keeps 200 usages under a second), and a failure reads as `unknown` rather than throwing.
- **[Trade-off] A point-in-time figure among cumulative ones.** The line says "now" so it is not read as a total.
