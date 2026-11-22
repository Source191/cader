# Tasks

**Implement with:** Opus 5.5 at high effort: a per-repository cache whose correctness rests on file-system change events, ordering across overlapping scans and the existing "a scan's rows never replace a later scan's" rule; a missed invalidation shows stale rows, so the invalidation rules need careful, adversarial tests.

Each task is red, green, refactor, with a commit per step. Inventory facts use the existing fixtures, `FakeTimeProvider` and a runner that records which repository each read ran in; change facts use `FakeFolderChanges`.

## 1. Change kinds per repository

- [ ] 1.1 `ChangeFilterShould.MarkAReferenceChangeStaleOnly` (`refs\heads\fix-123`, `packed-refs`, `FETCH_HEAD`: stale, no refresh) and `KeepWorktreeRecordsAndHeadAsARescan`; `ChangeFilter` answers a closed `ChangeEffect` (`None`, `StaleOnly`, `Rescan`) per change. Verify the existing `ChangeFilterShould` facts hold as `Rescan` or `None`. Commit.
- [ ] 1.2 `ChangeWatchesShould.ReportTheRepositoryOfAGitFolderChange` (the watch's folder's parent) and `MarkEveryRepositoryOnAnOverflow`, through a `StaleRepositories` holder (`Application`, take-and-clear behind a `Lock`). Commit.

## 2. Layout cache

- [ ] 2.1 `WorktreeInventoryRepositoriesShould.ReuseAnUnchangedRepositorysLayout` (second scan: no `worktree list` or `for-each-ref` in it) and `StillReadEachLiveWorktreesCounts`. Commit.
- [ ] 2.2 `WorktreeInventoryRepositoriesShould.ReadAStaleRepositoryAgain`, `ReadALayoutFiveMinutesOldAgain` and `ReadEveryRepositoryWhenAllAreMarked`. Commit.
- [ ] 2.3 `WorktreeInventoryLeftoversShould.JudgeOrphansAlikeFromCachedAndFreshLayouts` and `DropARepositoryWhoseFolderWentAndItsCacheEntry`. Commit.
- [ ] 2.4 `WorktreeInventoryRepositoriesShould.KeepAChangeHeardDuringAScanForTheNext`. Commit.
- [ ] 2.5 `GitCliIntegrationShould.MarkTheRepositoryForEveryCommandARowDependsOn`: in `TemporaryGitRoot`, run worktree add, remove and prune, switch, commit, fetch from a local remote, branch create and delete, and rebase, and assert each writes a file `ChangeFilter` answers `StaleOnly` or `Rescan` for (via a real `FolderChanges` watch). Record the list with git's version in `.claude\rules\windows.md`. Commit.

## 3. Merged counts

- [ ] 3.1 `GitBranchListingShould.ReadEachTipsCommit` (`%(objectname)`) and `WorktreeInventoryBranchesShould.CountCommitsAfterAMergedHeadOncePerTipAndHead`. Commit.

## 4. Console

- [ ] 4.1 `ConsoleLoadsShould.MarkEveryRepositoryForF5AndAnActionsReload`: the loads take a "full" flag the screen passes for F5 and `TableBody`'s reload after an effect; `ConsoleScreen.RefreshShown` and the reload pass it. Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0). Commit.

## 5. Release

- [ ] 5.1 `.claude\rules\console-screen.md`, Refreshing: the two tiers, the stale causes and the five-minute safety net. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of #379 and add a CHANGELOG `Unreleased` entry under Changed (the Worktrees tab rescans in a fraction of the time, re-reading only the repositories that changed). The worktrees scan count and git runs per hour before and after come from the owner's timing run at #379's checkpoint C, not from this pull request. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate rescan-only-what-changed`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
