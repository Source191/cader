# Proposal

## Why

With Diagnostic log on, `%LOCALAPPDATA%\Cader\cader.log` grows to 10 MB and then rolls to `cader.log.1`, so up to about 20 MB of history sits on the machine. Before reproducing a problem, or before handing the log to Claude or attaching it to an issue, a person wants a clean log that holds only the run in question. Today they must quit every Cader and delete the files in Explorer, the Config tab never shows `cader.log.1` at all, and nothing says how big either file is.

## What Changes

- **`d` on the Diagnostic log file row** asks to confirm, naming the files and their sizes, then deletes `cader.log` and `cader.log.1`. The status line says how much was deleted, that nothing was there to delete, or that a file in use by another program was kept. The in-memory events a problem report (`!`) carries are kept; only the files go. While Diagnostic log is on, the next event starts a new `cader.log`, so the file comes back at once holding only what happened after the clear.
- **A Diagnostic log file (older) row**, directly after the Diagnostic log file row, shows `cader.log.1` while it exists; `c` copies its path, `f` opens it in the editor and `d` clears both files as on the main row.
- **Each log row shows its file's size** (KB below 1 MB, MB to one decimal from there), whether the setting is on or off, so a person can see what a clear will remove.
- **`d` on any other Config row** says it clears only the diagnostic log and changes nothing.
- **The clear is recorded** as a diagnostic event of numbers only (files removed, bytes removed, files kept), so a fresh `cader.log` says why it starts mid-run.
- The key bar, the help, the row tooltips, the user guide and the changelog name the new key.

## Out of scope

- Changing what a log line says or how it is laid out (the parallel change `make-the-diagnostic-log-useful`).
- Deleting `last-crash.log`, or clearing the in-memory events a problem report carries.
- Changing the 10 MB cap or the rollover to `cader.log.1`.
- Multiple Claude accounts, which is a later change.
- Missing session folders and the worktree scan's speed, which have their own changes.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `config-tab`: the log rows show their size, the older log file gets a row of its own, the key bar gains `d`, and `d` clears the diagnostic log after confirming.
- `diagnostics`: the log files can be cleared without touching the in-memory log, and the clear is recorded.

## Impact

- **Ports and adapters:** a new narrow port `IDiagnosticLogFiles` (read both files' sizes, delete both) over the two fixed paths, implemented beside `DiagnosticFile` in `Adapters\Diagnostics`, with a fake in `TestDoubles`. `IFolders` and `IFolderDeleter` are unchanged.
- **Application:** `SetupState.LogFile` and `LogOff` carry both files' facts, a new `SetupItem.OlderDiagnosticLogFile`, `SetupFactsReader` reads the new port, a `ClearDiagnosticLog` dialog effect, `ClearDiagnosticLogHandler` and its outcome, a `DiagnosticLogCleared` event, `DialogName.ClearLog`, and `Wording` for the key, row values, sizes, dialog, outcomes and tones.
- **Tui:** one confirm flow in `DialogEffects`.
- **Wiring:** `SharedParts`, `ConsoleComposition` and `tools\ReadmeScene\SceneHost` (which copies the composition) build the port and hand it to `EffectPorts` and `SetupFactsReader`.
- **Docs and rules:** `docs\user-guide.md`, `CHANGELOG.md`, `.claude\rules\console-screen.md` and `.claude\rules\security.md`.
- **Version:** a preview bump of `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
