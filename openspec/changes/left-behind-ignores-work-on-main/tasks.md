# Tasks

Each task is red, green, refactor with a commit per step. "Verify" means `dotnet build -c Debug` (warnings are errors) and the named tests pass.

## 1. The count in git (Adapters)

- [ ] 1.1 **Red:** `GitCliIntegrationShould.CountNoCommitsRebasedOntoMainBeforeTheMerge`, in a clone of a bare origin with `origin/HEAD` set: a branch with two commits, then main given rebased copies of them (`git cherry-pick`) pushed to origin, and the merged head the second copy; `CountCommitsAfterAsync` answers 0 for the branch. **Green:** `GitCli.CountCommitsAfterAsync` lists `rev-list <revision> --not <head>`, answers 0 when it is empty, otherwise lists `rev-list --no-merges --cherry-pick --right-only refs/remotes/origin/HEAD...<revision>` and answers how many commits are in both. **Verify:** the fact and every existing `CountCommitsAfterAsync` fact pass.
- [ ] 1.2 **Red:** `GitCliIntegrationShould.CountNoCommitsOfABranchMovedForwardToMainAfterItsMerge`: a branch merged at an older head, then reset to `origin/main` after main gained commits; the count is 0. **Green** if 1.1 does not already pass it. **Verify:** the fact passes.
- [ ] 1.3 **Red:** `GitCliIntegrationShould.CountOnlyTheWorkAfterTheMergeThatMainLacks`: after the merged head, a merge of main (which has gained a commit) into the branch and one new commit; the count is 1, where today's count is 3 (main's commit, the merge and the new one). **Green** if needed. **Verify:** the fact passes.
- [ ] 1.4 **Red:** `GitCliIntegrationShould.CountEveryCommitAfterTheHeadWhenOriginsDefaultIsNotRecorded`: the same branch as 1.3 after `git remote set-head origin -d`; the count is 2, today's answer. **Green** if needed: a failed second call answers the first list's length. **Verify:** the fact passes, and `AnswerNoCountForAHeadGitHasNotGotOrARevisionThatIsNotARef` still answers null for an unknown head and for a revision that is not a full ref name.
- [ ] 1.5 Update the `IGit.CountCommitsAfterAsync` description to say that commits whose changes origin's default branch has, and merge commits, are not counted unless origin's default is not recorded. **Verify:** `dotnet build -c Debug`.

## 2. The scripted git in Core's tests

- [ ] 2.1 Change `WorktreeInventoryBranchesShould`'s two exact `rev-list --count <revision> --not <head>` answers (the `c-new-bug` origin row and `fix-123`) to answer the two new calls with the same counts as commit lists, keeping every expected flag and count. **Verify:** `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` passes, including the facts that answer any `rev-list` with a count, which must still flag nothing left behind.

## 3. Documentation and release

- [ ] 3.1 Update `docs\user-guide.md`: BRANCH's `▲ left behind` line and the paragraph after the table say that only commits after the merge whose changes origin's default branch lacks count, so a branch rebased before merging or reused after it is not left behind. **Verify:** the guide's words match the spec delta's requirement.
- [ ] 3.2 Raise `Version` with `.github\scripts\Step-Version.ps1` (a fix: patch, or the next prerelease number while the version is a preview) and add a `CHANGELOG.md` line under `## Unreleased`, `### Fixed`: a branch rebased before its pull request merged, or reused after it, no longer reads left behind. **Verify:** `Assert-VersionBumped.ps1 -Base origin/main -Head HEAD` and `Assert-ChangelogUpdated.ps1 -Base origin/main -Head HEAD` pass.
- [ ] 3.3 Tick track-branches' 5.5 in its archived `tasks.md` (the owner checked on 16 November 2026 that `⑂`, `○` and `▲` take one cell each). **Verify:** the archived task reads `[x]`.

## 4. Integration

- [ ] 4.1 Run `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes` and `openspec validate --all`. **Verify:** each exits 0.
- [ ] 4.2 **Check (owner):** install with `tools\install-local.ps1` and open the Worktrees tab over this repository: `c-mem-leak` (on `c-dispose-tidy`) reads at most `1 commit after #320 merged`, `c-multi-account` is not left behind, and the gone, closed and no PR flags read as before. **Verify:** by eye.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` matches `main`.
