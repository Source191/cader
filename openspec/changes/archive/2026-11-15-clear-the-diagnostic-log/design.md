# Design

## Context

- `Adapters\Diagnostics\DiagnosticFile` appends each line with `File.AppendAllText` (no handle kept open; `FileShare.Read` for the moment of each append) and, when a line would pass 10 MB, moves `cader.log` to `cader.log.1` first; both failures are swallowed. Several Cader processes may write the same file.
- The Config tab's Diagnostic log file row is `SetupState.LogFile(Path, Exists)` (setting on) or `LogOff(Path)` (setting off), read by `SetupFactsReader` through `IFolders.FileExists`; `ConfigRows.LogFileAs` drops the existence fact when the setting is off. `cader.log.1` is never shown.
- `IFolders` reads only; `IFolderDeleter` deletes folder trees and only `WorktreeRemover` may hold it. Nothing outside `Adapters` touches `System.IO.File`.
- Every dispatched effect makes `TableBody` reload its tab when it settles, so the Config rows are read again after a clear with no new code.
- `EffectDispatcher` already records `EffectRan` (effect, outcome kind, duration) for every handler.

## Goals / Non-Goals

**Goals:**
- One narrow port that can touch only the two log files, and only at the paths it was built with.
- The clear never fails the console, whatever other Cader processes or programs are doing to the files.
- The four-piece key pattern, so `EffectDispatcherShould`, `DialogEffectsShould`, `HandlersShould`, `KeyHelpParityShould` and `StatusTonesShould` hold the new pieces.

**Non-Goals:**
- No new smoke step: `SmokeScript` steps through some dialogs, not every `DialogName` (the confirm dialogs for deleting a session or a branch have none), and pressing `d` in the smoke run would reach real files under the runner's profile.
- No `LayeringShould` fact for the new port: it can delete only two fixed files, so the narrowness is in the port itself.

## Decisions

### 1. A narrow port, `IDiagnosticLogFiles`, built over the log path

`Ports\IDiagnosticLogFiles` has two methods and takes no path:

- `LogFiles Read()`: `LogFiles(LogFileOnDisc Current, LogFileOnDisc Older)`, where `LogFileOnDisc` is a closed hierarchy in Ports, `Absent(string Path)` or `Written(string Path, long Bytes)`. It never throws: a file that goes between the existence check and the length (a rollover or another clear) reads as `Absent`.
- `LogFilesCleared Clear()`: deletes `cader.log` and then `cader.log.1`, answering `LogFilesCleared(int Removed, long Bytes, int Kept)`. It never throws: a missing file (or folder) is skipped, and an `IOException` or `UnauthorizedAccessException` on a delete counts the file as kept.

The adapter `Adapters\Diagnostics\DiagnosticLogFiles(string path)` sits beside `DiagnosticFile`, and the older file's name has one home, `DiagnosticFile.OlderPath(path)`, which both use. `TestDoubles\FakeDiagnosticLogFiles` holds the two files' sizes in memory, can mark a file as held open, and counts its clears.

- **Security:** `Clear` takes nothing, so no text from a person, a repository or a listing can choose what is deleted; the paths come from `SharedParts` (`%LOCALAPPDATA%\Cader`) and `SceneHost`'s scene folder. `security.md` gains this rule beside Deleting folders.
- **Alternative: a delete and a size on `IFolders`.** Rejected: it would hand a general file delete to every holder of `IFolders`, the read models included, and grow every fake.
- **Alternative: a clear method on the sink (`DiagnosticFile`).** Rejected: the sink is `IDiagnosticSink`, which `DiagnosticLog` holds as a list of sinks, and reads belong to the setup reader, not the log.

### 2. Delete `cader.log` first, then `cader.log.1`

