# Tasks

**Implement with:** Opus 5.5 at high effort: thread-ordering fixes across the UI thread, pool threads and Terminal.Gui's timer lock, each pinned by a sociable test where the logic can live in Application and checked in the headless terminal where it cannot.

Each task is red, green, refactor, with a commit per step. Never press a saving key, or a key that stops, deletes or opens a tab, when driving a real cader exe headless; those behaviours are proved with fakes and checked by the owner by hand (the implementation plan, checkpoint A).

## 1. Facts

- [x] 1.1 `.claude\rules\terminal-gui.md`: the dialog-opened-inside-`Invoke` fact becomes observed (22 November 2026, Terminal.Gui 2.5.0 on .NET 11, a throwaway program in the headless terminal: a second thread's `app.Invoke` call returned only when the dialog closed, 4.1 s later). Commit.

## 2. Pure rules

- [x] 2.1 `RunningEffectsShould.RefuseAnEqualEffectWhileItRuns`, `AllowAnotherRowsEffect`, `AllowItAgainOnceEnded` and `TellApartEffectsOfOneRow` (`StopSession` and `DeleteSession` on fix-123): `Application\RunningEffects` (design 2). Commit.
- [x] 2.2 `StatusMessage.StillRunning(ViewEffect)`, its words in `Wording` from the effect's working words (`WordingShould.SayAnActionIsStillRunning`) and its tone in `Wording.ToneOf` (a line in `StatusTonesShould`). Commit.
- [x] 2.3 `CurrentSettingsShould.ApplyTwoChangesInOrder` (two `UpdateAsync` calls racing over an `InMemoryState` whose write is held open by a `TaskCompletionSource`; both values end saved) and `ReplaceTheSnapshotOnlyAfterTheWrite`: `CurrentSettings.UpdateAsync` (design 5). Commit.

## 3. Handlers

- [x] 3.1 `OpenTabHandlerShould.SwitchToAPendingTabInsteadOfOpeningAnother` and `SwitchToALiveTabTheRowDidNotKnowOf`: `OpenTabHandler` re-reads the registry for `LiveTab.None` (design 3). Commit.
- [x] 3.2 `ToggleUpdateCheckHandlerShould.KeepADiagnosticLogToggleMadeMeanwhile` and `ToggleDiagnosticLogHandlerShould.KeepAnUpdateCheckToggleMadeMeanwhile`: both handlers use `UpdateAsync`; `SaveSettingsHandler` uses it with the dialog's snapshot. Commit.
- [x] 3.3 `LaunchProgressShould.ReportWithoutCallingAnyone`: `LaunchProgress.Follow` and its listener removed, `Report` only replaces the step (design 1); the existing facts about `Shown` stay. Commit.

## 4. Screens

- [x] 4.1 `ConsoleScreen.RefreshWhatIsDue` calls `FrameChanged` when `launchProgress.Latest` differs from the step last drawn; the `Follow` registrations in the constructor and `StopTimers` go. Commit.
- [x] 4.2 `TableBody` takes `RunningEffects` through `ScreenContext` and calls `TryBegin` in `Dispatch` and `RunInBackground` (for a `Working(effect)`), showing `StillRunning` on a refusal and calling `End` in the settle; `ConsoleComposition` and `tools\ReadmeScene`'s `SceneHost` build one per console. Add `EffectDispatcherShould` or `DialogEffectsShould` coverage where a refusal is observable without Terminal.Gui. Commit.
- [x] 4.3 `ConsoleScreen.Leave` asks through `Confirm.Ask` with `DialogName.Quit` while any tab is running (design 4); `Wording.QuitWhileRunningTitle`, `QuitWhileRunningQuestion` and `QuitWhileRunningButtons` with their facts in `WordingShould`, and `DialogName.Quit` with its `DiagnosticEventsShould` line if the enum is listed there. Commit.

## 5. Check and release

- [x] 5.1 Run the smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0). Commit any fix found.
- [x] 5.2 `.claude\rules\console-screen.md`: the launch's step is redrawn by the ticker; an equal effect is refused while it runs; q asks while an action runs. Commit.
- [x] 5.3 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of the implementation plan, and add a CHANGELOG `Unreleased` entry under Fixed: a session being started no longer waits for a dialog; pressing a key twice no longer runs its action twice or opens two tabs; q asks before quitting mid-action; and two quick toggles both stick. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate guard-overlapping-actions`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
