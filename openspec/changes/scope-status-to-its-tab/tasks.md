# Tasks

**Implement with:** Opus 5.5 at medium effort: a pure classification and a small, clock-driven decision over immutable state, with the existing status-line rendering left untouched.

Each task is red, green, refactor, with a commit per step. Tests are sociable over the real `StatusMessage` cases, effects and `Wording`, and use `FakeTimeProvider` for every time.

## 1. Classification

- [ ] 1.1 `StatusScopesShould.ScopeEveryCase` (a theory table naming every `StatusMessage` case with its scope, design 1) and `StatusScopesShould.NameEveryCaseInTheTable` (reflection over the hierarchy, failing for a case the table leaves out): `StatusScope` (closed enum) and `StatusScopes.Of` with its fallback arm. Verify both pass and the second fails when a case is removed from the table. Commit.
- [ ] 1.2 `StatusScopesShould.ScopeWorkingAndReportedByTheirEffect`: `Working(StartSession)`, `Working(SaveSettings)` and each other console-wide effect's `Working` and `Reported` answer `Console`; `Working(CopyFolderPath)`, `Working(RemoveWorktree)`-style tab effects and their outcomes answer `Tab`. Commit.

## 2. Status line rules

- [ ] 2.1 `StatusLineShould.ShowTheShownTabsMessage`, `ShowNothingWhenNoTabHasAMessage` and `NeverShowAHiddenTabsOwnMessage`: `StatusLine` with `Seen`, `Left` and `Choose` answering `StatusChoice(Message, Owner)`. Commit.
- [ ] 2.2 `StatusLineShould.ShowAnotherTabsConsoleMessage`, `PreferTheNewerMessage`, `PreferTheShownTabOnATie` and `ShowTheConsoleMessageAgainOnceTheNewerClears`, with `FakeTimeProvider` advancing between `Seen` calls. Commit.
- [ ] 2.3 `StatusLineShould.KeepASameInstancesFirstSeenTime` and `RestampANewInstanceWithTheSameWords` (by reference, as `ShowStatus` compares today). Commit.
- [ ] 2.4 `StatusLineShould.KeepAMessageOnAQuickReturn` (back within `StatusLayout.SuccessShownFor`), `DropAMessageOnALateReturn`, `KeepAMessageThatArrivedWhileHidden`, `KeepARunningActionsMessage` and `KeepAConsoleMessage`: `KeepsOnReturn`. Commit.

## 3. Screen wiring

- [ ] 3.1 `ConsoleScreen.Show` calls `Left` for the tab it leaves and reduces `MessageDismissed` in the shown tab when `KeepsOnReturn` is false; `FrameChanged` calls `Seen` for all three tabs and `ShowStatus` takes `Choose`'s message and owner (the owner replacing `Active` in `StatusShown.Message`, its `Running` driving the spinner). Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0) and by hand against the spec's scenarios in a scratch build started from a clean environment. Commit.
- [ ] 3.2 `.claude\rules\console-screen.md`, Status line section: the line shows `StatusLine.Choose`'s message, console-wide messages follow every tab, and a tab's own message returns only while recent. Commit.

## 4. Release

- [ ] 4.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` to one preview step above what `main` holds when this merges (expected `0.22.0-preview.54` after #372), and add a CHANGELOG `Unreleased` entry under Changed: a launch's progress, update notices, settings saves and background failures now stay on the status line whichever tab is shown, and a tab's own message no longer comes back when you return to the tab more than ten seconds after leaving it. Verify `.github\scripts\Assert-VersionBumped.ps1` passes, `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
