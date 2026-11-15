# Tasks

Each task is red, green, refactor, a commit per step; "verify" means the Core suite (`dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build`) after `dotnet build -c Debug`, unless the task says otherwise. A rule file's sentence is written in the green step it describes.

## 1. The line format (Application, `DiagnosticLine`)

- [x] 1.1 Red `DiagnosticLineShould.PutTheProcessIdAfterTheTime` (an event line and a count line read `<time> [<Environment.ProcessId>] ...`), then green a process-id part in `DiagnosticLine`'s stamp, read from `Environment.ProcessId` in an expression-bodied member (design 1). Update the exact-line expectations this changes (`DiagnosticLineShould`, `CrashRecorderShould`, `ProblemReportsShould`, `DiagnosticsPrivacyShould` and any others the suite names), and add to `.claude\rules\security.md`'s Diagnostics section that each line carries Cader's own process id after its time, the one id a line may hold. Verify, including `DiagnosticLineShould.RecogniseACrashLineWhoseTextStartsAtTheCrashMarkerAndOneThatEndsAtIt` and `ProblemReportShould`'s fitting facts.
- [x] 1.2 Red `DiagnosticLineShould.SayCountsAreSinceStart` (a child program, a refresh part and the stalls line each read `since start`), then green the three count renderings (design 6), and say in `security.md` that the counts are since the process started. Verify, including `ProblemReportsShould`, whose count lines pair one to one with the snapshot.

## 2. Windows' error number (Application, `ExceptionSummary`)

