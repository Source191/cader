# Tasks

**Implement with:** Opus 5.5 at medium effort: one pure Domain rule, two adapter fields and one git query, all behind existing ports and caught by the sociable and real-git tests.

Each task is red, green, refactor, with a commit per step. Tests are sociable: the catalogue's tests keep the real `GitCli` over `FakeProcessRunner` and `FakePullRequests`, and the git query is also checked against a real repository in a temporary folder.

## 1. Domain

- [x] 1.1 Add `Domain\RecordedTime` (a plain record with no behaviour of its own, so no test of its own) (`At(DateTimeOffset When)`, `NotRecorded`, shaped like `HeadCommit`) and `Finished` on `PullRequest`, updating every construction site to pass `new RecordedTime.NotRecorded()`. Verify `dotnet build -c Debug` and the Core suite pass unchanged (`ClosedHierarchiesShould` and `NullFreeModelsShould` included). Commit.
- [x] 1.2 `PullRequestsShould.LeaveOutAPullRequestThatFinishedBeforeTheBranchWasCreated` (the #371 scenario: answers `Missing(NoPullRequest)`), `KeepAPullRequestThatFinishedAfterTheBranchWasCreated`, `ShowTheNewerPullRequestOfAReusedName` (#371 dropped, #380 shown), `NeverLeaveOutAnOpenPullRequest` (open and draft, opened before the branch), `LeaveOutAnAbandonedPullRequestThatFinishedBeforeTheBranch`, `KeepEverythingWhenTheBranchsCreationIsNotRecorded`, `KeepAFinishedPullRequestWithNoRecordedFinish` and `KeepAPullRequestThatFinishedInTheSameSecond`: `PullRequests.ForBranch(forBranch, branchCreated)` (design 3), with `PullRequestCatalogue` passing `new RecordedTime.NotRecorded()` for now. Verify the facts pass and the existing `PullRequestsShould` facts still do. Commit.

## 2. Adapters

- [x] 2.1 `GitHubPullRequestsShould.ReadWhenAMergedPullRequestClosed` (`closedAt` 2026-11-21T14:28:23Z on #371 gives `At` that time in UTC), `RecordNoFinishForAnOpenPullRequest`, `RecordNoFinishWhenClosedAtIsMissingOrBlank` and `AskForClosedAt` (the request's `--json` list holds `closedAt`): `GitHubPullRequests` reads `closedAt` for merged and closed states only (design 2). Verify they pass. Commit.
- [x] 2.2 `AzureDevOpsPullRequestsShould.ReadWhenACompletedPullRequestClosed` (`closedDate` 2026-11-20T10:05:00Z on 4521), `ReadWhenAnAbandonedPullRequestClosed`, `RecordNoFinishForAnActivePullRequest` (even with a `closedDate` present) and `RecordNoFinishWhenClosedDateIsMissing`: `AzureDevOpsPullRequests` reads `closedDate` for completed and abandoned only. Verify they pass. Commit.
- [x] 2.3 `GitCliShould.AnswerTheBranchsCreationFromTheReflog` (a canned `branch: Created from origin/main` last line with `@{1795338771}` answers 2026-11-22T09:12:51Z), `AnswerNothingWhenTheOldestEntryIsNotACreation` (`clone: from ...`), `AnswerNothingForNoReflogOrAFailure`, `AnswerNothingForAnUnreadableLine` and `AskForTheFullRefName` (`refs/heads/<branch>`, never a bare name): `IGit.BranchCreatedAtAsync(FolderPath, BranchName, CancellationToken)` and `GitCli` (design 1), with its `NullFreeModelsShould` `Kept` entry ("git answers nothing when the reflog does not record the branch's creation"). Verify they pass. Commit.
- [x] 2.4 `GitCliIntegrationShould.ReadWhenABranchWasCreated` against a temporary repository: a branch from `git branch`, one from `git worktree add -b` (asked from the main checkout), one renamed with `git branch -m`, and `main` after `git init` (answers null, its oldest entry being a commit, not a creation); each created branch's answer within a few seconds of the test's own clock. Verify it passes. Record the reflog fact (subjects, `--date=unix`, the oldest line, rename, git version) in `.claude\rules\windows.md`'s Windows process facts with today's date. Commit.

## 3. Catalogue

- [ ] 3.1 `PullRequestCatalogueShould.LeaveOutAReusedBranchsMergedPullRequest` (the reflog answers a creation after #371's close: `Find` answers `Missing(NoPullRequest)`), `KeepThePullRequestWhenTheReflogRecordsNoCreation` and `ReadTheBranchsCreationWithEachLookup` (a second lookup after the window, with the branch recreated, follows the new answer): `RefreshAsync` asks `BranchCreatedAtAsync` beside the listing, keeps it in `Answer`, and `Find` passes it to `ForBranch` (design 4). Adjust any existing fact that counts captured git requests to expect the reflog read. Verify the Core suite passes. Commit.
- [ ] 3.2 `SessionRowEnricherShould` and `WorktreeInventoryShould` gain one fact each that a reused branch's row shows no pull request and, on Worktrees, the · no PR flag (`BranchFlagKind.NoPullRequest`). Verify both pass. Commit.

## 4. Documentation and release

- [ ] 4.1 `docs\user-guide.md`: the PR column's line says a finished pull request from before the branch was created is not shown, so a reused branch name starts blank. `CHANGELOG.md` Unreleased, Fixed: a branch name reused for new work no longer shows the earlier branch's merged or closed pull request. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` as CONTRIBUTING's Versions says for a fix. Verify `Assert-VersionBumped.ps1` passes and `SpellBritish` holds.
- [ ] 4.2 Full check: `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes` and `openspec validate ignore-reused-branch-pull-requests`; all pass.
- [ ] 4.3 Manual check with a local build (`tools\install-local.ps1`): the `c-status` worktree's row shows a blank PR column and Worktrees flags it · no PR; a worktree whose pull request merged after it was created still shows it merged.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs\pull-requests\spec.md` gains both requirements.
