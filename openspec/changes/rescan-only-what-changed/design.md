## Context

See proposal.md, Why. After `lighter-git-reads` and `parallel-scan-reads`, a scan classifies each root's children on disc, then per repository reads `worktree list`, `for-each-ref` (with the cached email), `symbolic-ref` when there are unused or origin-only branches and `rev-list` per unused branch, and per live worktree `status` and `rev-list`; then it merges in name order, adds orphans and leftovers, sets sessions and pull requests and counts commits after a merged pull request's head. `ChangeWatches` keeps one watch per planned folder (a repository's `.git` with subfolders) and calls the tab's `RefreshSchedule.Changed()` for each change `ChangeFilter.Counts` accepts.

## Goals / Non-Goals

**Goals:** a scan costs about two git runs per live worktree plus a full read of only the repositories that changed; rows identical to a full scan's.

**Non-Goals:** fewer reads per live worktree; watching working folders.

## Decisions

### 1. Two tiers: a repository's layout and a worktree's counts

A repository's layout is everything the scan reads per repository (the worktree entries, the branch listing with tips and upstreams, origin's default branch, each unused branch's commits on no remote, the orphans under `.claude\worktrees`) and is kept in a per-repository cache inside `WorktreeInventory`, each entry with when it was read. A live worktree's counts (changed files, unpushed commits) are read on every scan. The merge, orphan and leftover rules run over the cached and fresh layouts exactly as today, so a scan's rows are what a full scan gives.

### 2. What makes a layout stale

A repository's layout is read again when any of these holds, else it is taken from the cache:

| Cause | Comes from |
| --- | --- |
| No entry yet, or the entry is five minutes old | the scan's clock |
| Its git folder recorded a worktree record or `HEAD` change (as today, also starting a scan) | `ChangeWatches` |
| Its git folder recorded a reference change: anything under `refs`, `packed-refs`, `FETCH_HEAD` (no scan starts) | `ChangeWatches` |
| A watch overflowed | `ChangeWatches`, every repository |
| F5, or the reload after an action | the console, every repository |

The stale set is written from the watchers' pool threads and taken whole by the scan as it starts, behind a `System.Threading.Lock` (a take-and-clear, which a `SharedSnapshot` alone cannot do atomically), so a change heard during a scan stays for the next one. Reference changes mark only, because they follow git's everyday work, Cader's own origin prune included, and starting a scan for them would make every scan start the next (`ChangeFilter`'s existing reason).

Why five minutes: a safety net for a change a watch never reports (a network drive, a folder Windows refused to watch, recorded as `WatchTrouble`) costing one full scan in twenty at the default interval. Alternative considered: no safety net; rejected because a missed event would then leave a row wrong until the user presses F5.

### 3. Overlapping scans

Only one scan runs at a time (`RunningLoads`), so the cache has one writer. A scan stores each repository's layout as it is read; the reload after an action, which may overlap a timed scan through `Track`, marks every repository stale before it starts, so it never reuses a layout the earlier scan read before the action.

### 4. Merged counts keyed by tip and head

`GitBranchListing` gains `%(objectname)`. The count of commits after a merged pull request's head is cached by repository, branch revision, tip commit and head commit, so it is read again only when one of those moves.

## Risks / Trade-offs

- [A change that git records somewhere this design does not watch leaves a row stale] → the five-minute safety net and F5; the facts in tasks 2.x list every git command Cader's rows depend on and the file each writes (worktree add, remove, prune, switch, commit, fetch, branch create and delete, rebase), checked against real git in `TemporaryGitRoot`.
- [More memory for the cache] → one layout per repository, a few kilobytes each; checked with an `Allocations` budget for a rescan with no change.
- [A rename of `.git\packed-refs` during `git pack-refs` arrives as a delete and a create] → both mark stale only.

## Migration Plan

None: the cache starts empty, so the first scan after an update is a full one.
