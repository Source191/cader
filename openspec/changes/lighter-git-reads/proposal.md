## Why

In an hour of real use (21 November 2026, about 55 repositories under the roots) Cader started about 12,700 git processes, and because they compete with each other and with the sessions' own work, each took 550 to 1,000 ms against about 100 ms on a quiet machine. A quarter of those runs answer a question Cader already has the answer to: `git config --get user.email` runs with every branch listing (2,255 an hour) though it almost never changes, `git log -1` runs for every worktree (894 an hour) though the branch listing beside it already holds each branch tip's commit time, and each session folder costs two `git rev-parse` runs where one gives both answers. Worse, Cader's `git status` refreshes and rewrites the worktree's index and takes `index.lock` while it does, so a session committing in that worktree at the same moment can fail with "index.lock exists".

**Implement with:** Sonnet 5.5 at medium effort: four small, independent changes in `GitCli`, `WorktreeInventory` and `SessionRowEnricher`, each pinned by an existing real-git adapter test or sociable scan test, with no threading or design choices left open.

## What Changes

- Every git read Cader runs takes none of git's optional locks, so `git status` no longer writes the index or takes `index.lock` (git's own setting for programs that read in the background).
- A repository's `user.email` is read at most once every ten minutes, not with every branch listing.
- A worktree on a branch takes its last commit time from the branch listing already read for its repository; only a worktree with a detached or unknown head runs `git log -1`.
- A session folder is located with one `git rev-parse` asking for both the top level and the common git directory, then `git branch --show-current`: two runs, not three.
- No change to what any row, column or pane shows.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: Cader's git reads take no optional locks, and a scan reuses answers it already holds (the user's email, a branch tip's time).

## Delivery

- **Order:** step 5 of the stack in `openspec\implementation-plan.md`. Depends on nothing. Must land before `parallel-scan-reads` and `rescan-only-what-changed`, which change the same methods. It also covers the index lock that `guard-overlapping-actions` first proposed to fix with `--no-optional-locks` on each read.
- **Together:** its four parts ship as one pull request; none needs another change to be useful.

## Out of scope

- Running any git read in parallel (`parallel-scan-reads`) or skipping repositories that did not change (`rescan-only-what-changed`).
- Changing which facts a row shows, or the scan's ordering rules.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Adapters\Git\GitCli`: `GIT_OPTIONAL_LOCKS=0` in the variables of every run; the email cache (ten minutes per repository, through a `TimeProvider`, kept inside the adapter so the email still never leaves it); one `rev-parse` with `--show-toplevel` and `--git-common-dir`.
- `Application\WorktreeInventory` and `WorktreeInspector`: the last commit from the branch listing, with `git log -1` only for a head the listing does not cover.
- `Application\SessionRowEnricher`: one located answer from the combined read.
- `Ports\IGit`: one combined location query; the two old ones stay while other callers use them.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-screen.md`, Refreshing.
- **Implement with:** Sonnet 5.5 at medium effort (reason above).
