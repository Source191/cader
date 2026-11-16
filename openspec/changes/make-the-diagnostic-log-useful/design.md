# Design

## Context

See proposal.md (Why). Every line is built by `Application\Diagnostics\DiagnosticLine` from a `TimedEvent`: `DiagnosticLog.Record` renders it for the sinks, and `CrashRecorder` and `ProblemReports` render the in-memory snapshot the same way, so one renderer is the one format. `DiagnosticEventsShould` allows an event only numbers, durations, versions, enums, `CodeName`, `ExceptionSummary` and `ChildCommand`, and `NullFreeModelsShould` keeps a nullable public member out of Core unless it is on its Kept list with a reason.

The four problems in the log come from four places:

- **Interleaved processes:** `DiagnosticLine.Stamp` writes only the UTC time.
- **8,500 "not started" lines:** `RecordedProcessRunner.GuardingStart` records `ProcessFailedToStart` on every throw, before `GitCli.RunAsync` turns `ERROR_DIRECTORY` (267) into exit -1, so a missing folder scanned every 30 seconds logs every 30 seconds. `ExceptionSummary` drops the message and with it the only place the error number showed.
- **Bare Terminal.Gui warnings:** `TerminalGuiBridge` reads only the level. Terminal.Gui 2.5.0's `Logging.Warning(message, [CallerMemberName] caller, [CallerFilePath] filePath)` puts the caller's file and method only inside the message text and calls `LogWarning(string)`, so the `EventId` is always 0 (decompiled with ilspycmd, 15 November 2026; the assembly holds no reference to `EventId` at all).
- **Unexplained stalls:** `Heartbeat.Beat` runs from a 250 ms timeout and sees only the gap. Work reaches the UI thread through `app.Invoke` in `TableBody.TriggerLoad` (a refresh's rows), `TableBody.RunInBackground` (an effect's outcome), `DetailReader.Read` (a row's detail) and keys through `TableBody.Apply`; Terminal.Gui draws after those callbacks return. The pull-request refresh never lands on its own: its answers are read by the next rows load.

## Goals / Non-Goals

**Goals:**
- Each change is small and lives where the fact is already in hand: the renderer, the runner, the bridge, the heartbeat.
- Nothing new can carry a path, a name or text; every new member is a number, a `CodeName` or a duration.

**Non-Goals:**
- No new port, setting, screen text or `Wording` entry: the lines stay a maintainer's format in `DiagnosticLine`.
- No timing of Terminal.Gui's drawing, layout or input parsing, and no per-interval counts.

## Decisions

### 1. The process id is part of every rendered line

`DiagnosticLine` prefixes every line, events and counts alike, with `[<pid>]` after the time: `2026-11-15T10:00:00.000Z [4120] effect StopSession -> Stopped in 312 ms`. It reads `Environment.ProcessId` itself (an expression-bodied member, not a static initialiser, which Stryker cannot attribute), because the id never changes within a process. Tests compare against `Environment.ProcessId`.

- **Alternative, injecting it through `DiagnosticLog`:** rejected. It changes 24 constructor calls across 22 test files and `SceneHost` for a value that is constant per process.
- **Alternative, adding it in the file sink:** rejected. The file's line would then differ from the in-memory line the spec says it repeats, and the crash file would lack it.
- **Consequence:** the crash file and a problem report carry the id too. It is Cader's own number, which the modified privacy requirement allows. `started` already carries the version; both the console and `cader here` record it (`ConsoleComposition`, `HereComposition`), so every process's id leads to its version.

### 2. A failed start is recorded once per command and folder until it starts there

`RecordedProcessRunner` keeps a `ConcurrentDictionary<(ChildCommand, WorkingFolder), byte>` of failed starts (it is called from pool threads). `GuardingStart` records `ProcessFailedToStart` only when `TryAdd` succeeds. Any run that started (it returned an exit code or timed out) does `TryRemove` for its key, so a folder that comes back and goes again is reported again. This is `OncePerFolder`'s pattern; the folder is a key in memory and never reaches the event.

- **The key compares `WorkingFolder` by record equality:** the path text as the caller passed it, ordinal. This is a de-duplication key, not a decision about a folder (`FolderPath` stays the one place that decides two paths are the same folder). The callers pass the same text every scan, and the worst case of a differently cased path is one extra line. `WorkingFolder.Inherit` is a key like any other.
- **The error number is not part of the key:** a second, different failure of the same command in the same folder before it ever starts is not logged. That is rare, and keeping it out keeps the key at the request's own two facts.
- **Repeats leave no trace:** a failed start was never counted (the exception passes `counts.Add`), and it still is not. Counting it as a pseudo exit code was rejected as a change to the counts' meaning. The parallel change `mark-sessions-whose-folder-is-gone` removes the main source.
- **Growth:** the dictionary holds one entry per distinct failing command and folder, bounded by the folders Cader is configured to scan.

### 3. Windows' error number goes in the exception summary

`ExceptionSummary.Part` writes `Win32Exception (error 267)` for a `System.ComponentModel.Win32Exception`, from `NativeErrorCode`, at any depth of the chain. The not-started line reads `not started git rev-parse: Win32Exception (error 267) at Process.StartWithCreateProcess < ...`, and every other summary of a Windows error gains its number (the process registry's, claude's, the pull-request listings').

- **Alternative, a new number member on `ProcessFailedToStart`:** rejected. It would be nullable for every other exception, and it would serve one event where the summary serves all.
- **Alternative, `HResult` for every exception:** rejected. For managed exceptions it is a constant per type and adds nothing to the type name.

### 4. The bridge names the Terminal.Gui method from the stack

`TerminalGuiBridge.Log` walks `new StackTrace(fNeedFileInfo: false)` and skips every frame of the bridge itself, of a type whose namespace starts with `Microsoft.Extensions.Logging`, and of the type named `Terminal.Gui.App.Logging`. Core cannot reference Terminal.Gui (`LayeringShould`), so these are matched by name as constants, as `ExceptionSummary` matches its await machinery. The first frame left is the raiser, named with `CodeName.For(MethodBase)`, which already names lambdas and async methods by the method they were written in.

- **The event:** `TerminalGuiReported(DiagnosticLevel Level, CodeName? Raiser, ExceptionSummary? Failure)`. `Raiser` is null only when no frame is left (trimmed or native frames), which gets a Kept entry in `NullFreeModelsShould` beside `Failure`'s. The line reads, for example, `terminal.gui Warning from AnsiResponseParser.ReleaseHeld` (an illustrative name), followed by `: <summary>` when there is an exception.
- **No `EventId`:** Terminal.Gui never sets one, so recording it would only ever say 0.
- **Cost:** one `StackTrace` per warning, and warnings are rare (33 in two days).

### 5. The heartbeat times marked UI-thread work

`Heartbeat` gains `Mark(CodeName work, Action run)`. It runs `run` in a `try`/`finally`, times it with the same `TimeProvider` timestamps, and keeps the longest marked piece since the previous beat. `Beat` judges as today and then clears it, so work from before an on-time beat is never blamed. It runs on the UI thread only, so it needs no lock.

A piece is timed from the later of its own start and the last beat, not from its start. `TableBody.Apply` shows a dialog synchronously, the dialog runs its own loop, and the heartbeat keeps beating inside it (`terminal-gui.md`). The key's mark therefore stays open for the dialog's whole life, and timed from its start it would close at 30 seconds and be blamed for the next freeze. Marks also nest, because a dialog's answer comes back through `Reduce` into `Apply`. Each mark keeps its own start in a local, so nesting needs no stack. An outer piece includes the inner ones, so it is the one named when it is the longest. A freeze is recorded as one of two events, so no member is null:

- `UiThreadStalled(Lateness)` when nothing was marked: `stalled ui thread for 700 ms, no marked work`
- `UiThreadStalledRunning(Lateness, CodeName Work, TimeSpan Took)`: `stalled ui thread for 820 ms, longest marked work WorktreeRowsLoaded 610 ms`

The marks are one line each in the Tui, named with names that already exist:

- **`TableBody.Apply`:** `CodeName.For(input.GetType())`, which covers a refresh's rows (`RowsLoaded`, `WorktreeRowsLoaded`, `ConfigRowsLoaded`), a failed load, keys (`KeyPressed`) and dialog answers. The tab switch a key causes runs inside it.
- **`TableBody.RunInBackground`'s settle:** `Attempt.What(working)`, the effect's type.
- **`DetailReader`'s `Show`:** `CodeName.For(typeof(DetailContent))`. `DetailReader.For` is handed the heartbeat beside the log it already takes.

Longest rather than last: a slow rows load followed by a cheap detail would otherwise blame the detail. A lateness well above the longest marked piece points at drawing or unmarked work. The pull-request refresh cannot be marked, because it lands through the next rows load; a burst of stalls during a slow `az` call with little marked work points at contention off the UI thread.

- **Alternative, a mark without timing:** rejected for the last-wins problem above.
- **Alternative, wrapping every `app.Invoke`:** rejected. Terminal.Gui's own callers and dialogs would need their own wrapper, for little more.

### 6. Count lines say "since start"

The counts were read as per-interval figures, so each count line says `since start`: `counts since start git status exit 0: 70 runs, ...`, `refreshes since start sessions listing: ...` and `stalls since start ui thread: ...`. The words go on every line, not in a header line, because `ProblemReports.CountLines` pairs each snapshot entry with its rendered line one to one. The process's `started` line, found by its id, gives the start time.

- **Alternative, per-interval counts:** rejected. They would reset the samples, change what the crash file and reports mean, and need a second set of figures.

## Risks / Trade-offs

- **[A problem report fits its lines into a 7,500-character address]** → `[pid] ` (about 8 characters) on every line and `since start` on each count line mean a few fewer lines fit in a report, though a report comes from one process. This is accepted so that every rendering stays one format. `ProblemReport.FitWithin` drops lines in order of their band, so the failures are kept first as before.
- **[A failure that keeps repeating leaves one line]** → That line can age out of the 500 events in memory, and so out of a problem report, while the failure goes on. `cader.log` keeps it.
- **[A process id is reused later]** → The `started` line and the times show where one process ends and the next begins.
- **[The JIT inlines the raising method]** → Its caller is named instead. This is still a Terminal.Gui method near the cause, and Release builds keep most frames of non-trivial methods.
- **[A failed start that keeps failing shows once]** → That is the point. A start in the same folder clears it, and the separate missing-folder change shows the user the cause.
- **[Old lines have no process id]** → The format is the maintainer's. A log that spans the upgrade shows where the new lines begin, and nothing parses it.
- **[A parallel change edits the same `diagnostics` spec]** → Today `clear-the-diagnostic-log` only ADDs to it (its clear line gains the process id like any other line), and `mark-sessions-whose-folder-is-gone` does not touch it. If either later MODIFIES a requirement this change modifies, whichever archives second rebases its block on the archived text.

## Migration Plan

None. The lines are written as before with more facts in them. `cader.log` is appended to, not rewritten, and rolling back restores the old format.
