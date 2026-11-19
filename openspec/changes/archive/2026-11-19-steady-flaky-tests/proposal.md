# Proposal

## Why

Three integration facts fail now and then on CI for reasons in the tests, not in Cader, so a red run has to be re-run before anyone can tell whether it means anything.

- **`WorktreeInventoryShould.CarryEachRowsPullRequest`** (failed on #347, run 44481237017) and **`WorktreeInventoryBranchesShould.LookUpABranchOfYoursOnlyOnOriginButListNoRowForItUntilItsAnswerLands`** (failed once locally while building #346): a scan starts its pull request lookups without awaiting them, by design, and reads the catalogue as it builds the rows. The fake host answers at once, so the answer can land before the rows are read, and the first scan then shows the pull request the fact expects it not to have yet.
- **`GitWorktreeRemoveShould.DropTheRegistrationAndEmptyTheFolderWhenAProgramHoldsIt`** (failed on main in runs 44281853616 and 44379308100 and on #348 in 44482091266): the fact holds the worktree's folder with `cmd /c ping`, kills the tree and waits only for `cmd`, so `ping.exe` can still hold the folder as its working directory when the fixture deletes it (`IOException`, "being used by another process").

## Benefit

- **Maintainers:** these three facts no longer fail by chance, so a red pipeline means a real problem and no pull request waits on a re-run.

## What Changes

- **The two lookup facts** hold the fake host's answers (`FakePullRequests.Hold`) until the scan has returned, then release them, so the first scan always sees the lookup still running, as the facts mean.
- **The removal fact** holds the folder with `ping.exe` itself rather than through `cmd`, so killing the one process and waiting for it leaves nothing holding the folder.

Out of scope:

- Any change to the scan, the catalogue or the worktree removal.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort: three small test edits whose causes are already found.
- **Tests:** `test\Source191.Cader.Infrastructure.Tests\WorktreeInventoryShould.cs`, `GitWorktreeRemoveShould.cs`, `test\Source191.Cader.Core.Tests\Application\WorktreeInventoryBranchesShould.cs`.
- **Version and changelog:** none; nothing under `src` changes.
