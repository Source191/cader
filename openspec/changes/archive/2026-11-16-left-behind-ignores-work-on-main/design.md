# Design

## Context

`WorktreeInventory.FlaggedAsync` asks `IGit.CountCommitsAfterAsync(repository, revision, head)` for each row whose shown pull request merged with a recorded head, and `BranchFlags.Of` flags left behind when the count is above zero. `GitCli` answers with `git rev-list --count <revision> --not <head>`, a count by commit identity. Core tests run the real `GitCli` over `FakeProcessRunner`, so they script these exact git arguments. Origin's default branch is already read elsewhere through `refs/remotes/origin/HEAD` (`DefaultRemoteBranchAsync`).

## Goals / Non-Goals

**Goals:**
- Count only commits after the merged head whose changes origin's default branch lacks, inside `GitCli`, so the port, the inventory, the flag rule and the pane keep their shape.

**Non-Goals:**
- Recognising a commit redone by hand on main (its patch differs).
- Caching counts between scans.

## Decisions

1. **Two git calls, intersected in `GitCli`:**
   - First `git rev-list <revision> --not <head>`, the commits after the head, as today but listed rather than counted. An empty list answers 0 at once, so a branch whose tip is its merged head (the usual case) costs one call, as today.
   - Then `git rev-list --no-merges --cherry-pick --right-only refs/remotes/origin/HEAD...<revision>`, the branch's commits that are not on origin's default branch and whose patch matches none there.
   - The count is the commits in both lists.
   - Checked on 16 November 2026 against this repository: `c-dispose-tidy` answers 1 (today 14), `c-multi-account` 0 (today 638), and `c-new-bug`'s stranded commit d6ed49cf 1 (today 1).
   - **Alternative rejected:** one call, `rev-list --cherry-pick ... ^<head>`. Excluding the head also removes from the left side the rebased copies the head reaches, so `--cherry-pick` has nothing to match the old commits against, and `c-dispose-tidy` would still read 14.
2. **Origin's default by its symbolic ref:**
   - `refs/remotes/origin/HEAD` names origin's default branch to git directly, so no extra lookup is needed and the port takes no new parameter.
   - When that ref is missing, the second call fails, and the count is the first list's length: today's answer.
   - This keeps a repository whose origin was added by hand (no `origin/HEAD` until `git remote set-head origin -a`) flagging as it does now rather than hiding work.
   - **Alternative rejected:** passing a `RemoteBranch` through `IGit` from the inventory. That needs a per-repository default for worktree rows too, which the inventory reads today only when there are branch rows, all to name a ref git already resolves.
3. **Merge commits never count:** `--no-merges` on the second list drops them. A merge adds no change of its own beyond its parents', whose own commits are judged one by one.
4. **The revision guard stays:** only a full ref name (`refs/...`) is given to git, as today, and the head goes through `CommitId`, so nothing from a listing becomes an option.

## Risks / Trade-offs

- **[Cost of `--cherry-pick`]:** git works out a patch id for each commit on both sides since the merge base, which is slow for a branch whose merge base with main is old and which also has commits after its head. Mitigation: the second call runs only when the first list is not empty, and only for rows whose pull request merged with a recorded head.
- **[A redone commit still counts]:** a commit rebased with a conflict resolved (such as `c-dispose-tidy`'s version raise) has a different patch on main, so a branch rebased that way still reads left behind with a small count. The pane names the commits, and `d` and `D` keep their rules. Accepted in the proposal.
- **[Core fixtures follow git's arguments]:** the facts that script `rev-list --count <revision> --not <head>` must script the two new calls. Their expected counts stay the same, so the flag rules they check are unchanged.
