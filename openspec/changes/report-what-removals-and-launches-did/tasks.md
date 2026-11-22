# Tasks

**Implement with:** Sonnet 5.5 at medium effort: two contained fixes in Application handlers with sociable tests over the existing fakes, plus one real-git adapter fact.

Each task is red, green, refactor, with a commit per step. Tests drive the real `GitCli` and `ClaudeCli` over `FakeProcessRunner`, with `InMemoryState`, `FakeFolders` and `FakeTimeProvider`, as `RemoveWorktreeHandlerShould` and `LaunchServiceShould` do.

## 1. Removal

- [ ] 1.1 `Infrastructure.Tests\Git\GitBranchDeletionShould.RefuseDashDForAMergedBranchWhoseUpstreamWasPruned` and `ForceDeleteThatBranch` over `TemporaryGitRoot` (a bare origin, a clone, a branch pushed with `-u`, merged into origin's main, deleted on origin and pruned, local main behind): pins the git behaviour of design Context. Commit.
- [ ] 1.2 `RemoveWorktreeHandlerShould.JudgeMergedAfterTheWorktreeIsRemoved` (the fake runner's log shows `merge-base --is-ancestor` after `worktree remove`) and `DeleteAMergedBranchItCreatedWithForce` (`branch -D`): `WorktreeRemover.CarryOutAsync` drops its `merged` parameter and checks after git's removal; `RemoveWorktreeHandler` drops its own check (design 1, 2). Commit.
- [ ] 1.3 `DeleteSessionHandlerShould.JudgeMergedAfterTheWorktreeIsRemoved` and `RemoveTheWorktreeAndBranchOfAPushedMergedSession`: `DeleteSessionHandler` drops its `IsMergedAsync` call. Commit.
- [ ] 1.4 `RemoveWorktreeHandlerShould.ReportTheRemovalWhenGitRefusesTheBranch`: `RemoveWorktreeOutcome.RemovedBranchNotDeleted` (design 3), with its words in `WordingOfOutcomes` (`WordingOfOutcomesShould.SayTheWorktreeWasRemovedAndTheBranchKept`) and its tone in `Wording.ToneOf` (a line in `StatusTonesShould`). Commit.
- [ ] 1.5 `DeleteSessionHandlerShould.ReportTheRemovalWhenGitRefusesTheBranch`: the case passes through `DeleteSessionOutcome.RemovedWorktree`. Commit.
- [ ] 1.6 `RemoveWorktreeHandlerShould.AskWhenTheMergeCheckFails` (a `GitException` from `merge-base` reads as not merged and asks). Commit.

## 2. Launch

- [ ] 2.1 `LaunchServiceShould.ReportNotListedYetWhenTheListingFailsAfterTheStart` (the CLI's `agents` exits with an error) and `ReportNotListedYetWhenTheListingTimesOut`: `StartedAsync` catches `ClaudeCliException` and `ProcessTimedOutException` around the listing (design 4). Commit.
- [ ] 2.2 `LaunchServiceShould.CarryOnWhenTheRecentRepositoryCannotBeRecorded` (an `IRecentRepositories` stand-in that throws `IOException`; the diagnostic log holds one `Failed` naming `LaunchService` and no message text): `LaunchService` takes a `DiagnosticLog`, wired in `Composition\LaunchParts`. Commit.

## 3. Release

- [ ] 3.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges, and add a CHANGELOG `Unreleased` entry under Fixed: removing a merged worktree whose branch origin has deleted no longer reports a failure and deletes the branch, and a started session is never reported as a failed launch. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