- [x] 2.1 Red `ExceptionSummaryShould.NameAWindowsErrorByItsNumber` (an `InvalidOperationException` wrapping `new Win32Exception(2)` summarises as `InvalidOperationException ...; inner Win32Exception (error 2) ...`, and Windows' message for error 2 is absent), then green the number in `ExceptionSummary.Part` (design 3), and add Windows' error number to `security.md`'s list of what an `ExceptionSummary` holds. Verify.

## 3. A failed start once per command and folder (Application, `RecordedProcessRunner`)

- [x] 3.1 Red `RecordedProcessRunnerShould.RecordAFailedStartOncePerCommandAndFolder` (the inner runner throws `new Win32Exception(267)` for `git rev-parse` in `C:\Users\me\source\worktrees\Northwind.Orders-fix-123` three times: one `ProcessFailedToStart` line, holding `error 267`, and each call still throws), then green the failed-start keys in `RecordedProcessRunner.GuardingStart` (design 2). Extend the test's runner wrapper or `FakeProcessRunner` to throw for a request if neither can yet. Add to `security.md` that a failed start is recorded once per command and folder until it starts there, with the folder only an in-memory key. Verify.
- [x] 3.2 Red `RecordedProcessRunnerShould.RecordANewFolderOrCommandAgain` (the same failure in a second folder, and `git status` in the first, each add a line), then green if needed. Verify.
- [x] 3.3 Red `RecordedProcessRunnerShould.RecordAgainAfterTheCommandStartedThere` (fail, exit 0, fail gives two lines; fail, time out, fail gives two not-started lines and one timed-out line), then green the removal of the key on any start. Verify.
- [x] 3.4 Red `DiagnosticsPrivacyShould.KeepAFailedStartsFolderOutOfTheLog` (git fails to start in the Northwind fixture folder, and none of the seeded values is in any rendered line), then green if needed. Verify.

## 4. Terminal.Gui's raising method (Application, `TerminalGuiBridge`)

- [x] 4.1 Red `TerminalGuiBridgeShould.NameTheTerminalGuiMethodThatWarned`: push the bridge with `Terminal.Gui.App.Logging.PushLogger`, call `Logging.Warning("Northwind.Orders")` from a `[MethodImpl(MethodImplOptions.NoInlining)]` helper, and expect the line `terminal.gui Warning from TerminalGuiBridgeShould.<helper>` with neither "Northwind" nor `Logging.Warning` in it. Then green `TerminalGuiReported.Raiser` and the stack walk in `TerminalGuiBridge.Log` (design 4), its line in `DiagnosticLine`, and the Kept entry for `Raiser` in `NullFreeModelsShould` with its one-line reason. Update `security.md`'s `TerminalGuiBridge` sentence to say it also keeps the raising method's code name. Verify, including `DiagnosticEventsShould` and `NullFreeModelsShould.KeepNoStaleEntry`.
- [x] 4.2 Red `TerminalGuiBridgeShould.NameTheCallerOfALoggerExtensionWithItsFailure` (`bridge.LogError(exception, "...")` from a helper gives `terminal.gui Error from ...<helper>: <summary>`), then green if needed. Verify, including the existing `TerminalGuiBridgeShould` facts.
- [x] 4.3 Record in `.claude\rules\terminal-gui.md`, dated 15 November 2026 and read from the decompiled 2.5.0 assembly (ilspycmd), that `Logging.Warning` and its siblings put the caller's file and member only inside the message and call `LogWarning(string)`, so the `EventId` is always 0 (the assembly references no `EventId`), and that the bridge therefore reads the raiser from the stack. Verify the fact is in the file and `dotnet format --verify-no-changes` is clean.

## 5. Stall attribution (Application, `Heartbeat`)

- [ ] 5.1 Red `HeartbeatShould.NameTheLongestMarkedWorkInAFreeze` (marks of `WorktreeRowsLoaded` taking 610 ms and `KeyPressed` taking 5 ms on the fake clock, then a beat 820 ms late: one `UiThreadStalledRunning` naming `WorktreeRowsLoaded` and 610 ms), then green `Heartbeat.Mark`, `UiThreadStalledRunning` and its line in `DiagnosticLine` (design 5), and add the marks to `security.md`'s Diagnostics section (each a `CodeName` and a duration). Verify, including `DiagnosticEventsShould`.
- [ ] 5.2 Red `HeartbeatShould.SayNoMarkedWorkRan` and `ForgetWorkMarkedBeforeAnOnTimeBeat`, plus `DiagnosticLineShould.RenderAStallWithNoMarkedWork` (`stalled ui thread for 700 ms, no marked work`), then green the clearing in `Beat` and the changed line. Verify, including the existing `HeartbeatShould` facts (counts unchanged, sleep ignored).
- [ ] 5.3 Red `HeartbeatShould.TimeAMarkOnlySinceTheLastBeat` (a mark whose action beats on time 120 times over 30 seconds, as a dialog does, and then works 400 ms before returning, followed by a beat 600 ms late: the stall names the mark with 400 ms, not 30 seconds) and `HeartbeatShould.NameTheOuterMarkOfNestedWork` (a mark inside a mark), then green the start taken as the later of the mark's start and the last beat. Verify.

## 6. The marks on the UI thread (Tui)

- [ ] 6.1 Run `TableBody.Apply`'s body through `context.Heartbeat.Mark(CodeName.For(input.GetType()), ...)`, `RunInBackground`'s `app.Invoke` body through `Mark(Attempt.What(working), ...)`, and `DetailReader`'s `Show` through `Mark(CodeName.For(typeof(DetailContent)), ...)`, handing `DetailReader<TRow>.For` the heartbeat. These are one-line screen wiring with the logic tested in group 5. Verify that `dotnet build -c Debug` passes with warnings as errors and that the full `dotnet test -c Debug --no-build` passes, then that the smoke check passes on both architectures in CI's `check` jobs, which open every screen and dialog through these marks.

## 7. Release housekeeping

- [ ] 7.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` with `.github\scripts\Step-Version.ps1`. Add a `## Unreleased` `### Changed` line to `CHANGELOG.md` saying the diagnostic log now names the process on every line, records a program that cannot start once per folder with Windows' error number, names the Terminal.Gui method behind a warning and the work behind a UI stall, and marks its counts as since start. Check that `docs\user-guide.md`'s privacy sentence ("numbers, durations, versions and names of its own") still holds, which it does because the process id is a number, and leave it unchanged. Verify that `.github\scripts\Assert-VersionBumped.ps1 -Base main -Head HEAD` and the changelog check pass, and that `dotnet format --verify-no-changes` is clean.
- [ ] 7.2 Run `openspec validate make-the-diagnostic-log-useful --strict` and verify it passes.

## Workflow follow-up

- Archive the change in the same pull request, once its tasks are done, so `openspec\specs` matches `main`. If a parallel change archived first and modified one of the same `diagnostics` requirements (none does today), rebase this change's MODIFIED blocks on the archived text before archiving.
- After a day's use with the Diagnostic log on, read a fresh `cader.log`: group the stall lines by their named work and the Terminal.Gui lines by their method, and open a change for whichever dominates.
