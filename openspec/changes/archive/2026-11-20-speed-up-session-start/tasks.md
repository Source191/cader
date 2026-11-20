# Tasks

**Implement with:** Opus 5.5 at high effort: a fetch shared between the wizard and the launch across threads, a hold on the refresh schedules that must always be released, and progress reported from a pool thread to the UI thread.

Each task is red, green, refactor, with a commit per step. Tests run the real `GitCli` and `ClaudeCli` over `FakeProcessRunner` (its `WhileRunning` holds a run open) and use `FakeTimeProvider` for every clock.

## 1. Launch steps

- [x] 1.1 `LaunchStepsShould.CountFiveStepsForAFetchedNewWorktree`, `CountFourWithoutAnUpdate`, `CountThreeWithoutANewWorktree` and `NumberEachStageInOrder`: `LaunchStage` (closed enum) and `LaunchSteps` with `Step(stage)` answering `LaunchStep(Name, Stage, Number, Count)` (design 1). Commit.
- [x] 1.2 `LaunchProgressShould.StartWithNoStep`, `KeepTheLatestStep`, `CallItsListenerOnEachStep`, `ShowTheStepForTheLaunchItNames` and `LeaveAnotherLaunchsMessageAlone`: `LaunchProgress` over `SharedSnapshot<LaunchStep>`, `Follow(Action)` and `Shown(StatusMessage)` (design 2). Commit.
- [x] 1.3 `WordingShould.SayALaunchStep` (one fact per stage, `fetching origin/main` and `pulling origin/main` naming the remote branch) and `StatusTonesShould` for `StatusMessage.Launching`: the new case, its words and its tone (design 2). Commit.
- [x] 1.4 `LaunchServiceShould.ReportEachStepItRuns` for the 5-, 4- and 3-step launches (the reported steps in order, read from a `LaunchProgress`), `ReportNoFetchStepWithoutOriginHead`, and `ReportNothingAfterAFailedStart`: `LaunchService` takes `LaunchProgress`, builds `LaunchSteps` and reports as each step begins; `LaunchOutcome.Listed` carries its steps. Commit.
- [x] 1.5 `StartSessionHandlerShould.ReportOpeningTheTabAsTheLastStep`: the handler reports `OpeningTab` with the listed outcome's steps before opening the tab. Commit.

## 2. Hold refreshes during a launch

- [x] 2.1 `LaunchHoldShould.BeHeldWhileAnyHoldIsOpen`, `BeReleasedWhenTheLastHoldEnds` and `ReleaseOnceWhenDisposedTwice`: `Application\LaunchHold` behind a `Lock` (design 3). Commit.
- [x] 2.2 `RefreshScheduleShould.NotBeDueWhileHeld` (timer and change), `NotBeDueForAChangeWhileHeld`, `KeepAChangeSeenWhileHeld` and `BeDueOnceTheHoldEnds`: `RefreshSchedule` takes the hold; the existing facts pass with an unheld hold. Commit.
- [x] 2.3 `StartSessionHandlerShould.HoldRefreshesWhileItLaunches` (held during `claude --bg`, read through `WhileRunning`), `ReleaseTheHoldAfterAFailedLaunch` and `ReleaseTheHoldWhenTheLaunchThrows`: the handler takes the hold in a `using`. Commit.

## 3. Fetch while the wizard is open

- [x] 3.1 `BaseFetchesShould.FetchOriginsDefaultBranchWhenBegun`, `StartNoSecondFetchWhileOneRuns`, `JoinARunningFetch`, `JoinAFetchThatSucceededWithinAMinute`, `FetchAgainAfterAMinute`, `FetchAgainAfterAFailedEarlyFetch`, `FetchAgainForAnotherRemoteBranch`, `FetchNothingWithoutOriginHead` and `NeverFaultWhenGitFails`: `Application\Launch\BaseFetches` (design 4). Commit.
- [x] 3.2 `BaseUpdaterShould.JoinAnEarlyFetch` and `FastForwardOnlyAfterTheLaunchAsks` (Pull with a finished early fetch: no `merge`, `fetch` or `branch -f` before `UpdateAsync`); `BaseUpdater` fetches through `BaseFetches`; the existing facts pass. Commit.
- [x] 3.3 `WizardViewShould.AskToFetchWhenTheRepositoryIsConfirmed` (Enter, Tab from the list, focus on a later field), `AskOncePerRepository`, `AskAgainForAnotherRepository`, `NotAskWhileTheCursorMoves`, `NotAskForCurrentBranchHeadOrNothing`, `AskWhenBeforeBranchingTurnsToFetchAfterConfirming`, `NotAskForAFolderChoice` and `NotAskOnAFixedFolder`: `WizardState`'s confirmed flag and last asked repository, and `WizardEffect.Prefetch(FolderPath)`. Commit.
- [x] 3.4 `LaunchWizardDialog` handles `WizardEffect.Prefetch` by calling `BaseFetches.Begin` and pushing the state as for `None` (Tui binder; covered by 3.3 and the smoke check). Commit.

## 4. Screen and wiring

- [x] 4.1 `ConsoleScreen.ShowStatus` passes each message through `LaunchProgress.Shown`, and registers `Follow` once to redraw the status line through `app.Invoke`. Commit.
- [x] 4.2 `Composition\LaunchParts` builds `LaunchProgress`, `LaunchHold` and `BaseFetches` once; `ConsoleComposition` hands them to `RefreshSchedule`, `StartSessionHandler`, `ConsoleScreen` and the wizard; `tools\ReadmeScene\SceneHost` copies the change. `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes`. Commit.
- [ ] 4.3 Smoke check of a local install (`tools\install-local.ps1`, then `dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`): exit 0. Then by hand, a launch in a scratch repository shows each step on the status line. Commit nothing unless a step fails.

## 5. Docs, version and archive

- [x] 5.1 `docs\user-guide.md`'s new-session section names the status line's steps and the early fetch; `.claude\rules\console-screen.md` records the hold on the refresh schedules and the launch progress holder. Commit.
- [x] 5.2 `Version` rises one preview step in `src\Source191.Cader\Source191.Cader.csproj`; `CHANGELOG.md` gets a Changed entry (the steps on the status line, refreshes waiting for a launch, the early fetch). Commit.
- [x] 5.3 `openspec validate speed-up-session-start --strict`, then archive the change in the same pull request. Commit.
- [ ] 5.4 After a day's use, compare `claude --bg`'s median and the `StartSession` effect time in `cader.log` against the proposal's figures, and record them in the pull request (design 5).
