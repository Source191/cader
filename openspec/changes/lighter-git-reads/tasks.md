# Tasks

**Implement with:** Sonnet 5.5 at medium effort: four small, independent changes in `GitCli`, `WorktreeInventory` and `SessionRowEnricher`, each pinned by an existing real-git adapter test or sociable scan test, with no threading or design choices left open.

Each task is red, green, refactor, with a commit per step. Real-git facts use `TemporaryGitRoot`; scan facts use the existing `WorktreeInventory*Should` fixtures and `FakeTimeProvider`.

## 1. Optional locks off

- [ ] 1.1 `GitCliShould.TurnOptionalLocksOffForEveryRun`: every request `GitCli` hands the process runner carries `GIT_OPTIONAL_LOCKS=0`, merged with a fetch's own `GIT_TERMINAL_PROMPT` and `GCM_INTERACTIVE`. Verify the fact fails before the change. Commit.
- [ ] 1.2 `GitCliIntegrationShould.LeaveTheIndexUnwrittenByStatus` (a modified file in a scratch repository: the index's write time is unchanged after `CountChangedFilesAsync`) and `NotCountATouchedUnchangedFile`. Record the observed behaviour with git's version in `.claude\rules\windows.md`. Commit.

## 2. Email read once per ten minutes

- [ ] 2.1 `GitCliShould.ReadTheEmailOncePerRepositoryForTenMinutes` and `ReadTheEmailAgainAfterTenMinutes` over `FakeProcessRunner` and `FakeTimeProvider`; `GitCli` takes a `TimeProvider` (the composition passes `TimeProvider.System`; `SceneHost` and the test fixtures follow). Commit.
- [ ] 2.2 `GitCliShould.NotKeepAFailedEmailRead`: a failed `config --get` is asked again on the next listing. Commit.

## 3. Last commit from the branch listing

- [ ] 3.1 `WorktreeInspectorShould.CountWithoutTheLastCommit`: the new count runs `status` and `rev-list` only. Commit.
- [ ] 3.2 `WorktreeInventoryRepositoriesShould.TakeAWorktreesLastCommitFromItsBranchTip` (no `log -1` for a worktree on a listed branch), `ReadTheLastCommitOfADetachedHead` and `ReadTheLastCommitWhenTheBranchListingFailed`, asserting the requests the fake runner saw. Commit.

## 4. One rev-parse per session folder

- [ ] 4.1 `GitCliIntegrationShould.LocateATopLevelAndItsCommonDirectoryInOneRun` in a linked worktree and in a main checkout, and `AnswerNullOutsideARepository`: the new `IGit` query. Commit.
- [ ] 4.2 `SessionRowEnricherShould.LocateAFolderWithTwoGitRuns`: the rows are as before and the fake runner saw one `rev-parse` and one `branch --show-current` per uncached folder. Commit.

## 5. Release

- [ ] 5.1 `.claude\rules\console-screen.md`, Refreshing: the scan's reads per worktree and repository. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of #379 and add a CHANGELOG `Unreleased` entry under Changed (fewer git runs per scan; Cader's reads no longer lock a worktree's index while a session commits). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate lighter-git-reads`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
