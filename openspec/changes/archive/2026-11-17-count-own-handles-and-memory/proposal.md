# Proposal

## Why

The output-pipe leak fixed in #318 grew by about three handles a second and was only noticed because a console left open for 16 hours held 116,475 handles and twice its usual memory. The diagnostic log already writes counts every 10 minutes and at exit, but none of them says how many handles or how much memory Cader itself holds, so a problem report could not have shown the leak, and the next one would go unseen the same way.

## What Changes

- Each time the counts are written (every 10 minutes while the Diagnostic log setting is on, at exit, in the crash file and in a problem report), one more line gives Cader's own handle count and private memory at that moment, so a report of a slow or swollen console carries the evidence.
- The line says "now", not "since start": unlike the other counts it is a reading, so two marks compared show growth.

Out of scope:
- **Handles or memory of sessions and their children:** the detail pane and `m` already show sessions' memory; a session's handles are Claude Code's concern, not Cader's.
- **Warning the user when the figures grow:** a reading in the log is enough to diagnose; a threshold would need its own design.
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `diagnostics`: a new requirement that the counts carry Cader's own handle count and private memory at the moment they are written.

## Impact

- **`IProcessRegistry`** (`Domain`) gains a handle-count read, and `ProcessRegistry` answers it through the limited-rights handle it already opens (`GetProcessHandleCount`); `FakeProcessRegistry` follows.
- **`Application\Diagnostics`:** the counts' rendering (`DiagnosticLine`) and their writers (`PeriodicCounts`, the exit write, `CrashRecorder`) take the reading.
- **Tool project wiring:** hands the registry and Cader's own process id to the counts.
- **Tests:** `DiagnosticLineShould`, `PeriodicCountsShould` and a `ProcessRegistryIntegrationShould` fact for the handle count; `DiagnosticsPrivacyShould` keeps passing (the line holds only numbers).
- **Version:** `src\Source191.Cader\Source191.Cader.csproj` raises its `Version`; `CHANGELOG.md` gets an Added entry.
- **Users:** nothing on screen changes; the log file, the crash file and a problem report gain one line wherever the counts appear.
