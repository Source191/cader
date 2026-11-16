# Proposal

## Why

The Worktrees tab flags a branch `▲ left behind` when its pull request has merged and it has commits the pull request's head does not contain. It does not ask whether those commits' work is already on main, so it raises false alarms in two common cases, both seen on this repository on 16 November 2026:

- **Rebased before merging:** `c-dispose-tidy` read `14 commits after #320 merged`. The pull request was rebased before it merged, so the local branch still held the 14 commits from before the rebase. Thirteen are the same changes as commits on main, and the fourteenth, a version raise, was redone on main.
- **Reused after merging:** `c-multi-account` read `638 commits after #247 merged`. Its pull request merged long ago, and the branch was later moved forward to main and used again. Every commit on it is on main.

A flag that cries wolf trains people to ignore it, and the case it exists for (`c-new-bug`: a commit pushed after the merge that never reached main) looks the same.

## What Changes

- A branch is left behind only for commits after its pull request's merged head whose changes origin's default branch does not already have. A commit that is on origin's default branch, or whose changes match one there, does not count. Merge commits do not count either.
- The pane's count (`1 commit after #320 merged`) counts only those commits.
- When origin's default branch is not recorded in the repository (no `origin/HEAD`), the count is the one made today, so no left-behind branch is hidden.
- A branch row whose count falls to zero is no longer treated as holding commits at risk, as today for a count of zero.
- `docs\user-guide.md` says what counts.

Out of scope:
- Telling a commit redone on main by hand (a rebase that resolved a conflict, such as `c-dispose-tidy`'s version raise) from new work: its changes differ, so it still counts and the pane names it.
- Fetching, and judging against any branch but origin's default.
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `worktree-management`: "Left behind is judged against the merged head" counts only commits whose changes origin's default branch does not have, and keeps today's count when origin's default is not recorded.

## Impact

- **Adapters:** `GitCli.CountCommitsAfterAsync` lists the commits after the head and, when any are, the branch's commits whose changes origin's default branch lacks, and counts the commits in both; the `IGit` description says so. No port signature changes.
- **Tests:** new `GitCliIntegrationShould` facts in real repositories; the Core tests that script `rev-list` under the real `GitCli` follow the new commands.
- **Docs:** `docs\user-guide.md`'s BRANCH lines, `CHANGELOG.md` (Fixed) and a patch version raise.