A rollover in another Cader moves `cader.log` over `cader.log.1`. Deleting `cader.log` first means a move landing between the two deletes finds no file (its `File.Move` throws `FileNotFoundException`, which `DiagnosticFile` already swallows), and a move that landed just before is undone by the second delete. A `cader.log` written after the clear by any process is new and holds only lines after it. The one outcome left is a file kept because it was open at that moment (an append in progress, which `AppendAllText` opens without `FileShare.Delete`, or another program); the outcome says so and the next `d` clears it.

### 3. The setup state carries both files' facts

`SetupState.LogFile(LogFiles Files)` (on) and `SetupState.LogOff(LogFiles Files)` (off) replace the path-and-flag shapes, so a log that is off still shows its size, which is when a person is most likely to clear it. A new `SetupItem.OlderDiagnosticLogFile` row with `SetupState.OlderLog(LogFiles Files)` is added by `ConfigRows` directly after the log row only when `Files.Older` is `Written`; while the setup is `Reading` or `Unreadable` there is no older row, so it never flickers. `SetupFactsReader` takes `IDiagnosticLogFiles` in place of the log path and its `IFolders`, which it used only to ask whether the log existed. `ConfigView.TryPathOf` reads `Files.Current.Path` and `Files.Older.Path`, and `OpenEffect` opens the file for both log items.

### 4. Sizes are words, not a Domain type

`Wording.FileSize(long bytes)`: below 1,048,576 bytes, whole kilobytes rounded up (`1 KB` for 300 bytes, `0 KB` only for an empty file, as Explorer shows them); from there megabytes to one decimal (`10.0 MB`), invariant culture, 1,024-based as `Wording.Memory` and Task Manager count. `Wording.Memory` is not reused: it rounds to whole megabytes, so a fresh log would read `0 MB`. The Diagnostic log file tooltip's "10 MiB" becomes "10 MB" so the tab uses one unit.

- **Alternative: a `FileSize` value in Domain.** Rejected for now: one number read by one port and words in one place; `MemorySize` is about process memory.

### 5. The key, the effect, the dialog

- **Reducer:** `d` on a log row whose state holds `LogFiles` with either file `Written` raises `ClearDiagnosticLog(LogFiles Files)` (a `ViewEffect, IDialogEffect`); with both `Absent` it explains `StatusMessage.NoLogToClear`; while `Reading` or `Unreadable` it explains `StatusMessage.LogFilesNotRead`; on every other row `StatusMessage.ClearsOnlyTheLog`, through `RowDecision.Explain` as `c` and `f` do.
- **Dialog flow:** `DialogEffects` gains `ConfirmClearLog`, shaped like `ConfirmCloseEmpty`: `Confirm.Ask` with the new `DialogName.ClearLog`, `Wording.ClearLogTitle`, `Wording.ClearLogQuestion(LogFiles)` (each existing file and its size, that the events kept for a problem report stay, and that a log that is on starts again with its next event) and `Wording.DeleteButtons`; Cancel shows `StatusMessage.Kept`, Delete dispatches the effect. The question's sizes are as the tab last read them; the handler acts on the disc as it is.
- **Handler:** `Application\Actions\ClearDiagnosticLogHandler(IDiagnosticLogFiles files, DiagnosticLog log)` calls `Clear`, records `DiagnosticLogCleared`, and answers `ClearDiagnosticLogOutcome`: `Cleared(int Files, long Bytes)` (Success), `NothingToClear` (Hint), `PartlyCleared(int Files, long Bytes, int Kept)` (Warning) or `NoneCleared(int Kept)` (Warning), so a clear that removed nothing never reads as if it had. `EffectPorts` gains `IDiagnosticLogFiles LogFiles`, and `Composition.Handlers` registers it.

### 6. The clear is recorded, numbers only

`Application\Diagnostics\DiagnosticLogCleared(int FilesRemoved, long BytesRemoved, int FilesKept)` with its `DiagnosticLine` arm (`diagnostic log cleared: 2 files, 13002282 bytes, 0 kept`). The handler records it before returning, so with the setting on it reaches the new `cader.log` ahead of the dispatcher's `EffectRan` (another thread or Cader may still write a line between the delete and it); a maintainer reading the file sees why it starts mid-run. `DiagnosticEventsShould` accepts it as it holds only numbers.

