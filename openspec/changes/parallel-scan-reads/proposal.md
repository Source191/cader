## Why

Several of Cader's reads wait on each other for no reason. The first worktrees scan starts no git until the Sessions tab's first `claude agents` listing returns (2.7 s at the median), though it only needs the sessions at the very end to mark which worktree has one. Inside a scan, each worktree's three git reads hold one slot one after another, each repository's unused branches are counted one at a time, and the merged-pull-request check runs row by row outside the slot pool altogether. Session enrichment locates one folder at a time (95th percentile 6.2 s, longest 55 s in the measured hour). On a machine where each git run takes 0.5 to 1 s under load, these queues are most of the waiting.

**Implement with:** Opus 5.5 at high effort: it changes concurrency in the scan and the enrichment, where the merge order, the orphan rule ("judged against its own registrations and those before it"), cancellation and the eight-slot limit must all hold, and mistakes show as rare, order-dependent failures.

## What Changes

- The first worktrees scan starts its git reads at once and waits for the first sessions listing only to mark each worktree's session; its rows still never show sessions from before that listing.
- Within a scan, every git read goes through the same pool of eight: a worktree's status, unpushed count and last commit run as separate reads, a repository's unused branches are counted in parallel, and the merged-pull-request check runs in the pool, not row by row.
- Session enrichment locates up to eight folders at once.
- The rows, their order and every rule about which rows appear are exactly as today.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: the start-up scan no longer waits for the first sessions listing before reading git; it waits only to fill in the sessions.

## Delivery

- **Order:** 5 of 11, the first of wave 2 (the scan). Depends on `lighter-git-reads`, which removes reads this change would otherwise parallelise.
- **Together:** must land before `rescan-only-what-changed`, which builds its per-repository cache on the reads this change restructures; do them one after the other with nothing else on the scan in between.

## Out of scope

- Skipping repositories that did not change (`rescan-only-what-changed`).
- Raising the pool above eight; the measured contention says fewer runs, not more at once.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\WorktreeInventory`: sessions awaited at the merge; `FlaggedAsync`, `ListedBranchesAsync` and the per-worktree count through `InSlotAsync`.
- `Application\WorktreeInspector`: the count's reads run together when given a slot pool.
- `Application\ConsoleLoads.ScanWorktreesAsync` and `LatestSessions`: the scan starts before the first listing and hands the inventory a task of sessions.
- `Application\SessionRowEnricher`: folders located through a pool of eight, the rows kept in listing order.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-screen.md`, Refreshing.
- **Implement with:** Opus 5.5 at high effort (reason above).
