## Why

The Diagnostic log row's tooltip is the only place the log file's path appears, and a tooltip cannot be selected or copied. A user who wants to read the log, or give it to Claude, has to retype the path. The Settings file row has a related gap: `f` opens its folder, not the file. The help also never says what a problem report (`!`) contains, or that `cader.log` is a separate, longer history that is not attached to it.

## What Changes

- Add a grey **Diagnostic log file** row to the Config tab's Setup rows, beside Settings file, showing the full path of `cader.log` all the time, with whether the log is on or off and whether the file exists yet. The path is plain text on screen, so it can be selected and copied from the terminal.
- Drop the path from the Diagnostic log row's tooltip; the row's value stays `on` or `off`.
- `f` on the Settings file and Diagnostic log file rows opens that file in the editor chosen in settings. A missing file is reported in the status line, with no editor started. `f` on the other path rows is unchanged.
- `c` on the Diagnostic log file row copies its full path, as it already does for Settings file.
- Say in the help and `docs\user-guide.md` what `!` includes (the latest in-memory events even when the file log is off, the last crash, the child program counts), that it never attaches `cader.log`, and that `cader.log` (rolls at 10 MiB) can be opened with `f` and handed to Claude or attached by hand.

Out of scope: attaching the log file to a report, any change to what a report holds, any change to how the log is written or rolled, opening Windows Terminal's fragment file (it keeps opening its folder), multiple Claude accounts (a later change).

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `config-tab`: the Setup rows gain Diagnostic log file; the Diagnostic log row's tooltip no longer names the path; `f` on a settings or log file row opens the file; the row counts change.
- `problem-reports`: the help and user guide say what a report holds and that the log file is separate.

## Impact

- `src\Source191.Cader.Core`: `SetupItem`, the setup state for the log file, `SetupFactsReader`, `ConfigRow.DiagnosticLog` (loses its `File`), `ConfigRows`, `ConfigView`, a new `OpenFileInEditor` effect and handler, `Wording*` (config words and key help).
- `src\Source191.Cader.Tui`: effect routing only if the new effect needs it.
- `docs\user-guide.md`, `.claude\rules\console-screen.md` (the `DiagnosticLog` row no longer carries the file) and the version in `src\Source191.Cader\Source191.Cader.csproj`.
- Overlaps in files with the in-flight `accounts-in-the-config-tab` change; whichever merges second rebases.
