# Tasks

**Implement with:** Opus 5.5 at high effort: it changes concurrency in the scan and the enrichment, where the merge order, the orphan rule ("judged against its own registrations and those before it"), cancellation and the eight-slot limit must all hold, and mistakes show as rare, order-dependent failures.

Each task is red, green, refactor, with a commit per step. Concurrency facts use a scripted runner whose reads can be held and released in a chosen order, and assert both the rows and the most reads seen at once.

## 1. Reads in the pool

- [ ] 1.1 `WorktreeInspectorShould.RunACountsReadsTogetherInThePool`: the three (after `lighter-git-reads`, two) reads start together, each in its own slot, and the counts are as before. Commit.
- [ ] 1.2 `WorktreeInventoryBranchesShould.CountUnusedBranchesTogether` and `KeepTheBranchRowsInListingOrder`. Commit.
- [ ] 1.3 `WorktreeInventoryBranchesShould.CheckMergedPullRequestsInThePool` (never more than eight reads at once) and `KeepTheShownOrder`. Commit.
- [ ] 1.4 `WorktreeInventoryLeftoversShould.ListTheSameOrphansWhenReadsFinishInReverseOrder` and `StartNoReadAfterTheScanIsCancelled`. Commit.

## 2. Sessions at the merge

- [ ] 2.1 `WorktreeInventoryRepositoriesShould.ReadGitBeforeTheSessionsArrive` (a sessions task left pending: the git reads all run) and `MarkSessionsOnceTheyArrive`: `ScanAsync` takes a task of sessions. Commit.
- [ ] 2.2 `ConsoleLoadsShould.StartTheFirstScanBeforeTheFirstListingReturns` and `TimeTheScanWithoutTheWaitForTheListing` (the refresh count holds the git time only). Verify `LatestSessionsShould` still passes. Commit.

## 3. Enrichment in parallel

- [ ] 3.1 `SessionRowEnricherShould.LocateUpToEightFoldersAtOnce`, `LocateAFolderSharedByTwoSessionsOnce` and `KeepTheListingsOrder`. Commit.

## 4. Release

- [ ] 4.1 `.claude\rules\console-screen.md`, Refreshing: the start-up scan reads git at once and waits for the listing only to mark sessions. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of the implementation plan and add a CHANGELOG `Unreleased` entry under Changed (the worktrees appear sooner after start and on each scan). The worktrees scan and sessions enrichment counts before and after come from the owner's timing run at the implementation plan's checkpoint C, not from this pull request. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate parallel-scan-reads`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
