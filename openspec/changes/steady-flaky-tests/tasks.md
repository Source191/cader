# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort.

## 1. The lookup facts

- [x] 1.1 `WorktreeInventoryShould.CarryEachRowsPullRequest`: `hosts.Hold()` before the first scan and `hosts.Release()` after it; verify it passes 20 times in a row.
- [x] 1.2 `WorktreeInventoryBranchesShould.LookUpABranchOfYoursOnlyOnOriginButListNoRowForItUntilItsAnswerLands`: the same hold, released after the assertions; verify it passes 20 times in a row.

## 2. The removal fact

- [x] 2.1 `GitWorktreeRemoveShould.StartSleeperIn` starts `ping.exe` directly (output redirected and left unread, as 60 lines fit the pipe); verify the fact passes 20 times in a row.

## 3. Check

- [x] 3.1 Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request and verify that `openspec validate` passes.
