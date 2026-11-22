## Why

Every worktrees scan re-reads every repository under the roots from scratch: its worktree listing, its branch listing, origin's default branch and the commits on each unused branch, then each live worktree's status. With about 55 repositories that is some 300 git runs a scan, and in the measured hour a scan took 38 s at the median and 168 s at the 95th percentile, while the shown tab asks for one every 15 seconds and every heard change asks for another. Almost all of that work answers the same as last time: in a quarter of an hour perhaps two repositories gained a worktree, a branch or a commit. The folder watches already know which repository's git folder changed.

**Implement with:** Opus 5.5 at high effort: a per-repository cache whose correctness rests on file-system change events, ordering across overlapping scans and the existing "a scan's rows never replace a later scan's" rule; a missed invalidation shows stale rows, so the invalidation rules need careful, adversarial tests.

## What Changes

- A scan re-reads a repository's layout (its worktree listing, branch listing, origin's default branch, unused branches' commits and leftovers) only when that repository's git folder recorded a change since the last read, F5 was pressed, an action was taken, or five minutes have passed; every other repository's layout comes from the previous scan.
- Every scan still reads each live worktree's changed files and unpushed commits, so the CHANGES and UNPUSHED cells stay as fresh as today.
- A reference changing in a repository (a commit, a fetch, a branch made or deleted) marks that repository's layout for re-reading by the next scan but still starts no scan, so git's everyday work never makes the worktrees rescan.
- A watch that overflows marks every repository for re-reading.
- A merged pull request's "commits after its head" count is reused while neither the branch's tip nor the pull request's head has moved.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: a scan re-reads only the repositories that changed, with a five-minute full re-read as a safety net; references changing mark a repository for the next scan without starting one.

## Delivery

- **Order:** step 11 of the stack in `openspec\implementation-plan.md`. Depends on `lighter-git-reads` and `parallel-scan-reads`, in that order; nothing else should change the scan between them. Checkpoint B decides whether it goes ahead as written.
- **Together:** the cache, its invalidation by the watches and the five-minute safety net must ship in one pull request: the cache without the invalidation would show stale rows, and the invalidation without the cache saves nothing. `show-refresh-progress` follows it and builds its per-repository progress on this cache.

## Out of scope

- Watching the worktrees' working folders for file edits (the 15-second count of changes stays the way edits are noticed).
- The Sessions tab's enrichment.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\WorktreeInventory`: a per-repository layout cache with a stale set, read per scan; counts per live worktree every scan.
- `Application\ChangeFilter` and `ChangeWatches`: a change in a repository's git folder is reported with its repository, as either "rescan" (as today) or "stale only" (references, `packed-refs`, `FETCH_HEAD`); an overflow marks every repository.
- `Application\ConsoleLoads` and `Tui\ConsoleScreen`: F5 and an action's reload mark every repository stale before their scan.
- `Adapters\Git\GitBranchListing`: the tip's commit id (`%(objectname)`) for the merged-count key.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-screen.md`, Refreshing.
- **Implement with:** Opus 5.5 at high effort (reason above).