### 7. Wiring

`SharedParts` builds `DiagnosticLogFiles` over the same `logFile` as the sink and carries it; `ConsoleComposition` hands it to `EffectPorts` and `SetupFactsReader`. `tools\ReadmeScene\SceneHost` copies both constructor changes over its scene folder (CI builds it, so a missed change fails the build). Each constructor change lands with the wiring and test call sites it breaks (`SetupFactsReaderShould`, `ConsoleLoadsShould`, `EffectDispatcherShould`, `DiagnosticsPrivacyShould`), so no commit leaves the solution unbuildable. `Composition.Handlers` keeps its `diagnosticLogFile` string for `ToggleDiagnosticLogHandler`.

## Risks / Trade-offs

- [A file open in another program is kept] → the outcome names how many were kept and says to try again; the row still shows it.
- [The question's sizes may be stale by the time Delete is chosen] → the outcome reports what was actually deleted, from the disc.
- [With the log on, `cader.log` comes back at once] → the spec and the question say so; the new file starts with the clear's own line, which is the point.
- [The parallel change `make-the-diagnostic-log-useful` changes the line format and may touch `DiagnosticFile` and `DiagnosticLine`] → whichever merges second rebases; the new event's arm follows whatever format is on `main`.
- [The accounts changes add Config rows and `ConfigRows` cases] → a textual merge in `ConfigRows`, `ConfigView` and `WordingOfConfig`; no requirement overlaps.

## Deviations in implementation

- **Checked against the diagnostic log change (#303), which merged first:** `DiagnosticFile` still appends with `File.AppendAllText` and rolls to `path + ".1"`, now named once by `DiagnosticFile.OlderPath`; every `DiagnosticLine` now starts with the UTC time and `[pid]`, so the clear's line reads `<time> [pid] diagnostic log cleared: 2 files, 13002282 bytes, 0 kept`, its body as designed, and `DiagnosticEventsShould` already allows `long`. The diagnostics spec delta needed no change.
- **Tones:** there is no Warning tone. `PartlyCleared` and `NoneCleared` are Notice (the orange `!`), the tone of every other refused or partial outcome; `Cleared` is Success and `NothingToClear` Hint. The three new status messages (`NoLogToClear`, `LogFilesNotRead`, `ClearsOnlyTheLog`) are Hint.
- **`LogFileOnDisc(string Path)`:** the path is on the base record, so `Files.Current.Path` and `Files.Older.Path` read for either case.
- **The port grew in two steps:** `Read` in 2.1 and `Clear` in 2.2, so no commit carried a stub.
- **`FakeDiagnosticLogFiles(string path)`:** the fake takes the log's path (the parameterless one uses `TestSettings.DiagnosticLogFile`), so the privacy flow in 4.3 clears a log under a Northwind folder; `Fixtures.Placeholder` gained a `long`.
- **Red before Green across 4.1 and 4.2:** both Red commits came first and one Green followed, because `WordingOfOutcomesShould` and `StatusTonesShould` fail for any outcome case without words and a tone.
- **The reading fact:** `ConfigRowsShould.ShowTheSetupAsReadingBeforeItIsRead` now expects every setup item but the older log row, which never shows while the setup is read.
- **Left as it was:** the help's Report a problem section and the user guide still say `cader.log` rolls at 10 MiB, as the problem report text does; only the Config tab's tooltip moved to 10 MB. `Wording.Working` has no arm for the clear, which reads Working... for the moment the delete takes.
- **Spec:** the setup requirement's Diagnostic log file clause was shortened so the requirement stays under openspec's 500-character warning.
- **Version and checks:** 0.22.0-preview.22 to 0.22.0-preview.23, and both release asserts run against `logs-2-useful-diagnostic-log`, the branch this stacks on.
