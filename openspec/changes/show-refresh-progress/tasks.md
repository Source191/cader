# Tasks

**Implement with:** Opus 5.5 at high effort: partial results from one scan must fit the existing ordering rules ("a scan's rows never replace those of a later scan", the selection kept across a refresh, the filter, the orphan merge in name order), and the refresh mark touches the console's layout code, which has known Terminal.Gui pitfalls (`terminal-gui.md`).

Each task is red, green, refactor, with a commit per step.

## 1. Scan progress

- [ ] 1.1 `WorktreeInventoryRepositoriesShould.ReportTheRowsOfEachPrefixReadInNameOrder` and `ReportNothingWhileTheFirstRepositoryIsUnread`, with a runner that releases repositories out of order; `ScanProgress` (rows, read, to read). Commit.
- [ ] 1.2 `WorktreeInventoryLeftoversShould.LeaveWorktreeRootLeftoversToTheFinalRows`: a prefix holds no broken worktree or empty folder under the worktree root. Commit.

## 2. Reducer and wording

- [ ] 2.1 `WorktreesViewShould.ShowPartialRowsOnlyWhileUnloaded`, `KeepTheSelectionFromPartialToFinalRows` and `IgnorePartialRowsOnceLoaded`. Commit.
- [ ] 2.2 `WordingOfScreensShould.CountTheRepositoriesRead` (the waiting words and the title's progress) and `WordingShould.MarkARefreshDim`. Commit.

## 3. Screen

- [ ] 3.1 `LoadSequenceShould.AcceptAPartialInputOnlyForTheNewestTicket` and `DropAPartialInputAfterItsFinal`; `TableBody` hands a progress that posts partial inputs with the load's ticket. Commit.
- [ ] 3.2 `ConsoleScreen.FrameChanged` appends the mark while the shown tab's `RunningLoads` holds a running load, and a load's start and end redraw the frame. Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0) and a capture of the first scan filling from the top in a scratch build. Commit.

## 4. Release

- [ ] 4.1 `.claude\rules\console-screen.md`, Layout and Refreshing; `docs\user-guide.md`, where it describes the waiting words. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges and add a CHANGELOG `Unreleased` entry under Changed (the first scan fills the Worktrees tab from the top; a ↻ marks a refresh in progress). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate show-refresh-progress`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
