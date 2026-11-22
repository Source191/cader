# Tasks

**Implement with:** Opus 5.5 at high effort: thread-ordering fixes across the UI thread, pool threads and Terminal.Gui's timer lock, each pinned by a sociable test where the logic can live in Application and checked in the headless terminal where it cannot.

Each task is red, green, refactor, with a commit per step. Never press a saving key, or a key that stops, deletes or opens a tab, when driving a real cader exe headless; those behaviours are proved with fakes and checked by the owner by hand.

## 1. Facts

- [ ] 1.1 `.claude\rules\terminal-gui.md`: the dialog-opened-inside-`Invoke` fact becomes observed (22 November 2026, Terminal.Gui 2.5.0 on .NET 11, a throwaway program in the headless terminal: a second thread's `app.Invoke` call returned only when the dialog closed, 4.1 s later). Commit.

## 2. Pure rules

- [ ] 2.1 `RunningEffectsShould.RefuseAnEqualEffectWhileItRuns`, `AllowAnotherRowsEffect`, `AllowItAgainOnceEnded` and `TellApartEffectsOfOneRow` (`StopSession` and `DeleteSession` on fix-123): `Application\RunningEffects` (design 2). Commit.
- [ ] 2.2 `StatusMessage.StillRunning(ViewEffect)`, its words in `Wording` from the effect's working words (`WordingShould.SayAnActionIsStillRunning`) and its tone in `Wording.ToneOf` (a line in `StatusTonesShould`). Commit.
- [ ] 2.3 `DetailRequestsShould.StartTheFirstReadAtOnce`, `HoldARequestWhileAReadRuns`, `StartOnlyTheLatestHeldRequestWhenTheReadEnds` and `StartNothingWhenTheSelectionClearedMeanwhile`: the one-at-a-time rule in `DetailRequests` (design 5). Commit.
- [ ] 2.4 `CurrentSettingsShould.ApplyTwoChangesInOrder` (two `UpdateAsync` calls racing over an `InMemoryState` whose write is held open by a `TaskCompletionSource`; both values end saved) and `ReplaceTheSnapshotOnlyAfterTheWrite`: `CurrentSettings.UpdateAsync` (design 7). Commit.
- [ ] 2.5 `ClaudeVersionWatchShould.AnswerUnknownUntilTheVersionArrives`, `AnswerTheVersionOnceItArrives`, `JudgeItAgainstTheMinimum` and `WaitAtMostTheStartUpTime` (`FakeTimeProvider`): `Application\ClaudeVersionWatch` (design 8). Commit.

## 3. Adapters

- [ ] 3.1 `GitCliShould.PassNoOptionalLocksOnEveryRead` (a theory over each read-only `IGit` method, reading the argument list from `FakeProcessRunner`) and `TakeLocksOnEveryWrite`: `--no-optional-locks` before the subcommand of every read-only call (design 6). Commit.

## 4. Handlers

- [ ] 4.1 `OpenTabHandlerShould.SwitchToAPendingTabInsteadOfOpeningAnother` and `SwitchToALiveTabTheRowDidNotKnowOf`: `OpenTabHandler` re-reads the registry for `LiveTab.None` (design 3). Commit.
- [ ] 4.2 `ToggleUpdateCheckHandlerShould.KeepADiagnosticLogToggleMadeMeanwhile` and `ToggleDiagnosticLogHandlerShould.KeepAnUpdateCheckToggleMadeMeanwhile`: both handlers use `UpdateAsync`; `SaveSettingsHandler` uses it with the dialog's snapshot. Commit.
- [ ] 4.3 `LaunchProgressShould.ReportWithoutCallingAnyone`: `LaunchProgress.Follow` and its listener removed, `Report` only replaces the step (design 1); the existing facts about `Shown` stay. Commit.

## 5. Screens

- [ ] 5.1 `ConsoleScreen.RefreshWhatIsDue` calls `FrameChanged` when `launchProgress.Latest` differs from the step last drawn; the `Follow` registrations in the constructor and `StopTimers` go. Commit.
- [ ] 5.2 `TableBody` takes `RunningEffects` through `ScreenContext` and calls `TryBegin` in `Dispatch` and `RunInBackground` (for a `Working(effect)`), showing `StillRunning` on a refusal and calling `End` in the settle; `ConsoleComposition` and `tools\ReadmeScene`'s `SceneHost` build one per console. Add `EffectDispatcherShould` or `DialogEffectsShould` coverage where a refusal is observable without Terminal.Gui. Commit.
- [ ] 5.3 `ConsoleScreen.Leave` asks through `Confirm.Ask` with `DialogName.Quit` while any tab is running (design 4); `Wording.QuitWhileRunningTitle`, `QuitWhileRunningQuestion` and `QuitWhileRunningButtons` with their facts in `WordingShould`, and `DialogName.Quit` with its `DiagnosticEventsShould` line if the enum is listed there. Commit.
- [ ] 5.4 `DetailReader.Reading` follows `DetailRequests`' one-at-a-time rule: a request asked while a read runs is held, and the read's end starts the latest one. Commit.
- [ ] 5.5 `ConsoleComposition` starts `VersionAsync` with no 5 s cancellation, builds a `ClaudeVersionWatch` over it, records the start with `WithinAsync(5 s)`, and builds `SettingsRows` from `Current`. Commit.

## 6. Check and release

- [ ] 6.1 In the headless terminal: start the console cold (first run after a build) and check the header reads Claude unknown and then the version; hold Down for two seconds on the Worktrees tab and check, with `Get-Process git` sampled during the run, that no more than one row's git reads (plus a scan's) run at once. Run the smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0). Commit any fix found.
- [ ] 6.2 Rerun the allocation-budget and handle-count facts in `Infrastructure.Tests` and the worktree scan's timing (`RefreshPart.WorktreesScan` in a diagnostic log on a machine with several worktrees) before and after `--no-optional-locks`, and note the figures in the pull request. Commit.
- [ ] 6.3 `.claude\rules\console-screen.md`: the launch's step is redrawn by the ticker; an equal effect is refused while it runs; q asks while an action runs; the pane reads one row at a time; the header's Claude version may arrive late. Commit.
- [ ] 6.4 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges, and add a CHANGELOG `Unreleased` entry under Fixed: a session being started no longer waits for a dialog; pressing a key twice no longer runs its action twice or opens two tabs; q asks before quitting mid-action; holding an arrow key no longer floods git; Cader's git status no longer holds a worktree's index lock; two quick toggles both stick; and a slow Claude version still reaches the header. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
