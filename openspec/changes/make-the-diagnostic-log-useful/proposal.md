# Proposal

## Why

A real `cader.log` (12,431 lines over two days, unchanged in 0.22.0-preview.20) could not answer the questions it was turned on for. Several Cader processes wrote to it at once with nothing to tell their lines apart; one missing folder's failed git start filled 8,500 lines; 33 Terminal.Gui warnings said only `terminal.gui Warning`; and 590 UI stalls said how long but never what the UI thread was doing. The counts every 10 minutes were also read as per-interval figures when they cover the whole run.

## What Changes

- Every line (log file, crash file, problem report) names the process that wrote it by its process id, so interleaved consoles and headless runs can be told apart; the `started` line keeps the version, so a process id leads to its version.
- A child program that cannot be started is recorded once per command and folder until that command starts in that folder again, instead of on every refresh; the folder is only an in-memory key and is never written.
- Every exception summary of a Windows error (`Win32Exception`) carries Windows' error number (267 for a missing folder, 2 for a missing file), a number and never Windows' message.
- A Terminal.Gui warning or error names the Terminal.Gui method that raised it, taken from the call stack as a code name; the message is still never read.
- A UI stall line names the longest piece of marked UI-thread work since the beat before (a refresh landing, a key, an effect's outcome, a row's detail) and how long it took, or says nothing marked ran, which points at drawing or Terminal.Gui itself.
- Each count line says its figures are since the process started.

Out of scope:
- Clearing the log from the Config tab and showing `cader.log.1`: the parallel change `clear-the-diagnostic-log`.
- Missing session folders and git being started for them: the parallel change `mark-sessions-whose-folder-is-gone`. This change is the general guard against any repeated start failure.
- Making the worktree scan or the pull-request refresh faster; this change only makes their cost visible.
- Counting the repeats of a failed start, timing Terminal.Gui's own drawing, and per-interval counts.
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `diagnostics`: lines carry the process id (allowed as Cader's own number); failed starts are recorded once per command and folder with Windows' error number; Terminal.Gui reports name their raising method; stall lines name the work that ran; count lines say they are since start.

## Impact

- `Source191.Cader.Core`, `Application\Diagnostics`: `DiagnosticLine` (process id prefix, the stall and Terminal.Gui wording, "since start"), `ExceptionSummary` (the error number), `RecordedProcessRunner` (once per command and folder), `TerminalGuiBridge` (the raising method), `Heartbeat` (timed marks), and the `TerminalGuiReported` and stall events; `NullFreeModelsShould`'s Kept list gains one entry.
- `Source191.Cader.Tui`: `TableBody` and `DetailReader` run their UI-thread callbacks through the heartbeat's mark (one line each).
- No new package, port or setting; no change to the screen. `.claude\rules\security.md` (Diagnostics) and `.claude\rules\terminal-gui.md` gain what was verified.
- Version bump in `src\Source191.Cader\Source191.Cader.csproj` and a `CHANGELOG.md` line.
